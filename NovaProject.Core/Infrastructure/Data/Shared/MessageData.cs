using System.Text.Json.Serialization;

namespace NovaProject.Core.Infrastructure.Data.Shared;

public struct MessageData
{
    [JsonPropertyName("conversationId")]
    public string ConversationId { get; init; }
    [JsonPropertyName("Content")]
    public string Content { get; init; }
    [JsonPropertyName("Timestamp")]
    public DateTime Timestamp { get; init; }
    [JsonPropertyName("Direction")]
    public bool IsOutbound { get; init; }
    
    public MessageData(
        string conversationId,
        bool isOutbound,
        string content, 
        DateTime? timestamp = null)
    {
        this.ConversationId = conversationId;
        this.Content = content;
        this.IsOutbound = isOutbound;
        this.Timestamp = timestamp ?? DateTime.UtcNow;
    }

    public enum MessageContentType
    {
        Text,
        Image,
        Video,
        Audio,
        Link,
    }
    
    public enum MessageDirection
    {
        Incoming,
        Outgoing
    }
}