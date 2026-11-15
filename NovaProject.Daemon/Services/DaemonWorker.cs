using System.Collections.Concurrent;
using System.IO.Pipes;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Channels;
using NovaProject.Core.Infrastructure.DaemonIO;
using NovaProject.Core.Infrastructure.Data.Shared;
using NovaProject.Core.Infrastructure.Local;
using NovaProject.Core.Services;
using NovaProject.Daemon.Database;
using NovaProject.Daemon.Services.Database;
using NovaProject.Daemon.Services.Network;

namespace NovaProject.Daemon.Services;

public class DaemonWorker(UserProfileService profileService, UserVaultManager vaultManager, LocalVaultDataService vaultDataService) : BackgroundService
{
    private const string PipeName = "NovaApp_Pipe";
    private readonly UserProfileService _profileService = profileService;
    private readonly UserVaultManager _vaultManager = vaultManager;
    private readonly LocalVaultDataService _vaultDataService = vaultDataService;

    private ServerConnectionWorker? _serverWorker;
    private readonly ECDsa _clientKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
    
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        NovaLogger.LogInfo("Daemon Worker Service Started.");
        
        // Initialize master Registry
        await _profileService.InitializeAsync();
        
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                //Knock knock, UI
                await using var pipeServer = new NamedPipeServerStream(
                    PipeName, 
                    PipeDirection.InOut,
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);
                
                NovaLogger.LogInfo("Daemon waiting for client connection...");
                await pipeServer.WaitForConnectionAsync(stoppingToken);
                NovaLogger.LogInfo("Client connected.");
                
