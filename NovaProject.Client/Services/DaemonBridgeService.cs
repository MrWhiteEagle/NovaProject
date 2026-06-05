using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NovaProject.Core.Infrastructure;
using NovaProject.Core.Infrastructure.Local;
using NovaProject.Core.Infrastructure.Ui.Daemon;
using NovaProject.Core.Services;

namespace NovaProject.Client.Services;

public class DaemonBridgeService
{
    private bool _isConnected = false;
    private bool _HasDaemonInitialized = false;
    private readonly CancellationTokenSource _cts = new();
    private ConcurrentQueue<DaemonServerRequest> _requestQueue = new();
    public DaemonBridgeService()
    {
        Task.Run(InitializeDaemonConnection);
    }

    private async Task InitializeDaemonConnection()
    {
        StartupDaemon();
        Logger.LogInfo("Initializing Daemon Server Bridge connection...");
        await using var client = new NamedPipeClientStream(".", "NovaProject_Pipe", PipeDirection.InOut);
        while (!client.IsConnected && !_cts.IsCancellationRequested)
        {
            try
            {
                // Try to connect with a 2-second timeout
                await client.ConnectAsync(2000, _cts.Token);
                Logger.LogInfo("Pipe connection established!");
            }
            catch (TimeoutException)
            {
                Logger.LogInfo("Waiting for Daemon pipe to become available...");
            }
            catch (Exception e)
            {
                Logger.LogError($"Connection error: {e.Message}");
                await Task.Delay(1000);
            }
        }
        using var reader = new StreamReader(client, leaveOpen: true);
        using var writer = new StreamWriter(client) { AutoFlush = true };
        Logger.LogInfo("Trying daemon handshake...");
        //Listen Task
        var listen = Task.Run(async () =>
        {
            try
            {
                Logger.LogInfo("Bridge ready for read");
                while (!_cts.IsCancellationRequested)
                {
                    var response = await reader.ReadLineAsync();
                    if (!string.IsNullOrEmpty(response) && !string.IsNullOrWhiteSpace(response))
                    {
                        _isConnected = true;
                        var json = JsonSerializer.Deserialize<DaemonServerResponse>(response);
                        HandleDaemonResponse(json);
                    }
                }

            }
            catch(Exception e)
            {
                Logger.LogError("Bridge listen error: " + e.Message);
            }
        });

        //Send Task
        var send = Task.Run(async () =>
        {
            try
            {
                Logger.LogInfo("Bridge ready for send");
                while (!_cts.IsCancellationRequested && !_HasDaemonInitialized)
                {
                    Logger.LogInfo("Attempting handshake ping...");
                    var ping = new DaemonServerRequest(payload: DateTime.Now.ToString());
                    var jsonPing = JsonSerializer.Serialize(ping);
                    await writer.WriteLineAsync(jsonPing);
            
                    // Wait 1 second before trying to ping again
                    await Task.Delay(1000); 
                }
                
                Logger.LogInfo("HANDSHAKE CONFIRMED, request gate is open...");
                
                while (!_cts.IsCancellationRequested && client.IsConnected)
                {
                    if (_requestQueue.TryDequeue(out var request))
                    {
                        Logger.LogInfo("Bridge sending request...");
                        var json = JsonSerializer.Serialize(request);
                        await writer.WriteLineAsync(json);
                        await client.FlushAsync();
                    }
                    else
                    {
                        await Task.Delay(500);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError("Bridge send error: " + e.Message);
            }
        });
        await Task.WhenAll(listen, send);
        client.Dispose();
        writer.Dispose();
        reader.Dispose();
        Logger.LogInfo("Daemon server connection closed.");
        
    }
    
    private void StartupDaemon()
    {
        var currentPid = Process.GetCurrentProcess().Id;
    
        // Find all processes with the Daemon name
        var existing = Process.GetProcessesByName("NovaProject.Daemon")
            .Where(p => p.Id != currentPid) // Safety check
            .ToList();

        if (existing.Count > 0)
        {
            Logger.LogInfo($"Found {existing.Count} ghost processes. Purging...");
            foreach (var p in existing)
            {
                try
                {
                    p.Kill();
                    p.WaitForExit(1000);
                }
                catch(Exception e)
                {
                    Logger.LogError(e.ToString());
                }
            }
        }
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string daemonName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) 
            ? "NovaProject.Daemon.exe"
            : "NovaProject.Daemon";
        string daemonPath = Path.Combine(baseDir, "Daemon", daemonName);
        if (File.Exists(daemonPath))
        {
            Logger.LogInfo("Starting Daemon server process...");
            LaunchDaemonBinary(daemonPath);
        }
        else
        {
            Logger.LogInfo("Daemon not found at " + daemonPath);
        }
            
        
    }

    private void LaunchDaemonBinary(string daemonPath)
    {
        Process.Start(
            new ProcessStartInfo
            {
                FileName = daemonPath,
                CreateNoWindow = true,
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            }
        );
    }

    private void HandleDaemonResponse(DaemonServerResponse response)
    {
        Logger.LogInfo("Daemon Server Response: " + response.ResponseType);
        _isConnected = true;
        switch (response.ResponseType)
        {
            case DaemonResponseType.PingSuccess:
                Logger.LogInfo("Ping successful");
                _HasDaemonInitialized = true;
                break;
            case DaemonResponseType.LoadUserList:
                Logger.LogInfo("Loading UserList from daemon...");
                App.ServiceProvider.GetService<ChatService>()?.UpdateUserList(JsonSerializer.Deserialize<List<User>>(response.Payload));
                break;
            case DaemonResponseType.LoadServerList:
                Logger.LogInfo("Loading ServerList from daemon...");
                App.ServiceProvider.GetService<ChatService>()?.UpdateServerList(JsonSerializer.Deserialize<List<ServerData>>(response.Payload));
                break;
            case DaemonResponseType.LoadUserConversation:
                App.ServiceProvider.GetService<ChatService>()?.LoadUserConversation(JsonSerializer.Deserialize<List<MessageData>>(response.Payload));
                Logger.LogInfo("Loading UserConversation from daemon...");
                break;
            case DaemonResponseType.MessageInbound:
                Logger.LogInfo("Message inbound from daemon...");
                App.ServiceProvider.GetService<ChatService>()?.ReceiveMessage(response);
                break;
                
        }
    }

    public void SendRequest(DaemonServerRequest request)
    {
        _requestQueue.Enqueue(request);
        Logger.LogInfo($"Request queued: {request.RequestType}");
    }

    public void GetUserConversation(User user)
    {
        var request = new DaemonServerRequest(DaemonRequestType.LoadUserConversation, JsonSerializer.Serialize(user));
        SendRequest(request);
    }

    private void HandleInboundMessage(DaemonServerResponse response)
    {
        App.ServiceProvider.GetService<ChatService>()?.ReceiveMessage(response);
    }
    public void PingDaemon()
    {
        var time = DateTime.Now.ToString("HH:mm:ss");
        var request = new DaemonServerRequest(payload: time);
        SendRequest(request);
    }
}