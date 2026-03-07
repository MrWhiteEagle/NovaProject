using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace NovaProject.CustomControls;

public partial class ChatBubbleOther : UserControl
{
    public static readonly StyledProperty<string> MessageContentProperty = 
        AvaloniaProperty.Register<ChatBubble, string>(nameof(MessageContent));

    public static readonly StyledProperty<DateTime> SentAtProperty =
        AvaloniaProperty.Register<ChatBubble, DateTime>(nameof(SentAt));
    public ChatBubbleOther()
    {
        InitializeComponent();
    }

    public string MessageContent
    {
        get => GetValue(MessageContentProperty);
        set => SetValue(MessageContentProperty, value);
    }

    public DateTime SentAt
    {
        get => GetValue(SentAtProperty);
        set => SetValue(SentAtProperty, value);
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