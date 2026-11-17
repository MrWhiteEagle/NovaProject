using System;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using NovaProject.Client.Services;
using NovaProject.Client.ViewModels;
using NovaProject.Core.Infrastructure.Data.Shared;
using NovaProject.Core.Infrastructure.Local;
using NovaProject.Core.Services;
using NovaProject.CustomControls;

namespace NovaProject.Client.CustomControls.ViewModels;

public partial class ChatInputViewModel : ViewModelBase
{
    private readonly ChatService _chatService;
    private LocalUid? _conversationIdContext;

    public event Action<MessageData>? MessageSent;
    
    [ObservableProperty]
    private string _inputText = string.Empty;

    public ChatInputViewModel(ChatService chatService)
    {
        _chatService = chatService;
    }
    
    [RelayCommand]
    private void SendMessage()
    {
        if (string.IsNullOrWhiteSpace(InputText) || _conversationIdContext == null) return;

        var message = new MessageData()
        {
            Content = InputText,
            ConversationId = _conversationIdContext.ToString(),
            IsOutbound = true
        };
        
        _chatService.SendMessage(message);
        
        Dispatcher.UIThread.Post(() =>
        {
            InputText = string.Empty;
        });
    }

    public void ChangeConversationContext(LocalUid conversationIdContext)
    {
        this._conversationIdContext = conversationIdContext;
    }
}