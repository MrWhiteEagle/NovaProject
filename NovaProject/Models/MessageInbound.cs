namespace NovaProject.Models;

public class MessageInbound(string content, User sender, User recipient) : MessageIo(content, sender, recipient)
{
    
}