using System.Runtime.CompilerServices;
using NovaProject.Core.Infrastructure.Local;

namespace NovaProject.Core.Infrastructure.Ui;

public class ImageTextMessage : TextMessageIncoming
{
    public ImageTextMessage(string text, string url, LocalUid sender, LocalUid recipient) : base(text, sender, recipient)
    {
        this.Content = url;
    }
}

public class ImageMessageOutgoing : TextMessageOutgoing
{
    public ImageMessageOutgoing(string text, string url, LocalUid sender, LocalUid recipient) : base(text, sender, recipient)
    {
        this.Content = url;
    }
}