using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NovaProject.CustomControls.ViewModels;
using NovaProject.Models;
using NovaProject.Models.Events;

namespace NovaProject.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public ChatInputViewModel Input { get; set; } = new();
    public ChatBodyViewModel Body { get; set; } = new();
    
    [ObservableProperty] private bool _isPaneOpen = false;

    [RelayCommand]
    private void OpenPane()
    {
        IsPaneOpen = !IsPaneOpen;
    }

    public void UpdateMessagesRequest(MessageSentEventArgs eventArgs)
    {
        Body.UpdateMessageList(eventArgs);
    }


}