using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NovaProject.Core.Infrastructure;
using NovaProject.Core.Infrastructure.ClientServices;
using NovaProject.Core.Services;
using NovaProject.CustomControls.ViewModels;
using NovaProject.Models.Events;

namespace NovaProject.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private NotificationService _notificationService;
    private readonly string _debugId = Guid.NewGuid().ToString();
    [ObservableProperty] private User _thisUser;

    public MainWindowViewModel()
    {
        Console.WriteLine("New main window with id:" + _debugId);
        _notificationService = new NotificationService();
        AppGlobalService.SetCurrentUser(new User(
            "mrwhiteeagle",
            "MrWhiteEagle",
            "2137"));
        ThisUser = AppGlobalService.CurrentUser;
    }

    public ChatInputViewModel Input { get; set; } = new();
    [ObservableProperty] private ChatBodyViewModel _body;
    [ObservableProperty] private ChatTitlebarViewModel _titlebar;
    
    //Server-Private Tabs
    public ObservableCollection<UserListViewModel> Tabs { get; } = new();
    [ObservableProperty] private int _currentTabIndex;
    [ObservableProperty] private UserListViewModel _currentTab;

    public ObservableCollection<ChatBodyViewModel> Chats { get; } = new();
    [ObservableProperty] private User _currentOpenUser;

    partial void OnCurrentTabIndexChanged(int value)
    {
        CurrentTab = value == 0 ? Tabs[0] : Tabs[1];
    }

    private readonly List<User> _userConversations =
    [
        new("user1", "SAMPLE_USER_001", "1111"),
        new("user2", "SAMPLE_USER_002", "2222"),
        new("user3", "SAMPLE_USER_003", "3333"),
        new("user4", "SAMPLE_USER_004", "4444"),
        new("user5", "SAMPLE_USER_005", "5555"),
        new("user6", "SAMPLE_USER_006", "6666"),
        new("user7", "SAMPLE_USER_007", "7777"),
        new("user8", "SAMPLE_USER_008", "8888"),
        new("user9", "SAMPLE_USER_009", "9999"),
        new("user10", "SAMPLE_USER_010", "1010"),
    ];

    private readonly List<ServerData> _serverList =
    [
        new("server1", "Example_Server_001", "1111", "1111"),
        new("server2", "Example_Server_002", "2222", "2222"),
        new("server3", "Example_Server_003", "3333", "3333"),
        new("server4", "Example_Server_004", "4444", "4444"),
        new("server5", "Example_Server_005", "5555", "5555"),
        new("server6", "Example_Server_006", "6666", "6666"),
        new("server7", "Example_Server_007", "7777", "7777"),
        new("server8", "Example_Server_008", "8888", "8888"),
    ];
    
    [ObservableProperty] private bool _isPaneOpen;

    [RelayCommand]
    private void OpenPane()
    {
        IsPaneOpen = !IsPaneOpen;
    }

    public void SetupTabs()
    {
        Tabs.Add(new UserListViewModel("PersonRegular"));
        Tabs.Add(new UserListViewModel("GlobeRegular"));
        foreach (var item in _serverList)
        {
            Tabs[1].AddDataToList(item);
        }
        foreach (var item in _userConversations)
        {
            Tabs[0].AddDataToList(item);
        }
        
        CurrentTabIndex = 0;
        CurrentTab = Tabs[0];
        CurrentOpenUser = _userConversations[0];
    }

    public void SetupInitialChatView()
    {
        Body = new ChatBodyViewModel();
        Body.ChangeUserContext(CurrentOpenUser);
        Chats.Add(Body);
        Titlebar = new ChatTitlebarViewModel();
        Titlebar.ChangeUserContext(CurrentOpenUser);
    }

    public void UpdateMessagesRequest(MessageSentEventArgs eventArgs)
    {
        Body.UpdateMessageList(eventArgs);
    }

    public void UpdateMessagesRequest(MessageReceivedEventArgs eventArgs)
    {
        Body.UpdateMessageList(eventArgs);
        _notificationService.ShowNotification(
            eventArgs.Sender != null ? eventArgs.Sender.DisplayName : "NovaProject.Client", eventArgs.Message);
    }

    public void OpenConversationRequest(OpenConversationEventArgs eventArgs)
    {
        Logger.LogInfo("Got request to open conversation with user: " + eventArgs.SelectedUserItem.DisplayName);
        if (Chats.All(c => c.Recipient != CurrentOpenUser))
        {
            Chats.Add(Body);
        }
        
        CurrentOpenUser = eventArgs.SelectedUserItem;
        var firstOrDefault = Chats.FirstOrDefault(c => c.Recipient == eventArgs.SelectedUserItem);
        if(firstOrDefault != null)
        {
            Body = firstOrDefault;
        }
        else
        {
            Body = new ChatBodyViewModel();
            Body.ChangeUserContext(CurrentOpenUser);
            GetConversationData(eventArgs.SelectedUserItem);
        }
        Titlebar.ChangeUserContext(CurrentOpenUser);
    }

    public void OpenServerRequest(OpenServerEventArgs eventArgs)
    {
        Logger.LogInfo("Got request to open server: " + eventArgs.SelectedServerData.DisplayName);
        GetConversationData(eventArgs.SelectedServerData);
    }

    public void OpenOutboundCallRequest(CallRequestEventArgs eventArgs)
    {
        Logger.LogInfo("[MainWindowVM] Got Outbound Call Request for user: " + eventArgs.CallRecipient.DisplayName);
    }

    private void GetConversationData(UserListDisplayItem item)
    {
        //Implement data fetch
        Logger.LogInfo("Fetching data for user: " + item.Name);
    }


}