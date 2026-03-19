using System.Collections.Concurrent;
using System.IO.Pipes;
using System.Text.Json;
using NovaProject.Core.Infrastructure;
using NovaProject.Core.Infrastructure.Daemon;
using NovaProject.Core.Services;

namespace NovaProject.Daemon.Services;

public class DaemonWorker : BackgroundService
{
    private ConcurrentQueue<DaemonServerResponse> _responseQueue { get; } = new();
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
                Logger.LogInfo("Daemon Worker waiting for connection...");

                using (var reader = new StreamReader(pipeServer, leaveOpen: true))
                {
                    var rawJson =  await reader.ReadLineAsync(stoppingToken);
                    if (!string.IsNullOrEmpty(rawJson) && !string.IsNullOrWhiteSpace(rawJson))
                    {
                        Logger.LogInfo("Daemon Worker received: " + rawJson);
                        try
                        {
                            var request = JsonSerializer.Deserialize<DaemonServerRequest>(rawJson);
                            await HandleRequest(request);
                        }
                        catch (Exception e)
                        {
                            Logger.LogError(e.ToString());
                        }
                    }
                }

                using (var writer = new StreamWriter(pipeServer, leaveOpen: true))
                {
                    while(_responseQueue.TryDequeue(out var response))
                    {
                        var payload = JsonSerializer.Serialize(response);
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

    private async Task HandleRequest(DaemonServerRequest? request)
    {
        if (request == null)
        {
            Logger.LogError("Request is null");
        }
        else
        {
            switch (request.RequestType)
            {
                case DaemonRequestType.LoadServerList:
                    Logger.LogInfo("Loading server list");
                    break;
                case DaemonRequestType.MessageUser:
                    Logger.LogInfo("MessageUser");
                    break;
                case DaemonRequestType.ReceiveUserMessage:
                    Logger.LogInfo("ReceiveUserMessage");
                    break;
                case DaemonRequestType.Ping:
                    Logger.LogInfo("Pong");
                    await RespondToPingRequest();
                    break;
                    
            }
        }
    }

    private async Task RespondToPingRequest()
    {
        var response = new DaemonServerResponse(DaemonResponseType.PingSuccess, DateTime.Now.ToString("HH:mm:ss"));
        _responseQueue.Enqueue(response);
    }
}