using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NovaProject.Core.Infrastructure;
using NovaProject.Core.Infrastructure.Daemon;
using NovaProject.Core.Services;

namespace NovaProject.Client.Services;

public class DaemonBridgeService
{
    private bool _isConnected = false;
    private readonly CancellationTokenSource _cts = new();
    private ConcurrentQueue<DaemonServerRequest> _requestQueue = new();
    public DaemonBridgeService()
    {
        Task.Run(InitializeDaemonConnection);
    }

    private async Task ListenToDaemon(CancellationToken cts)
    {
        Logger.LogInfo("Listening to Daemon Server...");
        while (!_cts.IsCancellationRequested)
        {
            if (_isConnected)
            {
                try
                {
                    await using var client = new NamedPipeClientStream(".", "NovaProject_Pipe", PipeDirection.InOut);
                    await client.ConnectAsync(2000);
                    await using (var writer = new StreamWriter(client, leaveOpen: true))
                    {
                        writer.AutoFlush = true;
                        if (_requestQueue.TryDequeue(out var request))
                        {
                            var json =  JsonSerializer.Serialize(request);
                            await writer.WriteLineAsync(json);
                        }
                    }

                    using (var reader = new StreamReader(client, leaveOpen: true))
                    {
                        var responseJson = await reader.ReadLineAsync();
                        if (!string.IsNullOrEmpty(responseJson) &&
                            JsonSerializer.Deserialize<DaemonServerResponse>(responseJson) is {} response)
                        {
                            HandleDaemonResponse(response);
                        }
                    }

                }
                catch (Exception e)
                {
                    Logger.LogError(e.Message);
                }
            }
            else
            {
                Logger.LogInfo("Daemon Server Not Listening");
            }
        }
        Logger.LogCritical("Daemon Server Disconnected");
    }

    public void PingDaemon()
    {
        var time = DateTime.Now.ToString("HH:mm:ss");
        var request = new DaemonServerRequest(payload: time);
        SendRequest(request);
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

    private async Task InitializeDaemonConnection()
    {
        await Task.Delay(500);
        Logger.LogInfo("Initializing Daemon Server Bridge connection...");
        DaemonServerResponse? response = await ConnectToDaemon();
        if (response != null)
        {
            if (response.ResponseType == DaemonResponseType.PingSuccess)
            {
                Logger.LogInfo("Daemon Connection established");
                _isConnected = true;
                await Task.Run(() => ListenToDaemon(_cts.Token));
            }
            else
            {
                Logger.LogError("Daemon Connection failed");
            }
        }
        else
        {
            Logger.LogCritical("Daemon Server Not Initialized");
        }
    }

    private void HandleDaemonResponse(DaemonServerResponse response)
    {
        Logger.LogInfo("Daemon Server Response: " + response.ResponseType);
        _isConnected = true;
        switch (response.ResponseType)
        {
            case DaemonResponseType.PingSuccess:
                break;
            case DaemonResponseType.LoadUserList:
                Logger.LogInfo("Loading UserList from daemon...");
                App.ServiceProvider.GetService<ChatService>()?.UpdateUserList(JsonSerializer.Deserialize<List<User>>(response.Payload));
                break;
            case DaemonResponseType.LoadServerList:
                Logger.LogInfo("Loading ServerList from daemon...");
                App.ServiceProvider.GetService<ChatService>()?.UpdateServerList(JsonSerializer.Deserialize<List<ServerData>>(response.Payload));
                break;
        }
    }

    public void SendRequest(DaemonServerRequest request)
    {
        _requestQueue.Enqueue(request);
        Logger.LogInfo("Sending request to Daemon Server...");
    }
}