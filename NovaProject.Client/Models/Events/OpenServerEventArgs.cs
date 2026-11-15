using Avalonia.Interactivity;
using NovaProject.Core.Infrastructure;
using NovaProject.Core.Infrastructure.Data.Shared;

namespace NovaProject.Client.Models.Events;

public class OpenServerEventArgs : RoutedEventArgs
{
    public ServerData SelectedServerData { get; set; }

    public OpenServerEventArgs(RoutedEvent routedEvent, ServerData selectedServerData) :
        base(routedEvent) => SelectedServerData = selectedServerData;
}