using System;
using Avalonia.Interactivity;

namespace NovaProject.Models.Events;

public class MessageReceivedEventArgs : RoutedEventArgs
{
    public string Message { get;}
    public User? Sender { get;}
    public DateTime SentAt { get; }

    public MessageReceivedEventArgs(string message)
    {
        this.Message = message;
        this.SentAt = DateTime.Now;
    }
    public MessageReceivedEventArgs(string message, User sender)
    {
        Message = message;
        Sender = sender;
        this.SentAt = DateTime.Now;
    }
}