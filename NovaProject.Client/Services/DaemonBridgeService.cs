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
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NovaProject.Core.Infrastructure;
using NovaProject.Core.Infrastructure.DaemonIO;
using NovaProject.Core.Infrastructure.Data.Shared;
using NovaProject.Core.Infrastructure.Local;
using NovaProject.Core.Services;

namespace NovaProject.Client.Services;

public class DaemonBridgeService
{
    private readonly string _pipeName = "NovaApp_Pipe";
    private readonly CancellationTokenSource _cts = new();
    private readonly Channel<DaemonServerRequest> _outgoingChannel;
    private static readonly JsonSerializerOptions _serializerOptions = new(){PropertyNameCaseInsensitive = true};

    private Task? _supervisorTask;
    private bool _isConnected;

    public bool IsConnected
    {
        get => _isConnected;
        private set
        {
            if (_isConnected != value)
            {
                _isConnected = value;
                ConnectionStateChanged?.Invoke(this, value);
            }
        }
    }

    public event EventHandler<bool>? ConnectionStateChanged;
    public event EventHandler<List<LocalUid>>? UserListReceived;
    public event EventHandler<List<ServerData>>? ServerListReceived;
    public event EventHandler<List<MessageData>>? ConversationReceived;
    public event EventHandler<MessageData>? MessageReceived;
    public event EventHandler<string>? AuthenticationSuccess;
    public event EventHandler<string>? AuthenticationFailure;
    public event EventHandler<string>? RegistrationFailure;
    public event EventHandler? RegistrationSuccess;
    public event EventHandler<string>? VaultTamperDetected;
    
    
    public DaemonBridgeService()
    {
        _outgoingChannel = Channel.CreateUnbounded<DaemonServerRequest>(new UnboundedChannelOptions()
        {
            SingleReader = true,
            SingleWriter = false
        });
    }

    public void Start()
    {
        if (_supervisorTask == null)
        {
            _supervisorTask = Task.Run(() => ConnectionSupervisorLoopAsync(_cts.Token));
        }
    }

