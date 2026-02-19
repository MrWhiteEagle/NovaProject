using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NovaProject.Models;

namespace NovaProject.ViewModels;

public partial class UserListViewModel(string tabIconName) : ViewModelBase
{
    public string TabIconName { get; set; } = tabIconName;
    public string DebugId { get; set; } = Guid.NewGuid().ToString();
    
    [RelayCommand]
    private void OpenConversationRequest(User user)
    {
        Console.WriteLine("Conversation Request for User: "+ user.DisplayName);
    }

    public ObservableCollection<UserListDisplayItem> DisplayItemList { get; } = [];
    
    public void AddDataToList(UserListDisplayItem item)
    {
        Dispatcher.UIThread.Post(() =>
        {
            DisplayItemList.Add(item);
            Console.WriteLine("Added Item to list with id: "+ DebugId + item.DisplayName);
        });
    }

    public void RemoveDataFromList(UserListDisplayItem item)
    {
        Dispatcher.UIThread.Post(() =>
        {
            DisplayItemList.Remove(item);
        });
    }
}