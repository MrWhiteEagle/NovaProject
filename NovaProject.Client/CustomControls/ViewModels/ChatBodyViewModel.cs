using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using NovaProject.Client.Services;
using NovaProject.Core.Infrastructure;
using NovaProject.Core.Infrastructure.Structs;
using NovaProject.Core.Services;
using NovaProject.ViewModels;

namespace NovaProject.CustomControls.ViewModels;

public class ChatBodyViewModel : ViewModelBase
{
    private readonly LocalUid? _currentUser = AppGlobalService.LocalUid;
    public LocalUid? Recipient;

    public ObservableCollection<MessageIo> Messages { get; set; } = new();

    public ChatBodyViewModel()
    {
        App.ServiceProvider.GetRequiredService<ChatService>().OnMessageReceived += OnMessageReceived;
        App.ServiceProvider.GetRequiredService<ChatService>().OnMessageSent += OnMessageSent;
    }

    private void OnMessageReceived(MessageData data)
    {
        if (data.Sender == Recipient)
        {
            Messages.Add(new MessageIncoming(
                data.Message,
                data.Sender,
                _currentUser!
                ));
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
            Messages.Add(new MessageOutgoing(
                data.Message,
                data.Sender,
                data.Recipient));
        }
    }
    public void ChangeUserContext(LocalUid user)
    {
        Recipient = user;
    }
}