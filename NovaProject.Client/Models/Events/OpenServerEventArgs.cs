using Avalonia.Interactivity;
using NovaProject.Core.Infrastructure;

namespace NovaProject.Client.Models.Events;

public class OpenServerEventArgs : RoutedEventArgs
{
    public ServerData SelectedServerData { get; set; }

    public OpenServerEventArgs(RoutedEvent routedEvent, ServerData selectedServerData) :
        base(routedEvent) => SelectedServerData = selectedServerData;
}