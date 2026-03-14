using System;

namespace NovaProject.Core.Infrastructure;

public class MessageIo(string content, User sender, User recipient)
{
    public int Id { get; set; } = new Random().Next();
    public string Content { get; set; } = content;
    public User Sender { get; set; } = sender;
    public User Recipient { get; set; } = recipient;
    public DateTime Time { get; set; } = DateTime.Now;
    public bool Edited { get; set; } = false;
}