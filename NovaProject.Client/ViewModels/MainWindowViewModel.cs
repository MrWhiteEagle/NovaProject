using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using NovaProject.Client.Services;
using NovaProject.Core.Infrastructure;
using NovaProject.Core.Infrastructure.Daemon;
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
    
    
    [ObservableProperty] private bool _isPaneOpen;

    [RelayCommand]
    private void OpenPane()
    {
        IsPaneOpen = !IsPaneOpen;
    }

    public void SetupTabs()
    {
        Tabs.Add(new UserListViewModel("PersonRegular", UserListDataType.UserData));
        Tabs.Add(new UserListViewModel("GlobeRegular",  UserListDataType.ServerData));

        var userListRequest = new DaemonServerRequest(DaemonRequestType.LoadLocalUserList);
        var serverListRequest = new DaemonServerRequest(DaemonRequestType.LoadServerList);

        Logger.LogInfo("Fetching Chats from daemon...");
        var daemon = App.ServiceProvider.GetRequiredService<DaemonBridgeService>();
        daemon.SendRequest(userListRequest);
        daemon.SendRequest(serverListRequest);
        
        CurrentTabIndex = 0;
        CurrentTab = Tabs[0];
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