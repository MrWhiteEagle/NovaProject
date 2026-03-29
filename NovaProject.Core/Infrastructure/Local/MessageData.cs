using System.Text.Json.Serialization;

namespace NovaProject.Core.Infrastructure.Structs;

public struct MessageData
{
    [JsonPropertyName("Message")]
    public string Message { get; init; }
    
    [JsonPropertyName("Sender")]
    public LocalUid Sender { get; init; }
    
    [JsonPropertyName("Recipient")]
    public LocalUid Recipient { get; init; }
    
    [JsonConstructor]
    public MessageData(string message, LocalUid sender, LocalUid recipient)
    {
        this.Message = message;
        this.Sender = sender;
        this.Recipient = recipient;
    }
}