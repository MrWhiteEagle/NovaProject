using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using NovaProject.Client.CustomControls.ViewModels;
using NovaProject.Core.Infrastructure;
using NovaProject.Core.Infrastructure.Local;
using NovaProject.Core.Infrastructure.Ui.Daemon;
using NovaProject.Core.Services;

namespace NovaProject.Client.Services;

public class ChatService
{
    private readonly DaemonBridgeService _daemon;
    private readonly Dictionary<LocalUid, ChatBodyViewModel> _loadedChats = new();
    public event Action<List<UserListDisplayItem>>? OnUserListUpdated;
    public event Action<List<UserListDisplayItem>>? OnServerListUpdated;
    public event Action<MessageData>? OnMessageReceived;
    public event Action<MessageData>? OnMessageSent;
    public event Action<LocalUid>? OnConversationSwitch;
    public event Action<LocalUid>? OnCallOutbound;

    public ChatService(DaemonBridgeService daemon)
    {
        _daemon = daemon;
        Logger.LogInfo("ChatService initialized");
    }

    public void LoadSavedServersRequest()
    {
        var request = new DaemonServerRequest(DaemonRequestType.LoadServerList);
        _daemon.SendRequest(request);
    }
    
    public void LoadUserListRequest()
    {
        var request = new DaemonServerRequest(DaemonRequestType.LoadLocalUserList);
        _daemon.SendRequest(request);
    }

    public void UpdateServerList(List<ServerData>? serverList)
    {
        var newServerList = serverList.Cast<UserListDisplayItem>().ToList();
        OnServerListUpdated?.Invoke(newServerList);
        Logger.LogInfo("Got ServerList from daemon");
    }
    public void UpdateUserList(List<User>? userList)
    {
        var newUserList = userList.Cast<UserListDisplayItem>().ToList();
        OnUserListUpdated?.Invoke(newUserList);
        Logger.LogInfo("Got UserList from daemon");
    }
    public void SendMessage(MessageData data)
    {
        OnMessageSent?.Invoke(data);
        _daemon.SendRequest(new DaemonServerRequest(DaemonRequestType.MessageOutbound, "", JsonSerializer.Serialize(data)));
    }
    public void ReceiveMessage(DaemonServerResponse response)
    {
        var data = JsonSerializer.Deserialize<MessageData>(response.Payload);
        OnMessageReceived?.Invoke(data);
    }

    public void SwitchConversation(LocalUid localUid)
    {
        Logger.LogInfo($"Switching conversation to {localUid.UserName}");
        OnConversationSwitch?.Invoke(localUid);
    }

    public void SwitchConversation(User user) => SwitchConversation(ComposeLocalUserId(user));

    public void SwitchServer(ServerData? server)
    {
        Logger.LogInfo($"Switching server to {server?.Name}");
    }

    public void RequestOutboundCall(LocalUid localUid)
    {
        Logger.LogInfo($"Requesting outbound call to {localUid.UserName}");
        OnCallOutbound?.Invoke(localUid);
    }

    public static LocalUid ComposeLocalUserId(User? user)
    {
        return user != null ? new LocalUid(user.Name, user.DisplayName, user.Tag, user.RelayAddress) : new LocalUid("null", "null", "null",  "null");
    }

    private static User ExtractDataFromLocalId(LocalUid localUid)
    {
        return new User(localUid.Name, localUid.UserName, localUid.Tag, localUid.RelayAddress ?? "");
    }
    
    public ChatBodyViewModel GetChatBodyForUser(LocalUid localUid)
    {
        if (_loadedChats.TryGetValue(localUid, out var chat))
        {
            return chat;
        }
        else
        {
            _daemon.GetUserConversation(ExtractDataFromLocalId(localUid));
            var newChat = new ChatBodyViewModel();
            newChat.ChangeUserContext(localUid);
            RegisterChat(newChat, newChat.Recipient!);
            return newChat;
        }
    }
    public void RegisterChat(ChatBodyViewModel chatBody, LocalUid userId)
    {
        _loadedChats.Add(userId, chatBody);
    }

    public ChatBodyViewModel GetChatBodyForUser(User user) => GetChatBodyForUser(ComposeLocalUserId(user));
}