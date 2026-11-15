using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using Avalonia.Threading;
using NovaProject.Client.CustomControls.ViewModels;
using NovaProject.Core.Infrastructure;
using NovaProject.Core.Infrastructure.DaemonIO;
using NovaProject.Core.Infrastructure.Data.Shared;
using NovaProject.Core.Services;

namespace NovaProject.Client.Services;

public class ChatService
{
    private readonly DaemonBridgeService _bridge;
    private readonly Dictionary<LocalUid, ChatBodyViewModel> _loadedChats = new();
    private ChatBodyViewModel? _currentChat;
    public ObservableCollection<LocalUid> LocalUserList { get; } = new();
    public ObservableCollection<ServerData> ServerList { get; } = new();

    public event EventHandler<MessageData>? MessageReceived;
    public event EventHandler<List<MessageData>>? ConversationLoaded;
    public event EventHandler<LocalUid>? AuthenticationSucceeded;
    public event EventHandler<string>? AuthenticationFailed;
    public event EventHandler<string>? RegisterFailed;
    public event EventHandler? RegisterSucceeded;
    public event EventHandler<string>? VaultTampered;
    public event EventHandler<List<ServerData>>? ServerListUpdated;
    public event EventHandler<List<LocalUid>>? UserListUpdated;
    
    public bool IsAuthenticated { get; private set; }
    public LocalUid? CurrentProfileId { get; private set; }

    public ChatService(DaemonBridgeService bridge)
    {
        _bridge = bridge;

        _bridge.AuthenticationSuccess += OnBridgeAuthenticationSucceeded;
        _bridge.AuthenticationFailure += OnBridgeAuthenticationFailed;
        _bridge.RegistrationSuccess += OnBridgeRegisterSucceeded;
        _bridge.RegistrationFailure += OnBridgeRegisterFailed;
        _bridge.VaultTamperDetected += OnBridgeVaultTampered;
        _bridge.ConnectionStateChanged += OnBridgeConnectionStateChanged;
        _bridge.MessageReceived += OnBridgeMessageReceived;
        _bridge.ConversationReceived  += OnBridgeConversationLoaded;
        _bridge.UserListReceived += OnBridgeUserListReceived;
        _bridge.ServerListReceived += OnBridgeServerListReceived;
        
        NovaLogger.LogInfo("ChatService initialized");
    }

    #region Event Chaining
    private void OnBridgeAuthenticationSucceeded(object? sender, string rawProfile)
    {
        LocalUid currentProfile = LocalUid.Parse(rawProfile);
        Dispatcher.UIThread.Post(() =>
        {
            CurrentProfileId = currentProfile;
            IsAuthenticated = true;
            
            AuthenticationSucceeded?.Invoke(this, currentProfile);
        });
    }

    private void OnBridgeAuthenticationFailed(object? sender, string reason)
    {
        Dispatcher.UIThread.Post(() =>
        {
            IsAuthenticated = false;
            AuthenticationFailed?.Invoke(this, reason);
        });
    }

    private void OnBridgeRegisterFailed(object? sender, string reason)
    {
        Dispatcher.UIThread.Post(() =>
        {
            RegisterFailed?.Invoke(this, reason);
        });
    }

    private void OnBridgeRegisterSucceeded(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            RegisterSucceeded?.Invoke(this, EventArgs.Empty);
        });
    }

    private void OnBridgeVaultTampered(object? sender, string details)
    {
        Dispatcher.UIThread.Post(() =>
        {
            VaultTampered?.Invoke(this, details);
        });
    }

    private void OnBridgeConnectionStateChanged(object? sender, bool isConnected)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (!isConnected)
            {
                IsAuthenticated = false;
            }
        });
    }

    private void OnBridgeMessageReceived(object? sender, MessageData message)
    {
        var conversationContext = LocalUid.Parse(message.ConversationId);
        Dispatcher.UIThread.Post(() =>
        {
            if (_loadedChats.TryGetValue(conversationContext, out var chat))
            {
                chat.Messages.Add(message);
                //TODO: IMPLEMENT CHATBODY UNREAD COUNT
            }
        });
    }

    private void OnBridgeConversationLoaded(object? sender, List<MessageData> messages)
    {
        Dispatcher.UIThread.Post(() => 
        { 
            ConversationLoaded?.Invoke(this, messages);
        });
    }

    private void OnBridgeUserListReceived(object? sender, List<LocalUid> userList)
    {
        Dispatcher.UIThread.Post(() =>
        {
            LocalUserList.Clear();
            foreach (var user in userList)
            {
                LocalUserList.Add(user);
            }
            UserListUpdated?.Invoke(this, userList);
        });
    }

    private void OnBridgeServerListReceived(object? sender, List<ServerData> serverList)
    {
        Dispatcher.UIThread.Post(() =>
        {
            ServerList.Clear();
            foreach (var server in serverList)
            {
                ServerList.Add(server);
            }
            ServerListUpdated?.Invoke(this, serverList);
        });
    }
    #endregion
    
    #region UI Commands
    public void Login(string username, string password) => _bridge.Login(username, password);
    public void Register(string username, string password) => _bridge.Register(username, password);
    public void RefreshLocalUsers() => _bridge.RequestUserList();
    public void RefreshServerList() => _bridge.RequestServerList();
    public ChatBodyViewModel LoadConversation(LocalUid localUid)
    {
        if (_loadedChats.TryGetValue(localUid, out var chat))
        {
            return chat;
        }
        _bridge.LoadConversation(localUid);
        var newChat = new ChatBodyViewModel(localUid, this);
        RegisterChat(newChat, localUid);
        return newChat;
    }
    public void SendMessage(LocalUid conversationContext, string text)
    {
        if (!IsAuthenticated || CurrentProfileId == null)
        {
            NovaLogger.LogError($"User {CurrentProfileId} is not authenticated");
            return;
        }

        var messageData = new MessageData
        {
            Content = text,
            ConversationId = conversationContext.ToString(),
            IsOutbound = true,
        };
        
        _bridge.SendMessage(messageData);
    }
    
    #endregion
    
    private void RegisterChat(ChatBodyViewModel chatBody, LocalUid userId)
    {
        _loadedChats.Add(userId, chatBody);
    }
}