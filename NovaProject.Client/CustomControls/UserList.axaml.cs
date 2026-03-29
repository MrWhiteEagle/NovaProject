using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using NovaProject.Client.Services;
using NovaProject.Core.Infrastructure;
using NovaProject.Core.Infrastructure.Structs;
using NovaProject.CustomControls.ViewModels;
using NovaProject.Models.Events;

namespace NovaProject.CustomControls;

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
        
        //Raise Routed Event for chat switch
        if (b.DataContext is User u && this.DataContext is UserListViewModel)
        {
            App.ServiceProvider.GetRequiredService<ChatService>().SwitchConversation(u);
        }else if (b.DataContext is ServerData s && this.DataContext is UserListViewModel)
        {
            App.ServiceProvider.GetRequiredService<ChatService>().SwitchServer(s);
        }
        
        
    }

    private void UserTile_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is Border b)
        {
            b.Classes.Remove("pressed");
        }
    }
}