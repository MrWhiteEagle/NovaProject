using System.Collections.ObjectModel;
using NovaProject.Core.Infrastructure;
using NovaProject.Core.Services;
using NovaProject.Models.Events;
using NovaProject.ViewModels;

namespace NovaProject.CustomControls.ViewModels;

public class ChatBodyViewModel : ViewModelBase
{
    private readonly User? _currentUser = AppGlobalService.CurrentUser;
    public User? Recipient;

    public ObservableCollection<MessageIo> Messages { get; set; } = new();

    public void UpdateMessageList(MessageSentEventArgs e)
    {
        if (_currentUser != null && Recipient != null)
                Messages.Add(
                    new MessageInbound(
                        e.Message,
                        _currentUser,
                        Recipient));
    }

    public void UpdateMessageList(MessageReceivedEventArgs e)
    {
        if (_currentUser != null && Recipient != null)
            Messages.Add(
                new MessageOutbound(
                    e.Message,
                    _currentUser,
                    Recipient));
    }
    public void ChangeUserContext(User user)
    {
        Recipient = user;
    }
}