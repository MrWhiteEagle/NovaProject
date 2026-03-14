using System;
using CommunityToolkit.Mvvm.Input;
using NovaProject.ViewModels;

namespace NovaProject.CustomControls.ViewModels;

public partial class ChatInputViewModel : ViewModelBase
{

    public event EventHandler? OnKeybindMessageSent;

    [RelayCommand]
    private void SendMessage()
    {
        Console.WriteLine("Tried to send a message via keybind");
        OnKeybindMessageSent?.Invoke(this, EventArgs.Empty);
    }
}