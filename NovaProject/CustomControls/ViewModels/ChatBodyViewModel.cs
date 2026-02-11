using System.Collections.Generic;
using System.Collections.ObjectModel;
using NovaProject.Models.Events;
using NovaProject.ViewModels;

namespace NovaProject.CustomControls.ViewModels;

public class ChatBodyViewModel : ViewModelBase
{

    public ObservableCollection<string> Messages { get; set; } = new();
    public ChatBodyViewModel()
    {
        
    }

    public void UpdateMessageList(MessageSentEventArgs e)
    {
        Messages.Add(e.Message);
    }
}