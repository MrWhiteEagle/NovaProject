using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using NovaProject.Client.Services;
using NovaProject.Client.ViewModels;
using NovaProject.Core.Infrastructure;

namespace NovaProject.Client.CustomControls.ViewModels;

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
                App.ServiceProvider.GetRequiredService<ChatService>().LoadSavedServersRequest();
                break;
            case UserListDataType.UserData:
                App.ServiceProvider.GetRequiredService<ChatService>().OnUserListUpdated += UpdateData;
                App.ServiceProvider.GetRequiredService<ChatService>().LoadUserListRequest();
                break;
        }
    }
    public ObservableCollection<UserListDisplayItem> DisplayItemList { get; } = [];
    //TODO: Refactor to use Local structs instead classes, user class should be reserved for daemon and server only
    
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