using Avalonia.Interactivity;
using NovaProject.Core.Infrastructure;

namespace NovaProject.Client.Models.Events;

public class OpenConversationEventArgs : RoutedEventArgs
{
    public User SelectedUserItem { get; set; }

    public OpenConversationEventArgs(RoutedEvent routedEvent, User selectedUserItem) :
        base(routedEvent) => SelectedUserItem = selectedUserItem;
}