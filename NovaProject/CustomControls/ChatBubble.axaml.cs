using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NovaProject.CustomControls;

public partial class ChatBubble : UserControl
{
    public static readonly StyledProperty<string> MessageContentProperty = 
        AvaloniaProperty.Register<ChatBubble, string>(nameof(MessageContent));
    public ChatBubble()
    {
        InitializeComponent();
    }

    public string MessageContent
    {
        get => GetValue(MessageContentProperty);
        set => SetValue(MessageContentProperty, value);
    }
    private void InputElement_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        EditMessageButton.IsVisible = true;
        RespondToMessageButton.IsVisible = true;
        DeleteMessageButton.IsVisible = true;
        MoreMessageActionsButton.IsVisible = true;
    }

    private void InputElement_OnPointerExited(object? sender, PointerEventArgs e)
    {
        EditMessageButton.IsVisible = false;
        RespondToMessageButton.IsVisible = false;
        DeleteMessageButton.IsVisible = false;
        MoreMessageActionsButton.IsVisible = false;
    }
}