using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using NovaProject.Core.Infrastructure.Local;

namespace NovaProject.Client.CustomControls.ChatElements;

public partial class ChatBubble : UserControl
{
    public static readonly StyledProperty<string> MessageTextProperty = 
        AvaloniaProperty.Register<ChatBubble, string>(nameof(MessageText));
    public static readonly StyledProperty<DateTime> SentAtProperty =
        AvaloniaProperty.Register<ChatBubble, DateTime>(nameof(SentAt));
    public static readonly StyledProperty<MessageData.MessageDirection> MessageDirectionProperty =
        AvaloniaProperty.Register<ChatBubble, MessageData.MessageDirection>(nameof(MessageDirection));
    public ChatBubble()
    {
        InitializeComponent();
    }

    public string MessageText
    {
        get => GetValue(MessageTextProperty);
        set => SetValue(MessageTextProperty, value);
    }
    public DateTime SentAt
    {
        get => GetValue(SentAtProperty);
        set => SetValue(SentAtProperty, value);
    }

    public MessageData.MessageDirection MessageDirection
    {
        get => GetValue(MessageDirectionProperty);
        set => SetValue(MessageDirectionProperty, value);
    }

    public bool IsIncoming
    {
        get => MessageDirection == MessageData.MessageDirection.Incoming ?  true : false;
    }
    private void InputElement_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        EditMessageButton.IsVisible = true;
        RespondToMessageButton.IsVisible = true;
        DeleteMessageButton.IsVisible = true;
        MoreMessageActionsButton.IsVisible = true;
        TimeReceivedText.IsVisible = true;
    }

    private void InputElement_OnPointerExited(object? sender, PointerEventArgs e)
    {
        EditMessageButton.IsVisible = false;
        RespondToMessageButton.IsVisible = false;
        DeleteMessageButton.IsVisible = false;
        MoreMessageActionsButton.IsVisible = false;
        TimeReceivedText.IsVisible = false;
    }
}