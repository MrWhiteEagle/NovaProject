using Avalonia.Interactivity;

namespace NovaProject.Models.Events;

public class CallRequestEventArgs : RoutedEventArgs
{
    public User CallRecipient { get; set; }
    public CallRequestEventArgs(RoutedEvent routedEvent, User callRecipient) : base(routedEvent) => CallRecipient = callRecipient;
}