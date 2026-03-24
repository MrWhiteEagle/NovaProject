using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using NovaProject.Client.Services;
using NovaProject.Core.Infrastructure;
using NovaProject.Core.Services;
using NovaProject.Models;
using NovaProject.Models.Events;
using NovaProject.ViewModels;

namespace NovaProject.CustomControls.ViewModels;

public partial class UserListViewModel : ViewModelBase
{
    public string TabIconName { get; set; }

    public UserListViewModel(string tabIconName, UserListDataType dataType)
    {
        this.TabIconName = tabIconName;
        switch (dataType)
        {
            case UserListDataType.ServerData:
                App.ServiceProvider.GetRequiredService<ChatService>().OnServerListUpdated += UpdateData;
                break;
            case UserListDataType.UserData:
                App.ServiceProvider.GetRequiredService<ChatService>().OnUserListUpdated += UpdateData;
                break;
        }
    }
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

    private void UpdateData(List<UserListDisplayItem> data)
    {
        Dispatcher.UIThread.Post(() =>
        {
            DisplayItemList.Clear();
            foreach (var item in data)
            {
                DisplayItemList.Add(item);
            }
        });
    }
}

public enum UserListDataType
{
    ServerData,
    UserData
}