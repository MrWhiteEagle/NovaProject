using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using NovaProject.CustomControls;
using NovaProject.Models.Events;
using NovaProject.ViewModels;

namespace NovaProject.Views;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _vm;
    public MainWindow()
    {
        InitializeComponent();
        _vm = new MainWindowViewModel();
        DataContext = _vm;
        this.Loaded += MainWindow_OnLoaded;
        AddHandler(UserList.OpenConversationEvent, OnOpenConversationRequest, RoutingStrategies.Bubble);
        AddHandler(UserList.OpenServerEvent, OnOpenServerRequest, RoutingStrategies.Bubble);
        AddHandler(ChatTitlebar.CallRequestEvent, OnCallOutboundRequest, RoutingStrategies.Bubble);
    }

    private void MainWindow_OnLoaded(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("MainWindow_OnLoaded");
        _vm.SetupTabs();
    }

    private void ChatField_OnMessageSent(object? sender, MessageSentEventArgs e)
    {
        _vm.UpdateMessagesRequest(e);
    }

    private void ChatField_OnMessageReceived(object? sender, MessageReceivedEventArgs e)
    {
        _vm.UpdateMessagesRequest(e);
    }

    private void OnOpenConversationRequest(object? sender, OpenConversationEventArgs e)
    {
        _vm.OpenConversationRequest(e);
    }

    private void OnCallOutboundRequest(object? sender, CallRequestEventArgs e)
    {
        _vm.OpenOutboundCallRequest(e);
    }

    private void OnOpenServerRequest(object? sender, OpenServerEventArgs e)
    {
        _vm.OpenServerRequest(e);
    }
}