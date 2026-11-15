using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using NovaProject.Core.Infrastructure.Data.Shared;
using NovaProject.Core.Infrastructure.Local;

namespace NovaProject.Client.CustomControls.ChatElements;

public partial class ChatBubble : UserControl
{
    public static readonly StyledProperty<MessageData> MessageDataContextProperty = 
        AvaloniaProperty.Register<ChatBubble, MessageData>(nameof(MessageDataContext));
    public static readonly StyledProperty<bool> AreActionsVisibleProperty =
        AvaloniaProperty.Register<ChatBubble, bool>(nameof(AreActionsVisible));
    public static readonly StyledProperty<LocalUid> SenderContextProperty =
        AvaloniaProperty.Register<ChatBubble, LocalUid>(nameof(SenderContext));
    public ChatBubble()
    {
        InitializeComponent();
        AreActionsVisible = false;
    }
    
    public MessageData MessageDataContext
    {
        get => GetValue(MessageDataContextProperty);
        set => SetValue(MessageDataContextProperty, value);
    }

    public bool AreActionsVisible
    {
        get => GetValue(AreActionsVisibleProperty);
        set => SetValue(AreActionsVisibleProperty, value);
    }

    public LocalUid SenderContext
    {
        get => GetValue(SenderContextProperty);
        set => SetValue(SenderContextProperty, value);
    }
    

    public bool IsIncoming
    {
        get => !MessageDataContext.IsOutbound;
    }
    private void InputElement_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        if(!IsIncoming)
            AreActionsVisible = true;
    }

    private void InputElement_OnPointerExited(object? sender, PointerEventArgs e)
    {
        AreActionsVisible = false;
    }
}