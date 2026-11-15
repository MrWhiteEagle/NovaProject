using Avalonia.Interactivity;
using NovaProject.Core.Infrastructure;
using NovaProject.Core.Infrastructure.Data.Shared;

namespace NovaProject.Client.Models.Events;

public class OpenConversationEventArgs : RoutedEventArgs
{
    public LocalUid SelectedUserItem { get; set; }

    public OpenConversationEventArgs(RoutedEvent routedEvent, LocalUid selectedUserItem) :
        base(routedEvent) => SelectedUserItem = selectedUserItem;
}