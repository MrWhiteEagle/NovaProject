using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using NovaProject.Models.Events;

namespace NovaProject.CustomControls;

public partial class ChatInput : UserControl
{
    public ChatInput()
    {
        InitializeComponent();
    }

    public static readonly RoutedEvent<MessageSentEventArgs> MessageSentEvent =
        RoutedEvent.Register<ChatInput, MessageSentEventArgs>(
            nameof(MessageSent), RoutingStrategies.Bubble);
    
    public event EventHandler<MessageSentEventArgs>? MessageSent
    {
        add => AddHandler(MessageSentEvent, value);
        remove => RemoveHandler(MessageSentEvent, value);
    }

    private void OnSendInternal()
    {
        var text = ChatInputBox.Text;
        if (string.IsNullOrWhiteSpace(text)) return;
        var args = new MessageSentEventArgs(text) { RoutedEvent = MessageSentEvent };
        RaiseEvent(args);
        ChatInputBox.Text = string.Empty;
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        OnSendInternal();
    }
}