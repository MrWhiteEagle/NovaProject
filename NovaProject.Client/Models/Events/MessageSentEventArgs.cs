using System;
using Avalonia.Interactivity;
using NovaProject.Core.Infrastructure;

namespace NovaProject.Models.Events;

public partial class MessageSentEventArgs : RoutedEventArgs
{
    public string Message { get;}
    public User? Recipient { get;}
    public DateTime SentAt { get;}

    public MessageSentEventArgs(string message)
    {
        this.Message = message;
        this.SentAt = DateTime.Now;
    }
    public MessageSentEventArgs(string message, User recipient)
    {
        Message = message;
        Recipient = recipient;
        this.SentAt = DateTime.Now;
    }
}