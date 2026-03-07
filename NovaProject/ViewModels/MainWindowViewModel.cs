using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NovaProject.CustomControls.ViewModels;
using NovaProject.Models;
using NovaProject.Models.Events;

namespace NovaProject.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly string _debugId = Guid.NewGuid().ToString();
    public MainWindowViewModel()
    {
        Console.WriteLine("New main window with id:" + _debugId);
    }
    
    public ChatInputViewModel Input { get; set; } = new();
    public ChatBodyViewModel Body { get; set; } = new();
    
    //Server-Private Tabs
    public ObservableCollection<UserListViewModel> Tabs { get; } = new();
    [ObservableProperty] private int _currentTabIndex;
    [ObservableProperty] private UserListViewModel _currentTab;

    public ObservableCollection<ChatBodyViewModel> Chats { get; } = new();
    [ObservableProperty] private User _currentOpenUser;
    [ObservableProperty] private ChatBodyViewModel _currentChat;
    [ObservableProperty] private ChatTitlebarViewModel _currentTitlebar;

    partial void OnCurrentTabIndexChanged(int value)
    {
        CurrentTab = value == 0 ? Tabs[0] : Tabs[1];
    }

    private readonly List<User> _userConversations =
    [
        new User("user1", "SAMPLE_USER_001", "1111"),
        new User("user2", "SAMPLE_USER_002", "2222"),
        new User("user3", "SAMPLE_USER_003", "3333"),
        new User("user4", "SAMPLE_USER_004", "4444"),
        new User("user5", "SAMPLE_USER_005", "5555"),
        new User("user6", "SAMPLE_USER_006", "6666"),
        new User("user7", "SAMPLE_USER_007", "7777"),
        new User("user8", "SAMPLE_USER_008", "8888"),
        new User("user9", "SAMPLE_USER_009", "9999"),
        new User("user10", "SAMPLE_USER_010", "1010"),
    ];

    private readonly List<ServerData> _serverList =
    [
        new ServerData("server1", "Example_Server_001", "1111", "1111"),
        new ServerData("server2", "Example_Server_002", "2222", "2222"),
        new ServerData("server3", "Example_Server_003", "3333", "3333"),
        new ServerData("server4", "Example_Server_004", "4444", "4444"),
        new ServerData("server5", "Example_Server_005", "5555", "5555"),
        new ServerData("server6", "Example_Server_006", "6666", "6666"),
        new ServerData("server7", "Example_Server_007", "7777", "7777"),
        new ServerData("server8", "Example_Server_008", "8888", "8888"),
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
        CurrentChat = new ChatBodyViewModel();
    }

    public void UpdateMessagesRequest(MessageSentEventArgs eventArgs)
    {
        Body.UpdateMessageList(eventArgs);
    }

    public void UpdateMessagesRequest(MessageReceivedEventArgs eventArgs)
    {
        Body.UpdateMessageList(eventArgs);
    }

    public void OpenConversationRequest(OpenConversationEventArgs eventArgs)
    {
        Console.WriteLine("Got request to open conversation with user: " + eventArgs.SelectedUserItem.DisplayName);
        GetConversationData(eventArgs.SelectedUserItem);
    }

    public void OpenServerRequest(OpenServerEventArgs eventArgs)
    {
        Console.WriteLine("Got request to open server: " + eventArgs.SelectedServerData.DisplayName);
        GetConversationData(eventArgs.SelectedServerData);
    }

    private void GetConversationData(UserListDisplayItem item)
    {
        //Implement data fetch
        Console.WriteLine(item.Name);
    }


}