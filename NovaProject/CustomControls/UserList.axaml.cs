using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using NovaProject.Models;
using NovaProject.Models.Events;
using NovaProject.ViewModels;

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
        if (b.DataContext is User u && this.DataContext is UserListViewModel vm)
        {
            var args = new OpenConversationEventArgs(OpenConversationEvent, u);
            RaiseEvent(args);
        }else if (b.DataContext is ServerData s && this.DataContext is UserListViewModel vm2)
        {
            Console.WriteLine("UserListOK");
            var args = new OpenServerEventArgs(OpenServerEvent, s);
            RaiseEvent(args);
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