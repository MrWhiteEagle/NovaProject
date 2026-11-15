using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using NovaProject.Core.Infrastructure.Data.Shared;
using NovaProject.Core.Infrastructure.Network;
using NovaProject.RelayServer.Security;

namespace NovaProject.RelayServer.Routing;

public class ClientConnectionHandler(ServerIdentityProvider serverIdentity, RelaySessionRegistry registry)
{
    public async Task HandleConnectionAsync(WebSocket socket, CancellationToken ct)
    {
        ConnectedClient? authenticatedClient = null;

        try
        {
            //STAGE 1: Receive ClientHello, respond with ServerProof
            var helloEnvelope = await ReceiveEnvelopeAsync(socket, ct);
            if (helloEnvelope?.Type != WirePacketType.ClientHello)
            {
                await socket.CloseAsync(WebSocketCloseStatus.ProtocolError,
                    "Expected ClientHello, socket needs verification first.", ct);
                return;
            }

            var clientHello = JsonSerializer.Deserialize<ClientHelloDto>(helloEnvelope.PayloadJson)!;
            byte[] serverNonce = RandomNumberGenerator.GetBytes(32);
            byte[] serverSignature = serverIdentity.SignChallenge(clientHello.Nonce);

            await SendEnvelopeAsync(socket, WirePacketType.ServerProof,
                new ServerProofDto(serverNonce, serverIdentity.PublicKey, serverSignature), ct);

            //STAGE 2:
            var authEnvelope = await ReceiveEnvelopeAsync(socket, ct);
            if (authEnvelope?.Type != WirePacketType.ClientAuthProof)
            {
                await socket.CloseAsync(WebSocketCloseStatus.ProtocolError,
                    "Expected ClientAuthProof, socket needs verification first.", ct);
                return;
            }

            var clientProof = JsonSerializer.Deserialize<ClientAuthProofDto>(authEnvelope.PayloadJson)!;
            using var clientEcdsa = ECDsa.Create();

            clientEcdsa.ImportSubjectPublicKeyInfo(clientProof.ClientPublicKey, out _);

            if (!clientEcdsa.VerifyData(serverNonce, clientProof.Signature, HashAlgorithmName.SHA256))
            {
                await SendEnvelopeAsync(socket, WirePacketType.ServerAuthAck,
                    new ServerAuthAckDto(clientProof.RequestedUsername, "", false,
                        "Client signature failed verification"), ct);
                await socket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Authentication failed.", ct);
                return;
            }

            string assignedTag =
                registry.AllocateOrRetrieveTag(clientProof.RequestedUsername, clientProof.ClientPublicKey);
            authenticatedClient = new ConnectedClient(clientProof.RequestedUsername, assignedTag,
                clientProof.ClientPublicKey, socket);
            registry.RegisterClient(authenticatedClient);

            await SendEnvelopeAsync(socket, WirePacketType.ServerAuthAck,
                new ServerAuthAckDto(authenticatedClient.Username, assignedTag, true), ct);

            //STAGE 3: Message Loop
            while (socket.State == WebSocketState.Open && !ct.IsCancellationRequested)
            {
                var envelope = await ReceiveEnvelopeAsync(socket, ct);
                if (envelope == null) break;

                if (envelope.Type == WirePacketType.DirectMessage)
                {
                    var msg = JsonSerializer.Deserialize<MessageData>(envelope.PayloadJson);
                    await RouteDirectMessageAsync(authenticatedClient, msg, ct);
                }
            }
        }
        catch (WebSocketException e)
        {
            Console.WriteLine($"[RELAY] Cancelled processing connection. Socket is closed. {e.Message}");
        }
        catch (Exception e)
        {
            Console.WriteLine(
                $"[RELAY] Encountered an unexpected error while processing client connection: {e.Message}");
        }
        finally
        {
            if (authenticatedClient != null)
            {
                registry.RemoveSession(authenticatedClient.FullHandle);
            }
        }
    }

    private async Task RouteDirectMessageAsync(ConnectedClient client, MessageData msg, CancellationToken ct)
    {
        var recipientUid = LocalUid.Parse(msg.ConversationId);
        string targetHandle = $"{recipientUid.Name}#{recipientUid.Tag}";
        
        Console.WriteLine($"[RELAY] {client.FullHandle} -> {targetHandle}: \"{msg.Content}\"");
        // if (registry.TryGetRecipient(targetHandle, out var recipient) && recipient?.Socket.State == WebSocketState.Open)
        // {
        //     var routedMessage = msg with
        //     {
        //         ConversationId = $"{client.FullHandle}#{client.AssignedTag}@{recipientUid.RelayAddress}",
        //         Timestamp = DateTime.UtcNow
        //     };
        //     
        //     await SendEnvelopeAsync(recipient.Socket, WirePacketType.DirectMessage, routedMessage, ct);
        //     Console.WriteLine($"[RELAY] Routed to {recipient.FullHandle} (DELIVERED).");
        // }
        // else
        // {
        //     Console.WriteLine($"[RELAY] {targetHandle} not found or offline. (FAIL)");
        //     await SendEnvelopeAsync(client.Socket, WirePacketType.DeliveryError,
        //         $"User '{targetHandle}' doesn't exist or is offline.", ct);
        // }
        var bounce = msg with
        {
            Timestamp = DateTime.UtcNow,
            Content = $"BOUNCED FROM RELAY: {msg.Content}"
        };
        await SendEnvelopeAsync(client.Socket, WirePacketType.DirectMessage, bounce, ct);
        Console.WriteLine($"[RELAY] BOUNCED MESSAGE");
    }

    private static async Task SendEnvelopeAsync<T>(WebSocket socket, WirePacketType type, T payload,
        CancellationToken ct)
    {
        var envelope = new WireEnvelope(type, JsonSerializer.Serialize(payload));
        byte[] bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(envelope));
        await socket.SendAsync(bytes, WebSocketMessageType.Text, endOfMessage: true, ct);
    }

    private static async Task<WireEnvelope?> ReceiveEnvelopeAsync(WebSocket socket, CancellationToken ct)
    {
        var buffer = new byte[8192];
        var result = await socket.ReceiveAsync(buffer, ct);
        if (result.MessageType == WebSocketMessageType.Close) return null;

        string json = Encoding.UTF8.GetString(buffer, 0, result.Count);
        return JsonSerializer.Deserialize<WireEnvelope>(json);
    }
}