using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using NovaProject.Client.CustomControls.ViewModels;
using NovaProject.Client.Services;
using NovaProject.Core.Infrastructure;
using NovaProject.Core.Infrastructure.Data.Shared;
using NovaProject.Core.Infrastructure.Local;
using NovaProject.Core.Services;

namespace NovaProject.Client.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly NotificationService _notificationService;
    private readonly ChatService _chatService;
    
    [ObservableProperty] private LocalUid _thisUser;
    [ObservableProperty] private bool _isLoggedIn;

    [ObservableProperty] private LoginWindowViewModel _loginWindow;
    public ChatInputViewModel Input { get; set; }
    [ObservableProperty] private ChatBodyViewModel? _body;
    [ObservableProperty] private ChatTitlebarViewModel _titlebar = new();

    
    //Server-Private Conversation Tabs
    public ObservableCollection<UserListViewModel> Tabs { get; } = new();
    [ObservableProperty] private int _currentTabIndex;
    [ObservableProperty] private UserListViewModel _currentTab;
    
    [ObservableProperty] private bool _isPaneOpen;
    [ObservableProperty] private bool _isContextInitialized;
    [ObservableProperty] private bool _wasUserListLoaded;
    [ObservableProperty] private bool _wasServerListLoaded;

    public MainWindowViewModel(NotificationService notificationService, ChatService chatService)
    {
        _notificationService = notificationService;
        _chatService = chatService;
        Input = new ChatInputViewModel(chatService);
        LoginWindow = new LoginWindowViewModel(chatService);

        if (_chatService.CurrentProfileId != null)
        {
            ThisUser = _chatService.CurrentProfileId;
        }

        _chatService.UserListUpdated += OnUserListUpdated;
        _chatService.ServerListUpdated += OnServerListUpdated;
        _chatService.AuthenticationSucceeded += OnAuthenticationSucceeded;
        
        SetupTabs();
    }

    private void OnUserListUpdated(object? sender, List<LocalUid> list)
    {
        WasUserListLoaded = true;
    }
    private void OnServerListUpdated(object? sender, List<ServerData> list)
    {
        WasServerListLoaded = true;
    }
    
    partial void OnCurrentTabIndexChanged(int value)
    {
        if (value >= 0 && value < Tabs.Count)
        {
            CurrentTab = Tabs[value];
        }
    }

    private void OnAuthenticationSucceeded(object? sender, LocalUid user)
    {
        Console.WriteLine("Authentication Succeeded, fetching data....");
        IsLoggedIn = true;
        ThisUser = _chatService.CurrentProfileId!;
        SetupTabs();
    }
    
    [RelayCommand]
    private void TogglePane()
    {
        IsPaneOpen = !IsPaneOpen;
    }

    public void SetupTabs()
    {
        Tabs.Clear();
        Tabs.Add(new UserListViewModel(_chatService, "PersonRegular", UserListDataType.UserData));
        Tabs.Add(new UserListViewModel(_chatService, "GlobeRegular",  UserListDataType.ServerData));
        Tabs[0].ConversationSwitched += OpenConversationRequest;
        Tabs[1].ConversationSwitched += OpenConversationRequest;
        CurrentTabIndex = 0;
        CurrentTab = Tabs[0];
    }

    private void OpenConversationRequest(LocalUid localUid)
    {
        ChatBodyViewModel chat = _chatService.LoadConversation(localUid);
        Body = chat;
        Titlebar.ChangeConversationContext(localUid);
        Input.ChangeConversationContext(localUid);
        IsContextInitialized = true;
    }

    public void Dispose()
    {
        _chatService.UserListUpdated -= OnUserListUpdated;
        _chatService.ServerListUpdated -= OnServerListUpdated;
        _chatService.AuthenticationSucceeded -= OnAuthenticationSucceeded;

        foreach (var tab in Tabs)
        {
            tab.Dispose();
        }
    }
}