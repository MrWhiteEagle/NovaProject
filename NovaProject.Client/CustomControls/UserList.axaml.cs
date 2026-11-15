using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using NovaProject.Client.CustomControls.ViewModels;
using NovaProject.Client.Models.Events;
using NovaProject.Client.Services;
using NovaProject.Core.Infrastructure;
using NovaProject.Core.Infrastructure.Data.Shared;

namespace NovaProject.Client.CustomControls;

public partial class UserList : UserControl
{
    public UserList()
    {
        InitializeComponent();
    }
    public static readonly RoutedEvent<OpenConversationEventArgs> OpenConversationEvent =
        RoutedEvent.Register<UserListViewModel, OpenConversationEventArgs>(
            nameof(OpenConversationEvent), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<OpenServerEventArgs> OpenServerEvent =
        RoutedEvent.Register<UserListViewModel, OpenServerEventArgs>(
            nameof(OpenServerEvent), RoutingStrategies.Bubble);
    
    //Pressed state
    private void UserTile_PointerDown(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Border b) return;
        b.Classes.Add("pressed");

        if (this.DataContext is UserListViewModel vm)
        {
            if (b.DataContext is LocalUid uid)
            {
                vm.LoadAndSwitchConversation(uid);
            }
        }
    }

    private void UserTile_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is not Border b) return;
            b.Classes.Remove("pressed");
    }
}