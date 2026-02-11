using System;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using NovaProject.Models.Events;
using NovaProject.ViewModels;

namespace NovaProject.CustomControls.ViewModels;

public partial class ChatInputViewModel : ViewModelBase
{
    
    public ChatInputViewModel()
    {
        
    }

    [RelayCommand]
    public void SendMessage()
    {
        Console.WriteLine("Tried to send a message via keybind");
    }
    public event EventHandler<MessageSentEventArgs>? OnMessageSent;
}