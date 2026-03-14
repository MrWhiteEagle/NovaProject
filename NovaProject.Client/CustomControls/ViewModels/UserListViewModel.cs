using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NovaProject.Core.Infrastructure;
using NovaProject.Models;
using NovaProject.Models.Events;
using NovaProject.ViewModels;

namespace NovaProject.CustomControls.ViewModels;

public partial class UserListViewModel(string tabIconName) : ViewModelBase
{
    public string TabIconName { get; set; } = tabIconName;
    public string DebugId { get; set; } = Guid.NewGuid().ToString();

    public ObservableCollection<UserListDisplayItem> DisplayItemList { get; } = [];
    
    public void AddDataToList(UserListDisplayItem item)
    {
        Dispatcher.UIThread.Post(() =>
        {
            DisplayItemList.Add(item);
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