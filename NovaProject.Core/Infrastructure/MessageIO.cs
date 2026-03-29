using System;
using NovaProject.Core.Infrastructure.Structs;

namespace NovaProject.Core.Infrastructure;

public class MessageIo(string content, LocalUid sender, LocalUid recipient)
{
    public int Id { get; set; } = new Random().Next();
    public string Content { get; set; } = content;
    public LocalUid Sender { get; set; } = sender;
    public LocalUid Recipient { get; set; } = recipient;
    public DateTime Time { get; set; } = DateTime.Now;
    public bool Edited { get; set; } = false;
}