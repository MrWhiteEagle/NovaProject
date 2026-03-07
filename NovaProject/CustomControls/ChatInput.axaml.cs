using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using NovaProject.CustomControls.ViewModels;
using NovaProject.Models.Events;

namespace NovaProject.CustomControls;

public partial class ChatInput : UserControl
{
    public ChatInput()
    {
        InitializeComponent();
        DataContextChanged += (s, e) =>
        {
            if (DataContext is ChatInputViewModel vm)
            {
                vm.OnKeybindMessageSent += (s, e) =>
                {
                    OnSendInternal();
                };
            }
        };
    }

    public static readonly RoutedEvent<MessageSentEventArgs> MessageSentEvent =
        RoutedEvent.Register<ChatInput, MessageSentEventArgs>(
            nameof(MessageSent), RoutingStrategies.Bubble);
    public static readonly RoutedEvent<MessageReceivedEventArgs> MessageReceivedEvent =
        RoutedEvent.Register<ChatInput, MessageReceivedEventArgs>(
            nameof(MessageReceived), RoutingStrategies.Bubble);
    
    public event EventHandler<MessageSentEventArgs>? MessageSent
    {
        add => AddHandler(MessageSentEvent, value);
        remove => RemoveHandler(MessageSentEvent, value);
    }
    
    public event EventHandler<MessageReceivedEventArgs>? MessageReceived
    {
        add => AddHandler(MessageReceivedEvent, value);
        remove => RemoveHandler(MessageReceivedEvent, value);
    }

    private void OnSendInternal()
    {
        var text = ChatInputBox.Text;
        if (string.IsNullOrWhiteSpace(text)) return;
        var args = new MessageSentEventArgs(text) { RoutedEvent = MessageSentEvent };
        RaiseEvent(args);
        //Testing for outbound messages display
        var outboundArgs = new MessageReceivedEventArgs(text) {RoutedEvent = MessageReceivedEvent };
        RaiseEvent(outboundArgs);
        ChatInputBox.Text = string.Empty;
    }
}