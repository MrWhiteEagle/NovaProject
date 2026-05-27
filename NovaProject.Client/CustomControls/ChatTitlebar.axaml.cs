using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using NovaProject.Client.CustomControls.ViewModels;
using NovaProject.Client.Models.Events;

namespace NovaProject.Client.CustomControls;

public partial class ChatTitlebar : UserControl
{
    public static readonly RoutedEvent<CallRequestEventArgs> CallRequestEvent =
        RoutedEvent.Register<ChatTitlebar, CallRequestEventArgs>(nameof(CallRequestEvent), RoutingStrategies.Bubble);
    
    public ChatTitlebar()
    {
        InitializeComponent();
    }

    private void CallButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("[ChatTitleBar] Call requested, raising event...");
        if (DataContext is not ChatTitlebarViewModel viewModel) return;
        var args = new CallRequestEventArgs(CallRequestEvent, viewModel.UserContext);
        RaiseEvent(args);
    }
}