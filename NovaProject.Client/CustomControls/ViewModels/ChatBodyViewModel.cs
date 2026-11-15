using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NovaProject.Client.Services;
using NovaProject.Client.ViewModels;
using NovaProject.Core.Infrastructure.Data.Shared;
using NovaProject.Core.Services;

namespace NovaProject.Client.CustomControls.ViewModels;

public class ChatBodyViewModel : ViewModelBase
{
    private readonly ChatService _chatService;
    private readonly LocalUid _conversationRecipientContext;
    public ObservableCollection<MessageData> Messages { get; set; } = new();

    public ChatBodyViewModel(LocalUid conversationRecipientContext, ChatService chatService)
    {
        _chatService = chatService;
        _conversationRecipientContext = conversationRecipientContext;
    }
}