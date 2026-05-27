using System.Text.Json.Serialization;

namespace NovaProject.Core.Infrastructure.Local;

public struct MessageData
{
    [JsonPropertyName("Message")]
    public string Message { get; init; }
    
    [JsonPropertyName("Sender")]
    public LocalUid Sender { get; init; }
    
    [JsonPropertyName("Recipient")]
    public LocalUid Recipient { get; init; }
    
    [JsonPropertyName("Content")]
    public string Content { get; init; }
    
    [JsonConstructor]
    public MessageData(string message, LocalUid sender, LocalUid recipient, string content = "")
    {
        this.Message = message;
        this.Sender = sender;
        this.Recipient = recipient;
        this.Content = content;
    }
    
    
}