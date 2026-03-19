using System.IO.Pipes;
using System.Text.Json;
using NovaProject.Core.Infrastructure.Daemon;
using NovaProject.Core.Services;

namespace NovaProject.Core.Infrastructure.ClientServices;

public class DaemonBridgeService
{
    private NotificationService _notificationService;
    private ChatManagerService _chatManager;
    public DaemonBridgeService()
    {
        _notificationService = new NotificationService();
        _chatManager = new ChatManagerService();
        InitializeDaemonConnection();
    }

    public async void PingDaemon()
    {
        var time = DateTime.Now.ToString("HH:mm:ss");
        var request = new DaemonServerRequest(payload: time);
    }

    private async Task<DaemonServerResponse?> ConnectToDaemon()
    {
        try
        {
            await using var client = new NamedPipeClientStream(".", "NovaProject_Pipe", PipeDirection.InOut);
            await client.ConnectAsync((2000));
            using (var writer = new StreamWriter(client, leaveOpen: true))
            {
                writer.AutoFlush = true;
                var handshake = new DaemonServerRequest(requestType: DaemonRequestType.Ping ,payload: DateTime.Now.ToString("HH:mm:ss"));
                await writer.WriteLineAsync(JsonSerializer.Serialize(handshake));
            };
            using (var reader = new StreamReader(client, leaveOpen: true))
            {
                Logger.LogInfo("Awaiting Daemon ping response...");
                var responseJson = await reader.ReadLineAsync();
                return JsonSerializer.Deserialize<DaemonServerResponse>(responseJson!);
            }
            
        }catch(Exception e)
        {
            Logger.LogError("Daemon Failed to respond to handshake ping " + e.Message);
            return null;
        }
    }

    private async void InitializeDaemonConnection()
    {
        await Task.Delay(500);
        Logger.LogInfo("Initializing Daemon Server Bridge connection...");
        DaemonServerResponse? response = await ConnectToDaemon();
        if (response != null)
        {
            if (response.ResponseType == DaemonResponseType.PingSuccess)
            {
                Logger.LogInfo("Daemon Connection established");
            }
            else
            {
                Logger.LogError("Daemon Connection failed");
            }
        }
        else
        {
            Logger.LogInfo("Daemon Server Not Initialized");
        }
    }
}