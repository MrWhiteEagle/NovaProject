using System.Text.Json.Serialization;

namespace NovaProject.Core.Infrastructure.Local;

public struct MessageData
{
    [JsonPropertyName("Sender")]
    public LocalUid Sender { get; init; }
    
    [JsonPropertyName("Recipient")]
    public LocalUid Recipient { get; init; }
    
    [JsonPropertyName("Content")]
    public string Content { get; init; }
    [JsonPropertyName("Timestamp")]
    public DateTime Timestamp { get; init; }
    [JsonPropertyName("Edited")]
    public bool Edited { get; init; }
    [JsonPropertyName("ContentType")]
    public MessageContentType ContentType { get; init; }

    [JsonPropertyName("Direction")]
    public MessageDirection Direction { get; init; }
    
    public MessageData(
        LocalUid sender, 
        LocalUid recipient, 
        string content = "", 
        DateTime? timestamp = null, 
        bool edited = false,  
        MessageContentType contentType = MessageContentType.Text, 
        MessageDirection direction = MessageDirection.Outgoing)
    {
        this.Sender = sender;
        this.Recipient = recipient;
        this.Content = content;
        this.Timestamp = timestamp ?? DateTime.UtcNow;
        this.Edited = edited;
        this.ContentType = contentType;
        this.Direction = direction;
    }

    public enum MessageContentType
    {
        Text,
        Image,
        Video,
        Audio,
        Gif,
        Link,
    }
    
    public enum MessageDirection
    {
        Incoming,
        Outgoing
    }
}