    private async Task ConnectionSupervisorLoopAsync(CancellationToken ct)
    {
        NovaLogger.LogInfo("Daemon bridge supervisor started.");
        while (!ct.IsCancellationRequested)
        {
            try
            {
                EnsureDaemonRunning();

                NovaLogger.LogInfo($"Attempting connection to pipe '{_pipeName}'...'");
                await using var client =
                    new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);

                await client.ConnectAsync(3000, ct);
                NovaLogger.LogInfo($"Connected to pipe '{_pipeName}'.");
                IsConnected = true;

                using var sessionCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

                var readTask = ProcessIncomingResponsesAsync(client, sessionCts.Token);
                var writeTask = ProcessOutgoingRequestsAsync(client, sessionCts.Token);

                //Handshake
                SendRequest(new DaemonServerRequest(DaemonRequestType.Ping, DateTime.UtcNow.ToString("O")));

                await Task.WhenAny(readTask, writeTask);
                await sessionCts.CancelAsync();

                await Task.WhenAll(readTask, writeTask);
            }
            catch (TimeoutException)
            {
                NovaLogger.LogInfo("Waiting for daemon named pipe to become avaible...");
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                NovaLogger.LogCritical($"Bridge pipe connection error: {ex.Message}");
            }
            finally
            {
                IsConnected = false;
            }

            //Retry connection on unexpected disconnect
            if (!ct.IsCancellationRequested)
            {
                await Task.Delay(2000, ct);
            }
        }
    }

    private async Task ProcessIncomingResponsesAsync(Stream pipeStream, CancellationToken ct)
    {
        using var reader = new StreamReader(pipeStream, leaveOpen: true);
        while (!ct.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(ct);
            if (line == null) break; //EOF (disconnected)

            if (string.IsNullOrWhiteSpace(line)) continue;

            try
            {
                var response = JsonSerializer.Deserialize<DaemonServerResponse>(line);
                if (response != null)
                {
                    DispatchResponse(response);
                }
            }
            catch (Exception ex)
            {
                NovaLogger.LogError($"Failed to parse daemon response line: {ex.Message}");
                NovaLogger.LogError($"Line content: {line}");
            }
        }
    }

    private async Task ProcessOutgoingRequestsAsync(Stream pipeStream, CancellationToken ct)
    {
        using var writer = new StreamWriter(pipeStream, leaveOpen: true) { AutoFlush = true };

        while (await _outgoingChannel.Reader.WaitToReadAsync(ct))
        {
            while (_outgoingChannel.Reader.TryRead(out var request))
            {
                var json = JsonSerializer.Serialize(request, _serializerOptions);
                await writer.WriteLineAsync(json.AsMemory(), ct);
                NovaLogger.LogInfo($"Bridge dispatched: {request.RequestType}");
            }
        }
    }

    private void DispatchResponse(DaemonServerResponse response)
    {
        NovaLogger.LogInfo("Daemon Server Response: " + response.ResponseType);
        _isConnected = true;
        switch (response.ResponseType)
        {
            case DaemonResponseType.PingSuccess:
                NovaLogger.LogInfo("Ping successful");
                break;
            case DaemonResponseType.AuthSuccess:
                AuthenticationSuccess?.Invoke(this, response.Payload);
                break;
            case DaemonResponseType.AuthFailed:
                AuthenticationFailure?.Invoke(this, response.Payload);
                break;
            case DaemonResponseType.RegisterFail:
                RegistrationFailure?.Invoke(this, response.Payload);
                break;
            case DaemonResponseType.RegisterSuccess:
                RegistrationSuccess?.Invoke(this, EventArgs.Empty);
                break;
            case DaemonResponseType.VaultTamper:
                VaultTamperDetected?.Invoke(this, response.Payload);
                break;
            case DaemonResponseType.LoadUserList:
                var users = JsonSerializer.Deserialize<List<LocalUid>>(response.Payload, _serializerOptions);
                if (users == null || users.Count == 0)
                {
                    NovaLogger.LogError("Received empty or null UserList");
                    return;
                }
                UserListReceived?.Invoke(this, users);
                break;
            case DaemonResponseType.LoadServerList:
                var servers = JsonSerializer.Deserialize<List<ServerData>>(response.Payload, _serializerOptions);
                if (servers == null || servers.Count == 0)
                {
                    NovaLogger.LogError("Received empty or null ServerList");
                    return;
                }
                ServerListReceived?.Invoke(this, servers);
                break;
            case DaemonResponseType.LoadUserConversation:
                var conversation = JsonSerializer.Deserialize<List<MessageData>>(response.Payload, _serializerOptions);
                if (conversation == null || conversation.Count == 0)
                {
                    NovaLogger.LogError("Received empty or null Conversation");
                    return;
                }
                ConversationReceived?.Invoke(this, conversation);
                break;
            case DaemonResponseType.MessageInbound:
                var msg = JsonSerializer.Deserialize<MessageData>(response.Payload, _serializerOptions);
                MessageReceived?.Invoke(this, msg);
                break;
                
        }
    }
    

    
    #region Public Consumer Commands (Cosumed by ChatService and Login)
    public void SendRequest(DaemonServerRequest request)
    {
        _outgoingChannel.Writer.TryWrite(request);
    }

    public void RequestUserList()
    {
        SendRequest(new DaemonServerRequest(DaemonRequestType.LoadLocalUserList));
    }

    public void RequestServerList()
    {
        SendRequest(new DaemonServerRequest(DaemonRequestType.LoadServerList));
    }

    public void Login(string username, string password)
    {
        var payload = JsonSerializer.Serialize(new ProfileCredentialsDto(username, password), _serializerOptions);
        SendRequest(new DaemonServerRequest(DaemonRequestType.LoginUser, payload));
    }

    public void Register(string username, string password)
    {
        var payload = JsonSerializer.Serialize(new ProfileCredentialsDto(username, password), _serializerOptions);
        SendRequest(new DaemonServerRequest(DaemonRequestType.RegisterUser, payload));
    }

    public void SendMessage(MessageData message)
    {
        var payload = JsonSerializer.Serialize(message, _serializerOptions);
        SendRequest(new DaemonServerRequest(DaemonRequestType.MessageOutbound, payload));
    }

    public void LoadConversation(LocalUid conversation)
    {
        var payload = JsonSerializer.Serialize(conversation, _serializerOptions);
        SendRequest(new DaemonServerRequest(DaemonRequestType.LoadUserConversation, payload));
    }
    
    #endregion
    
    #region Daemon Process Management
    private void EnsureDaemonRunning()
    {
        var currentPid = Environment.ProcessId;

        var runningDaemons = Process.GetProcessesByName("NovaProject.Daemon")
            .Where(p => p.Id != currentPid)
            .ToList();

        if (runningDaemons.Count > 0)
        {
            return;
        }
        
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string binaryName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) 
            ? "NovaProject.Daemon.exe"
            : "NovaProject.Daemon";
        string daemonPath = Path.Combine(baseDir, "Daemon", binaryName);
        
        if (!File.Exists(daemonPath))
        {
            daemonPath = Path.Combine(baseDir, binaryName);
        }

        if (File.Exists(daemonPath))
        {
            NovaLogger.LogInfo($"Launching daemon executable at: {daemonPath}...");

            var startInfo = new ProcessStartInfo
            {
                FileName = daemonPath,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput =  true,
                WorkingDirectory = Path.GetDirectoryName(daemonPath) ?? baseDir
            };
            var process = Process.Start(startInfo);

            if (process != null)
            {
                process.OutputDataReceived += (_, e) => { if (e.Data != null) NovaLogger.LogInfo($"[Daemon stdout] {e.Data}"); };
                process.ErrorDataReceived += (_, e) => { if (e.Data != null) NovaLogger.LogError($"[Daemon stderr] {e.Data}"); };
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }
        }
        else
        {
            NovaLogger.LogCritical($"Daemon executable not found at '{daemonPath}'. If debugging, start it manually.");
        }
    }
    
    #endregion
    
    
    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        _outgoingChannel.Writer.Complete();

        if (_supervisorTask != null)
        {
            await _supervisorTask;
        }
        
        _cts.Dispose();
        GC.SuppressFinalize(this);
    }
}
