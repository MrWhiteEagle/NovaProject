using System;
using Avalonia.Interactivity;

namespace NovaProject.Models.Events;

public partial class MessageSentEventArgs(string message) : RoutedEventArgs
{
    public string Message { get;} = message;
}