                var responseChannel = Channel.CreateUnbounded<DaemonServerResponse>(new UnboundedChannelOptions()
                {
                    SingleReader = true,
                    SingleWriter = false
                });
                
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);

                using var reader = new StreamReader(pipeServer, leaveOpen: true);
                using var writer = new StreamWriter(pipeServer, leaveOpen: true) { AutoFlush = true };

                var readTask = ProcessIncomingRequestsAsync(pipeServer, responseChannel, linkedCts.Token);
                var writeTask = ProcessOutgoingResponseAsync(pipeServer, responseChannel, linkedCts.Token);
                
                await Task.WhenAny(readTask, writeTask);
                await linkedCts.CancelAsync();
                await Task.WhenAll(readTask, writeTask);
                
                NovaLogger.LogInfo("Daemon connection closed");
                if (pipeServer.IsConnected)
                {
                    pipeServer.Disconnect();
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                NovaLogger.LogInfo("Daemon worker stopped, cancelling operations...");
            }
            catch (Exception ex)
            {
                NovaLogger.LogError($"Error in daemon pipe loop: {ex.Message}");
                //Rapid fire prevention
                await Task.Delay(2000, stoppingToken);
            }
        }
    }

    private async Task ProcessIncomingRequestsAsync(
        Stream pipeStream,
        ChannelWriter<DaemonServerResponse> responseWriter,
        CancellationToken ct)
    {
        using var reader = new StreamReader(pipeStream, leaveOpen: true);
        while (!ct.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(ct);
            if(line == null) break; //Disconnected (EOF)

            try
            {
                var request = JsonSerializer.Deserialize<DaemonServerRequest>(line, JsonOptions);
                if (request == null)
                {
                    NovaLogger.LogError("Deserialized Daemon Request was null.");
                    continue;
                }

                await HandleRequestAsync(request, responseWriter, ct);
            }
            catch (Exception ex)
            {
                NovaLogger.LogError($"Failed to parse or process request: {ex.Message}");
            }
        }
    }

    private async Task ProcessOutgoingResponseAsync(
        Stream pipeStream,
        ChannelReader<DaemonServerResponse> responseReader,
        CancellationToken ct)
    {
        using var writer = new StreamWriter(pipeStream, leaveOpen: true) {AutoFlush = true};

        while (await responseReader.WaitToReadAsync(ct))
        {
            while (responseReader.TryRead(out var response))
            {
                var json = JsonSerializer.Serialize(response, JsonOptions);
                await writer.WriteLineAsync(json.AsMemory(), ct);
                NovaLogger.LogInfo($"Daemon sent: {response.ResponseType}");
            }
        }
    }

    private async Task HandleRequestAsync(
        DaemonServerRequest request,
        ChannelWriter<DaemonServerResponse> responseWriter,
        CancellationToken ct)
    {
        NovaLogger.LogInfo("Daemon Responding to " + request.RequestType);
        switch (request.RequestType)
        {
            case DaemonRequestType.Ping: 
                await responseWriter.WriteAsync(new DaemonServerResponse(
                    DaemonResponseType.PingSuccess, 
                    DateTime.Now.ToString("HH:mm:ss")), ct);
                break;
            
            case DaemonRequestType.LoginUser:
                await HandleLoginUserAsync(request, responseWriter, ct);
                break;
            
            case DaemonRequestType.RegisterUser:
                await HandleRegisterUserAsync(request, responseWriter, ct);
                break;
            
            case DaemonRequestType.LockVault:
                break;
            
            case DaemonRequestType.LoadServerList:
                await responseWriter.WriteAsync(new DaemonServerResponse(
                    DaemonResponseType.LoadServerList,
                    JsonSerializer.Serialize(PlaceHolderData.PlaceHolderServerData, JsonOptions)), ct);
                break;
            
            case DaemonRequestType.LoadLocalUserList: 
                await responseWriter.WriteAsync(new DaemonServerResponse(
                    DaemonResponseType.LoadUserList,
                    JsonSerializer.Serialize(PlaceHolderData.PlaceholderUserData, JsonOptions)), ct);
                break;
            
            case DaemonRequestType.MessageOutbound: 
                await HandlePrivateMessageOutgoingAsync(request, responseWriter, ct); 
                break;
            
            case DaemonRequestType.MessageInbound:
                await HandlePrivateMessageIncomingAsync(request, responseWriter, ct);
                break;
            
            case DaemonRequestType.LoadUserConversation: 
                await HandleLoadingUserConversationAsync(request, responseWriter, ct); 
                break;
            
            default:
                NovaLogger.LogInfo($"Unhandled request type: {request.RequestType}");
                break;
            }
    }

    #region Handlers

    private async Task HandleLoginUserAsync(DaemonServerRequest request,
        ChannelWriter<DaemonServerResponse> responseWriter, CancellationToken ct)
    {
        try
        {
            var credentials = JsonSerializer.Deserialize<ProfileCredentialsDto>(request.Payload, JsonOptions);
            if (credentials == null || string.IsNullOrWhiteSpace(credentials.Username) ||
                string.IsNullOrWhiteSpace(credentials.Password))
            {
                await responseWriter.WriteAsync(new DaemonServerResponse(DaemonResponseType.AuthFailed,
                    "Username and password are required"), ct);
                return;
            }

            var result = await _profileService.UnlockProfileAsync(credentials.Username, credentials.Password);

            switch (result.Status)
            {
                case ProfileAccessStatus.Success:
                    await responseWriter.WriteAsync(new DaemonServerResponse(DaemonResponseType.AuthSuccess,
                        result.LUid!.ToString()), ct);
                    await ConnectToRelayServerAsync(credentials.Username, responseWriter, ct);
                    break;
                case ProfileAccessStatus.IntegrityCheckFail:
                    NovaLogger.LogCritical($"Vault integrity check failed for {credentials.Username}!");
                    await responseWriter.WriteAsync(
                        new DaemonServerResponse(DaemonResponseType.VaultTamper,
                            "Vault Integrity Check Failed. Potential disk corruption or tampering."), ct);
                    break;
                case ProfileAccessStatus.InvalidPassword:
                case ProfileAccessStatus.NotFound:
                default:
                    NovaLogger.LogInfo($"Login attempt failed for {credentials.Username}!");
                    await responseWriter.WriteAsync(
                        new DaemonServerResponse(DaemonResponseType.AuthFailed,
                            $"Invalid credentials for {credentials.Username}!"), ct);
                    break;
            }
        }
        catch (Exception ex)
        {
            NovaLogger.LogError($"Login failed: {ex.Message}");
            await responseWriter.WriteAsync(new DaemonServerResponse(DaemonResponseType.AuthFailed, "An error occured during authentication!"), ct);
        }
    }

    public async Task HandleRegisterUserAsync(DaemonServerRequest request, ChannelWriter<DaemonServerResponse> responseWriter,
        CancellationToken ct)
    {
        try
        {
            var credentials = JsonSerializer.Deserialize<ProfileCredentialsDto>(request.Payload, JsonOptions);
            if (credentials == null || string.IsNullOrWhiteSpace(credentials.Username) ||
                string.IsNullOrWhiteSpace(credentials.Password))
            {
                await responseWriter.WriteAsync(
                    new DaemonServerResponse(DaemonResponseType.RegisterFail, "Username and password are required"),
                    ct);
                return;
            }

            var result = await _profileService.RegisterUserAsync(credentials.Username, credentials.Password);

            if (result.Status == ProfileAccessStatus.Success)
            {
                await responseWriter.WriteAsync(
                    new DaemonServerResponse(DaemonResponseType.RegisterSuccess, result.Pid!), ct);
            }
            else
            {
                await responseWriter.WriteAsync(
                    new DaemonServerResponse(DaemonResponseType.RegisterFail, "User already exists!"), ct);
            }
        }
        catch (Exception ex)
        {
            NovaLogger.LogError($"Registration failed: {ex.Message}");
            await responseWriter.WriteAsync(new DaemonServerResponse(DaemonResponseType.RegisterFail, $"An error occured during registration: {ex.Message}"), ct);
        }
    }
    
    private async Task HandlePrivateMessageOutgoingAsync(
        DaemonServerRequest request, 
        ChannelWriter<DaemonServerResponse> responseWriter, 
        CancellationToken ct)
    {
        try
        {
            var payload = JsonSerializer.Deserialize<MessageData>(request.Payload, JsonOptions);
            
            //TODO: Implement DB persistence

            if (_serverWorker is { IsAuthenticated: true })
            {
                NovaLogger.LogInfo($"FORWARDING MESSAGE TO RELAY -> {payload.ConversationId}");
                await _serverWorker.SendPrivateMessageAsync(payload, ct);
            }
        }
        catch (Exception ex)
        {
            NovaLogger.LogError($"Daemon failed to process outgoing message {ex.Message}");   
        }
    }

    private async Task HandlePrivateMessageIncomingAsync(
        DaemonServerRequest request, 
        ChannelWriter<DaemonServerResponse> responseWriter, 
        CancellationToken ct)
    {
        
    }

    private async Task HandleLoadingUserConversationAsync(
        DaemonServerRequest request,
        ChannelWriter<DaemonServerResponse> responseWriter,
        CancellationToken ct)
    {
        try
        {
            var lUid = JsonSerializer.Deserialize<LocalUid>(request.Payload);
            if (lUid == null) return;
        
            string conversationId = lUid.ToString();
            
            NovaLogger.LogInfo($"Getting User conversation: {conversationId}");
        
            var dbResult = await _vaultDataService.GetMessagesAsync(conversationId, AppGlobalService.MessageHistoryLimit);
        
            var conversation = dbResult.Select(
                    m => new MessageData
                    {
                        ConversationId = m.ConversationId,
                        IsOutbound = m.IsOutbound,
                        Content = m.Content,
                        Timestamp = m.TimeStamp
                    })
                .ToList();

            await responseWriter.WriteAsync(
                new DaemonServerResponse(
                    DaemonResponseType.LoadUserConversation,
                    JsonSerializer.Serialize(conversation, JsonOptions)), ct);
        }
        catch (Exception ex)
        {
            NovaLogger.LogError($"Failed to load conversation: {ex.Message}");
            await responseWriter.WriteAsync(
                new DaemonServerResponse(DaemonResponseType.LoadUserConversationFail, ex.Message), ct);
        }
    }

    private async Task ConnectToRelayServerAsync(string username, ChannelWriter<DaemonServerResponse> responseWriter, CancellationToken ct)
    {
        try
        {
            if (_serverWorker != null)
            {
                await _serverWorker.DisposeAsync();
            }

            _serverWorker = new ServerConnectionWorker("localhost:5050", username, _clientKey);
            _serverWorker.MessageReceived += async (msg) =>
            {
                try
                {
                    await responseWriter.WriteAsync(
                        new DaemonServerResponse(
                            DaemonResponseType.MessageInbound,
                            JsonSerializer.Serialize(msg, JsonOptions)), ct);

                }
                catch (Exception ex)
                {
                    NovaLogger.LogError($"Failed to receive message: {ex.Message}");
                }
            };

            await _serverWorker.ConnectAndRunAsync(ct);
            NovaLogger.LogInfo($"Connecting to Relay Server as {_serverWorker.FullLocalUid}");

        }
        catch (Exception ex)
        {
            NovaLogger.LogError($"Failed to connect to relay server: {ex.Message}");
        }
    }
    #endregion
}