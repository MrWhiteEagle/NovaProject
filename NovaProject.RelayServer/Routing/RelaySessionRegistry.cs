using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Security.Cryptography;

namespace NovaProject.RelayServer.Routing;

public class RelaySessionRegistry
{
    private readonly ConcurrentDictionary<string, ConnectedClient> _clients = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, string> _publicKeyToTag = new();

    public string AllocateOrRetrieveTag(string username, byte[] clientPublicKey)
    {
        string publicKeyHex = Convert.ToHexString(clientPublicKey);

        if (_publicKeyToTag.TryGetValue(publicKeyHex, out var existingTag))
        {
            return existingTag;
        }

        string tag;
        do
        {
            tag = RandomNumberGenerator.GetInt32(1000, 9999).ToString();
        }
        while (_clients.ContainsKey($"{username}#{tag}"));
        
        _publicKeyToTag[publicKeyHex] = tag;
        return tag;
    }

    public void RegisterClient(ConnectedClient client)
    {
        _clients[client.FullHandle] = client;
        Console.WriteLine($"[+] Client registered: {client.FullHandle}");
    }

    public void RemoveSession(string fullHandle)
    {
        if (_clients.TryRemove(fullHandle, out _))
        {
            Console.WriteLine($"[-] Client disconnected: {fullHandle}");
        }
    }

    public bool TryGetRecipient(string fullHandle, out ConnectedClient? recipient)
    {
        return _clients.TryGetValue(fullHandle, out recipient);
    }
}

public record ConnectedClient(
    string Username,
    string AssignedTag,
    byte[] PublicKey,
    WebSocket Socket
)
{
    public string FullHandle => $"{Username}#{AssignedTag}";
}