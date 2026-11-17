using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Logging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using NovaProject.Client.Services;
using NovaProject.Client.ViewModels;
using NovaProject.Core.Infrastructure;
using NovaProject.Core.Infrastructure.Data.Shared;
using NovaProject.Core.Services;

namespace NovaProject.Client.CustomControls.ViewModels;

public partial class UserListViewModel : ViewModelBase
{
    private readonly ChatService _chatService;
    private UserListDataType _dataType;
    public string TabIconName { get; set; }
    public ObservableCollection<UserListDisplayItem> DisplayItemList { get; } = [];

    public event Action<LocalUid>? ConversationSwitched;
    

    public UserListViewModel(ChatService chatService, string tabIconName, UserListDataType dataType)
    {
        _chatService = chatService;
        _dataType = dataType;
        TabIconName = tabIconName;
        Initialize();
    }

    private void Initialize()
    {
        switch (_dataType)
        {
            case UserListDataType.ServerData:
                _chatService.ServerListUpdated += OnServerListUpdated;
                _chatService.RefreshServerList();
                break;
            case UserListDataType.UserData:
                _chatService.UserListUpdated += OnUserListUpdated;
                _chatService.RefreshLocalUsers();
                break;
        }
    }
    
    private void OnServerListUpdated(object? sender, List<ServerData>? data)
    {
        if(data == null) return;
        DisplayItemList.Clear();
        foreach (var item in data)
        {
            DisplayItemList.Add(item);
        }
    }
    
    private void OnUserListUpdated(object? sender, List<LocalUid>? data)
    {
        if (data == null) return;
        DisplayItemList.Clear();
        foreach (var item in data)
        {
            DisplayItemList.Add(item.ToDisplayItem());
        }
    }

    [RelayCommand]
    public void LoadAndSwitchConversation(LocalUid luid)
    {
        NovaLogger.LogInfo("Switch conversation to " + luid);
        ConversationSwitched?.Invoke(luid);
    }

    public void Dispose()
    {
        _chatService.ServerListUpdated -= OnServerListUpdated;
        _chatService.UserListUpdated -= OnUserListUpdated;
    }
}

public enum UserListDataType
{
    ServerData,
    UserData
}