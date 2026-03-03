using System.Collections.Generic;
using System.Collections.ObjectModel;
using NovaProject.Models;
using NovaProject.Models.Events;
using NovaProject.ViewModels;

namespace NovaProject.CustomControls.ViewModels;

public class ChatBodyViewModel : ViewModelBase
{
    private User? _currentUser;
    private User? _recipient;

    public ObservableCollection<MessageIo> Messages { get; set; } = new();
    public ChatBodyViewModel()
    {
        
    }

    public void UpdateMessageList(MessageSentEventArgs e)
    {
        Messages.Add(
            new MessageInbound(
                e.Message,
                new User(
                    "PlaceholderSender",
                    "placeholder_sender",
                    "1111"),
                new User(
                    "placeholder_recipient",
                    "placeholder_recipient",
                    "2222")));
    }
}