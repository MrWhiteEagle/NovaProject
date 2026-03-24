using System.Collections.Concurrent;
using System.IO.Pipes;
using System.Text.Json;
using NovaProject.Core.Infrastructure.Daemon;
using NovaProject.Core.Services;

namespace NovaProject.Daemon.Services;

public class DaemonWorker : BackgroundService
{
    private ConcurrentQueue<DaemonServerResponse> ResponseQueue { get; } = new();
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Logger.LogInfo("Daemon Worker listening....");
        //Main loop
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                //Knock knock, UI
                await using var pipeServer = new NamedPipeServerStream("NovaProject_Pipe", PipeDirection.InOut);
                await pipeServer.WaitForConnectionAsync(stoppingToken);

                using (var reader = new StreamReader(pipeServer, leaveOpen: true))
                {
                    var rawJson =  await reader.ReadLineAsync(stoppingToken);
                    if (!string.IsNullOrEmpty(rawJson) && !string.IsNullOrWhiteSpace(rawJson))
                    {
                        try
                        {
                            var request = JsonSerializer.Deserialize<DaemonServerRequest>(rawJson);
                            HandleRequest(request);
                        }
                        catch (Exception e)
                        {
                            Logger.LogError(e.ToString());
                        }
                    }
                }

                await using (var writer = new StreamWriter(pipeServer, leaveOpen: true))
                {
                    while(ResponseQueue.TryDequeue(out var response))
                    {
                        var payload = JsonSerializer.Serialize(response);
                        Logger.LogInfo("Daemon Worker sent " + response.ResponseType);
                        await writer.WriteLineAsync(payload);
                    }
                }
            
            

                await pipeServer.FlushAsync(cancellationToken: stoppingToken);
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
                case DaemonRequestType.MessageUser:
                    break;
                case DaemonRequestType.ReceiveUserMessage:
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
}