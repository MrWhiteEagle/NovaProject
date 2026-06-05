using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using NovaProject.Client.Services;
using NovaProject.Client.ViewModels;
using NovaProject.Core.Infrastructure.Local;
using NovaProject.Core.Infrastructure.Ui;
using NovaProject.Core.Services;

namespace NovaProject.Client.CustomControls.ViewModels;

public class ChatBodyViewModel : ViewModelBase
{
    private readonly LocalUid? _currentUser = AppGlobalService.LocalUid;
    public LocalUid? Recipient;

    public ObservableCollection<MessageData> Messages { get; set; } = new();

    public ChatBodyViewModel()
    {
        App.ServiceProvider.GetRequiredService<ChatService>().OnMessageReceived += OnMessageReceived;
        App.ServiceProvider.GetRequiredService<ChatService>().OnMessageSent += OnMessageSent;
    }

    private void OnMessageReceived(MessageData data)
    {
        if (data.Sender == Recipient)
        {
            // Messages.Add(new TextMessageIncoming(
            //     data.Content,
            //     data.Sender,
            //     _currentUser!
            //     ));
            Messages.Add(data);
        }
        else
        {
            Logger.LogInfo("Sender didnt match with " + Recipient.UserName + ". Was: " + data.Sender.UserName);
        }
    }

    private void OnMessageSent(MessageData data)
    {
        if (data.Recipient == Recipient)
        {
            // Messages.Add(new TextMessageOutgoing(
            //     data.Content,
            //     data.Sender,
            //     data.Recipient));
            Messages.Add(data);
        }
    }

    private void OnHistoryUpdated(List<MessageData> history)
    {
        Messages.Clear();
        foreach (var message in history)
        {
            
        }
    }
    public void ChangeUserContext(LocalUid user)
    {
        Recipient = user;
    }
}