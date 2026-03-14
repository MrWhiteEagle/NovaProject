using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using NovaProject.Models;

namespace NovaProject.Services;
public class ConnectionHubService
{
    private readonly Dictionary<string, HubConnection> _connections = new();
    private ChatManagerService _chatManagerService;

    public event Action<string, string, string>? MessageReceived;

    public async Task ConnectToServer(string url)
    {
        if (!_connections.ContainsKey(url))
        {
            Console.WriteLine("Tried to connect to server: " + url +", But the connection is already listed");
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
            Console.WriteLine("Tried SendMessage to " + url + " but no connection exists");
        }
    }
    
}