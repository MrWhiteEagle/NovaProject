using NovaProject.Core.Infrastructure.Local;

namespace NovaProject.Core.Infrastructure.Ui;

public class TextMessageIncoming(string text, LocalUid sender, LocalUid recipient) : MessageIo(text, sender, recipient)
{
    public void EditMessage(string newContent)
    {
        this.Text = newContent;
        this.Edited = true;
    }
}

public class TextMessageOutgoing(string text, LocalUid sender, LocalUid recipient) : MessageIo(text, sender, recipient)
{
    
}