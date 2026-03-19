using Microsoft.AspNetCore.SignalR.Client;
using NovaProject.Core.Services;

namespace NovaProject.Daemon.Services;
public class ConnectionHubService : BackgroundService
{
    private readonly Dictionary<string, HubConnection> _connections = new();
    private ChatManagerService _chatManagerService;

    public event Action<string, string, string>? MessageReceived;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Logger.LogInfo("ConnectionHub Heartbeat");
    }
    

    public async Task ConnectToServer(string url)
    {
        if (!_connections.ContainsKey(url))
        {
            Logger.LogError("Tried to connect to server: " + url +", But the connection is already listed");
            return;
        }
        var connection = new HubConnectionBuilder().WithUrl($"{url}/main").WithAutomaticReconnect().Build();
        connection.On<string, string>("ReceiveMessage", (tag, msg) =>
        {
            MessageReceived?.Invoke(url, tag, msg);
        });
        await connection.StartAsync();
        _connections.Add(url, connection);
    }

    public async Task SendMessage(string url, string message, string recipientTag)
    {
        if (_connections.TryGetValue(url, out var connection))
        {
            await connection.InvokeAsync("SendMessage", message, recipientTag);
        }
        else
        {
            Logger.LogError("Tried SendMessage to " + url + " but no connection exists");
        }
    }
    
}