using NovaProject.Core.Infrastructure.Structs;

namespace NovaProject.Core.Infrastructure;

public class MessageIncoming(string content, LocalUid sender, LocalUid recipient) : MessageIo(content, sender, recipient)
{
    public void EditMessage(string newContent)
    {
        this.Content = newContent;
        this.Edited = true;
        //Emit signal on edit
    }
}