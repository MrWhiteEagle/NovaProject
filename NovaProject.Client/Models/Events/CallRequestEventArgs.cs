using Avalonia.Interactivity;
using NovaProject.Core.Infrastructure;

namespace NovaProject.Models.Events;

public class CallRequestEventArgs : RoutedEventArgs
{
    public User CallRecipient { get; set; }
    public CallRequestEventArgs(RoutedEvent routedEvent, User callRecipient) : base(routedEvent) => CallRecipient = callRecipient;
}