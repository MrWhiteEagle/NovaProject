using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using NovaProject.Client.Services;
using NovaProject.Client.ViewModels;
using NovaProject.Core.Infrastructure.Data.Shared;
using NovaProject.Core.Services;

namespace NovaProject.Client.CustomControls.ViewModels;

public partial class ChatBodyViewModel : ViewModelBase
{
    private readonly ChatService _chatService;
    private readonly LocalUid _conversationRecipientContext;
    
    [ObservableProperty] private bool _isConversationLoaded;
    
    public ObservableCollection<MessageData> Messages { get; set; } = new();

    public ChatBodyViewModel(LocalUid conversationRecipientContext, ChatService chatService)
    {
        _chatService = chatService;
        _conversationRecipientContext = conversationRecipientContext;
    }

    public void UpdateChatHistory(List<MessageData>? messages)
    {
        if (messages?.FirstOrDefault().ConversationId != _conversationRecipientContext.ToString()) return;
        NovaLogger.LogInfo("Received a chatbody conversatio update for: " + _conversationRecipientContext.ToString());
        Messages.Clear();
        Messages = new ObservableCollection<MessageData>(messages);
        IsConversationLoaded = true;
    }
}