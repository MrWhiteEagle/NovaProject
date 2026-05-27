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
            Messages.Add(new TextMessageIncoming(
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
            Messages.Add(new TextMessageOutgoing(
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