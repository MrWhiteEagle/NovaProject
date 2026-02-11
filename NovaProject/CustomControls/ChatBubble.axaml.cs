using System;
using Avalonia;
using Avalonia.Controls;
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
}