using Microsoft.AspNetCore.SignalR.Client;

namespace NovaProject.Core.Infrastructure;

public partial class ServerInstance
{
    public string ServerUrl;
    public string ServerName;
    public string ServerDescription;
    HubConnectionState _connectionState = HubConnectionState.Disconnected;

    public HubConnectionState ConnectionState
    {
        get => _connectionState;
        set
        {
            _connectionState = value;
        }
    }
    private HubConnection _connection;

    public ServerInstance(string url)
    {
        ServerUrl = url;
        ServerName = string.Empty;
        ServerDescription = string.Empty;
        
        _connection = new HubConnectionBuilder()
            .WithUrl($"{url}/main")
            .WithAutomaticReconnect()
            .Build();
        _connection.On<string>("RequestDisconnect", async (reason) =>
        {
            await _connection.StopAsync();
            ConnectionState =  HubConnectionState.Disconnected;
            Console.WriteLine($"[{ServerName}] You have been disconnected from {ServerName}. Reason: {reason}");
        });
    }
    
    public async Task EnsureConnected()
    {
        if (_connectionState == HubConnectionState.Connected)
        {
            await _connection.StartAsync();
            ConnectionState = _connection.State;
            Console.WriteLine($"[{ServerName}] Establishing connection...");
        }
    }
    
    
}