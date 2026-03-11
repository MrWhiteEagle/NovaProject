using System.Collections.ObjectModel;
using NovaProject.Models;
using NovaProject.Models.Events;
using NovaProject.Services;
using NovaProject.ViewModels;

namespace NovaProject.CustomControls.ViewModels;

public class ChatBodyViewModel : ViewModelBase
{
    private User? _currentUser = AppGlobalService.CurrentUser;
    public User? Recipient;

    public ObservableCollection<MessageIo> Messages { get; set; } = new();

    public void UpdateMessageList(MessageSentEventArgs e)
    {
        Messages.Add(
            new MessageInbound(
                e.Message,
                _currentUser,
                Recipient));
    }

    public void UpdateMessageList(MessageReceivedEventArgs e)
    {
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