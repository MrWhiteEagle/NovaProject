
using NovaProject.Core.Infrastructure.Local;

namespace NovaProject.Core.Infrastructure.Ui;

public class MessageIo(string text, LocalUid sender, LocalUid recipient)
{
    public int Id { get; set; } = new Random().Next();
    public string Text { get; set; } = text;
    public string Content { get; set; } = "";
    public LocalUid Sender { get; set; } = sender;
    public LocalUid Recipient { get; set; } = recipient;
    public DateTime Time { get; set; } = DateTime.Now;
    public bool Edited { get; set; } = false;
}