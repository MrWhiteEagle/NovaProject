using System.Collections.Concurrent;
using System.IO.Pipes;
using System.Text.Json;
using NovaProject.Core.Infrastructure.Daemon;
using NovaProject.Core.Infrastructure.Structs;
using NovaProject.Core.Services;

namespace NovaProject.Daemon.Services;

public class DaemonWorker : BackgroundService
{
    private ConcurrentQueue<DaemonServerResponse> ResponseQueue { get; } = new();
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
                case DaemonRequestType.Ping:
                    RespondToPingRequest();
                    break;
                    
            }
        }
    }

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

            // NULL GUARD: This is where your crash was happening
            if (payload.Sender == null || payload.Recipient == null)
            {
                Logger.LogError("CRITICAL: MessageData deserialized but Sender/Recipient are NULL!");
                Logger.LogInfo($"Check the JSON keys in this payload: {request.Payload}");
                return;
            }
            var newMessage = new MessageData("REBOUND MESSAGE: " + payload.Message, payload.Recipient, payload.Sender);
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
    
    private void RespondToPrivateMessageIncoming()
    {
        
    }

    private void ForwardMessageToRelay(DaemonServerRequest request)
    {
        Logger.LogCritical("SENDING PRIVATE MESSAGE TO RELAY SERVER");
    }
}