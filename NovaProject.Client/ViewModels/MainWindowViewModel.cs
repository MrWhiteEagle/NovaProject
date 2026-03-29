using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using NovaProject.Client.Services;
using NovaProject.Core.Infrastructure;
using NovaProject.Core.Infrastructure.Structs;
using NovaProject.Core.Services;
using NovaProject.CustomControls.ViewModels;

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
        App.ServiceProvider.GetRequiredService<ChatService>().OnConversationSwitch += OpenConversationRequest;
        App.ServiceProvider.GetRequiredService<ChatService>().OnCallOutbound += OpenOutboundCallRequest;
    }

    public ChatInputViewModel Input { get; set; } = new();
    [ObservableProperty] private ChatBodyViewModel _body;
    [ObservableProperty] private ChatTitlebarViewModel _titlebar;
    
    //Server-Private Tabs
    public ObservableCollection<UserListViewModel> Tabs { get; } = new();
    [ObservableProperty] private int _currentTabIndex;
    [ObservableProperty] private UserListViewModel _currentTab;

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
        
        CurrentTabIndex = 0;
        CurrentTab = Tabs[0];
    }

    public void SetupInitialChatView()
    {
        Titlebar = new ChatTitlebarViewModel();
    }

    private void OpenConversationRequest(LocalUid localUid)
    {
        var chat = App.ServiceProvider.GetRequiredService<ChatService>().GetChatBodyForUser(localUid);
        
        //TODO: ADD DATA FETCH FOR BODY
        
        Body = chat;
        Titlebar.ChangeUserContext(chat.Recipient!);
        Input.ChangeUserContext(chat.Recipient!);
    }

    public void OpenOutboundCallRequest(LocalUid localUid)
    {
        Logger.LogInfo("[MainWindowVM] Got Outbound Call Request for user: " + localUid.UserName);
    }

    private void GetConversationData(UserListDisplayItem item)
    {
        //Implement data fetch
        Logger.LogInfo("Fetching data for user: " + item.Name);
    }
}