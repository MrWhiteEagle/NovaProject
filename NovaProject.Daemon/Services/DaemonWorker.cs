using System.Collections.Concurrent;
using System.IO.Pipes;
using System.Text.Json;
using NovaProject.Core.Infrastructure.Local;
using NovaProject.Core.Infrastructure.Ui.Daemon;
using NovaProject.Core.Services;

namespace NovaProject.Daemon.Services;

public class DaemonWorker(DaemonDataService dataService) : BackgroundService
{
    private ConcurrentQueue<DaemonServerResponse> ResponseQueue { get; } = new();
    private DaemonDataService _dataService { get; } = dataService;
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Logger.LogInfo("Daemon Worker waiting for handshake...");
        //Main loop
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                //Knock knock, UI
                await using var pipeServer = new NamedPipeServerStream("NovaProject_Pipe", PipeDirection.InOut);
                await pipeServer.WaitForConnectionAsync(stoppingToken);

                using var reader = new StreamReader(pipeServer, leaveOpen: true);
                using var writer = new StreamWriter(pipeServer, leaveOpen: true) { AutoFlush = true };

                var readTask = Task.Run(async () =>
                {
                    while (!stoppingToken.IsCancellationRequested && pipeServer.IsConnected)
                    {
                        var line = await reader.ReadLineAsync();
                        if (line == null) break;
                        var data = JsonSerializer.Deserialize<DaemonServerRequest>(line);
                        HandleRequest(data);
                    }
                });

                var writeTask = Task.Run(async () =>
                {
                    while (!stoppingToken.IsCancellationRequested && pipeServer.IsConnected)
                    {
                        if (ResponseQueue.TryDequeue(out var response))
                        {
                            await writer.WriteLineAsync(JsonSerializer.Serialize(response));
                            await writer.FlushAsync();
                            await pipeServer.FlushAsync(stoppingToken);
                            Logger.LogInfo("Daemon worker sent:  " + response.ResponseType);
                        }
                        else
                        {
                            await Task.Delay(100, stoppingToken);
                        }
                    }
                });
                
                await Task.WhenAll(readTask, writeTask);
                await pipeServer.FlushAsync(cancellationToken: stoppingToken);
                Logger.LogInfo("Daemon connection closed");
                pipeServer.Disconnect();
            }
            catch (OperationCanceledException)
            {
                Logger.LogInfo("Daemon worker stopped, cancelling operations...");
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                //Rapid fire prevention
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    private void HandleRequest(DaemonServerRequest? request)
    {
        if (request == null)
        {
            Logger.LogError("Request is null");
        }
        else
        {
            Logger.LogInfo("Daemon Responding to " + request.RequestType);
            switch (request.RequestType)
            {
                case DaemonRequestType.LoadServerList:
                    RespondToServerListRequest();
                    break;
                case DaemonRequestType.LoadLocalUserList:
                    RespondToUserListRequest();
                    break;
                case DaemonRequestType.MessageOutbound:
                    RespondToPrivateMessageOutgoing(request);
                    break;
                case DaemonRequestType.MessageInbound:
                    break;
                case DaemonRequestType.LoadUserConversation:
                    //Needs to be Task.Run in order to run it asynchronously without needing the whole HandleBlock to be
                    Task.Run(() => HandleLoadingUserConversation(request));
                    break;
                case DaemonRequestType.Ping:
                    RespondToPingRequest();
                    break;
                    
            }
        }
    }

    #region Response Methods
    private void RespondToPingRequest()
    {
        var response = new DaemonServerResponse(DaemonResponseType.PingSuccess, DateTime.Now.ToString("HH:mm:ss"));
        ResponseQueue.Enqueue(response);
    }

    private void RespondToServerListRequest()
    {
        var response = new DaemonServerResponse(DaemonResponseType.LoadServerList,
            JsonSerializer.Serialize(PlaceHolderData.PlaceHolderServerData));
        ResponseQueue.Enqueue(response);
    }

    private void RespondToUserListRequest()
    {
        var response = new DaemonServerResponse(DaemonResponseType.LoadUserList,
            JsonSerializer.Serialize(PlaceHolderData.PlaceholderUserData));
        ResponseQueue.Enqueue(response);
    }

    private void RespondToPrivateMessageOutgoing(DaemonServerRequest request)
    {
        ForwardMessageToRelay(request);
        //DEBUG PART - REBOUND MESSAGE BACK TO UI
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var payload = JsonSerializer.Deserialize<MessageData>(request.Payload, options);
            Task.Run(() => _dataService.SaveMessage(payload));
            //TODO: implement a notification for the UI that the message was sent correctly to relay and the relay received it
            if (payload.Sender == null || payload.Recipient == null)
            {
                Logger.LogError("CRITICAL: MessageData deserialized but Sender/Recipient are NULL!");
                Logger.LogInfo($"Check the JSON keys in this payload: {request.Payload}");
                return;
            }
            var newMessage = new MessageData(payload.Recipient, payload.Sender, "REBOUND MESSAGE: " + payload.Content, direction: MessageData.MessageDirection.Incoming);
            Logger.LogInfo($"Rebounding message from {newMessage.Sender.UserName ?? "Unknown"} to {newMessage.Recipient.UserName ?? "Unknown"}");
            var newPayload = JsonSerializer.Serialize(newMessage);
            var rebounce = new DaemonServerResponse(DaemonResponseType.MessageInbound, newPayload);
            ResponseQueue.Enqueue(rebounce);
        }
        catch (Exception e)
        {
            Logger.LogError("Daemon failed to process Outgoing message " + e.Message);   
        }
    }

    private void RespondToPrivateMessageIncoming(DaemonServerRequest request)
    {
        //TODO: Implement once the server project is ready
        
        // var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        // var payload = JsonSerializer.Deserialize<MessageData>(request.Payload, options);
    }

    private async Task HandleLoadingUserConversation(DaemonServerRequest? request)
    {
        var dbResult = await _dataService.GetMessageHistory(request.Payload);
        var response = new DaemonServerResponse(DaemonResponseType.LoadUserConversation, JsonSerializer.Serialize(dbResult));
        ResponseQueue.Enqueue(response);
    }

    private void ForwardMessageToRelay(DaemonServerRequest request)
    {
        Logger.LogCritical("SENDING PRIVATE MESSAGE TO RELAY SERVER");
    }
    #endregion
}