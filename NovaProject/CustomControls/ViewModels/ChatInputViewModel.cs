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

    public event EventHandler? OnKeybindMessageSent;

    [RelayCommand]
    private void SendMessage()
    {
        Console.WriteLine("Tried to send a message via keybind");
        OnKeybindMessageSent?.Invoke(this, EventArgs.Empty);
    }
}