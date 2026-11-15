using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using NovaProject.Core.Infrastructure.Data.Shared;
using NovaProject.Core.Infrastructure.Network;
using NovaProject.Core.Services;

namespace NovaProject.Daemon.Services.Network;

public class ServerConnectionWorker : IAsyncDisposable
{
    private readonly ClientWebSocket _socket = new();
    private readonly ECDsa _clientIdentityKey;
    private readonly string _username;
    private readonly string _serverHost; //e.g. "localhost:5050" for now.
    private readonly Uri _wsUri;

    private byte[]? _pinnedServerPublicKey;
    private CancellationTokenSource? _cts;
    
    public string? AssignedTag { get; private set; }
    public bool IsAuthenticated => _socket.State == WebSocketState.Open && !string.IsNullOrEmpty(AssignedTag);
    public string FullLocalUid => $"{_username}#{AssignedTag}@{_serverHost}";
    
    //Fired when message received or delivery failed
    public event Action<MessageData?> MessageReceived;
    public event Action<string>? DeliveryFailed;
    public event Action<string>? Authenticated; //Pass Full Local UID

    public ServerConnectionWorker(
        string serverHost,
        string username,
        ECDsa clientIdentityKey,
        byte[]? pinnedServerPublicKey = null)
    {
        _serverHost = serverHost;
        _username = username;
        _clientIdentityKey = clientIdentityKey;
        _pinnedServerPublicKey = pinnedServerPublicKey;
        _wsUri = new Uri($"ws://{serverHost}/ws");
    }

    public async Task ConnectAndRunAsync(CancellationToken externalCt = default)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(externalCt);
        var ct = _cts.Token;
        
        await _socket.ConnectAsync(_wsUri, ct);
        
        //STAGE 1: CLIENTHELLO && VERIFICATION
        byte[] clientNonce = RandomNumberGenerator.GetBytes(32);
        await SendEnvelopeAsync(WirePacketType.ClientHello, new ClientHelloDto(clientNonce), ct);

        var proofEnvelope = await ReceiveEnvelopeAsync(ct);
        if (proofEnvelope?.Type != WirePacketType.ServerProof)
            throw new InvalidOperationException(
                $"Protocol Violation! Expected ServerProof and instead got {proofEnvelope?.Type.ToString()}!");

        var serverProof = JsonSerializer.Deserialize<ServerProofDto>(proofEnvelope.PayloadJson)!;
        
        //TOFU check: If already pinned key, check for exact match
        if (_pinnedServerPublicKey != null && !_pinnedServerPublicKey.SequenceEqual(serverProof.ServerPublicKey))
        {
            await _socket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Server key mismatch", ct);
            throw new CryptographicException($"SECURITY VIOLATION! Server public  changed for {_serverHost}!");
        }
        
        using var serverEcdsa = ECDsa.Create();
        serverEcdsa.ImportSubjectPublicKeyInfo(serverProof.ServerPublicKey, out _);

        if (!serverEcdsa.VerifyData(clientNonce, serverProof.Signature, HashAlgorithmName.SHA256))
        {
            await _socket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Invalid server signature", ct);
            throw new CryptographicException($"Server {_serverHost} failed cryptographic challenge!");
        }
        
        _pinnedServerPublicKey ??= serverProof.ServerPublicKey;
        
        //STAGE 2: Verified -> Send Client proof
        byte[] clientPublicKey = _clientIdentityKey.ExportSubjectPublicKeyInfo();
        byte[] clientSignature = _clientIdentityKey.SignData(serverProof.ServerNonce, HashAlgorithmName.SHA256);

        var authProof = new ClientAuthProofDto(_username, clientPublicKey, clientSignature);
        await SendEnvelopeAsync(WirePacketType.ClientAuthProof, authProof, ct);
        
        var ackEnvelope = await ReceiveEnvelopeAsync(ct);
        if(ackEnvelope?.Type != WirePacketType.ServerAuthAck)
            throw new InvalidOperationException("Protocol Violation! Expected ServerAuthAck");

        var ack = JsonSerializer.Deserialize<ServerAuthAckDto>(ackEnvelope.PayloadJson)!;
        if (!ack.Success)
            throw new UnauthorizedAccessException($"Server rejected authentication: {ack.ErrorMessage}.");
        
        AssignedTag = ack.AssignedTag;
        
        NovaLogger.LogInfo($"[DAEMON -> SERVER] Connected and Authenticated as {FullLocalUid}.");
        
        Authenticated?.Invoke(FullLocalUid);
        
        //STAGE 3: Receive Loop
        _ = Task.Run(() => ReceiveLoopAsync(ct), ct);

    }


    private async Task ReceiveLoopAsync(CancellationToken ct)
    {
        try
        {
            while (_socket.State == WebSocketState.Open && !ct.IsCancellationRequested)
            {
                var envelope = await ReceiveEnvelopeAsync(ct);
                if (envelope == null) break;

                switch (envelope.Type)
                {
                    case WirePacketType.DirectMessage:
                        var incoming = JsonSerializer.Deserialize<MessageData>(envelope.PayloadJson);
                        NovaLogger.LogInfo($"[DAEMON <- SERVER] Received message from {incoming.ConversationId}.");
                        MessageReceived?.Invoke(incoming);
                        break;
                    case WirePacketType.DeliveryError:
                        var errorText = JsonSerializer.Deserialize<string>(envelope.PayloadJson);
                        NovaLogger.LogError($"[DAEMON <- SERVER] DeliveryError: {errorText}.");
                        DeliveryFailed?.Invoke(errorText);
                        break;
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception e)
        {
            NovaLogger.LogError($"[DAEMON -> SERVER] Connection loop terminated: {e.Message}");
        }
    }

    private async Task SendEnvelopeAsync<T>(WirePacketType packetType, T payload, CancellationToken ct)
    {
        var envelope = new WireEnvelope(packetType, JsonSerializer.Serialize(payload));
        byte[] bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(envelope));
        await _socket.SendAsync(bytes, WebSocketMessageType.Text, endOfMessage: true, ct);
    }
    
    private async Task<WireEnvelope?> ReceiveEnvelopeAsync(CancellationToken ct)
    {
        var buffer = new byte[8192];
        var result = await _socket.ReceiveAsync(buffer, ct);
        if (result.MessageType == WebSocketMessageType.Close) return null;
        
        string json = Encoding.UTF8.GetString(buffer, 0, result.Count);
        return JsonSerializer.Deserialize<WireEnvelope>(json);
    }

    public async Task SendPrivateMessageAsync(MessageData msg, CancellationToken ct)
    {
        await SendEnvelopeAsync(WirePacketType.DirectMessage, msg, ct);
        NovaLogger.LogInfo($"ServerConnectionWorker sending message...");
    }
    
    
    public async ValueTask DisposeAsync()
    {
        // TODO release managed resources here
    }
}