using Avalonia.Interactivity;
using NovaProject.Core.Infrastructure.Local;

namespace NovaProject.Client.Models.Events;

public class CallRequestEventArgs : RoutedEventArgs
{
    public LocalUid CallRecipient { get; set; }
    public CallRequestEventArgs(RoutedEvent routedEvent, LocalUid callRecipient) : base(routedEvent) => CallRecipient = callRecipient;
}