using System;
using Avalonia.Interactivity;

namespace NovaProject.Models.Events;

public partial class MessageSentEventArgs : RoutedEventArgs
{
    public string Message { get;}
    public User? Sender { get;}
    public User? Recipient { get;}
    public bool Incoming { get; set; } = false;

    public MessageSentEventArgs(string message)
    {
        this.Message = message;
    }
    public MessageSentEventArgs(string message, User sender, User recipient, bool incoming = false)
    {
        Message = message;
        Sender = sender;
        Recipient = recipient;
        this.Incoming = incoming;
    }
}