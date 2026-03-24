using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NovaProject.Core.Infrastructure;
using NovaProject.Core.Infrastructure.Daemon;
using NovaProject.Core.Services;
using NovaProject.CustomControls;
using NovaProject.Models.Events;

namespace NovaProject.Client.Services;

public class ChatService
{
    private readonly DaemonBridgeService _daemon;
    public event Action<List<UserListDisplayItem>>? OnUserListUpdated;
    public event Action<List<UserListDisplayItem>>? OnServerListUpdated;
    public event Action<MessageReceivedEventArgs>? OnMessageReceived;
    public event Action<MessageSentEventArgs>? OnMessageSent;

    public ChatService(DaemonBridgeService daemon)
    {
        _daemon = daemon;
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
    public void SendMessage(string message)
    {
        OnMessageReceived?.Invoke(new MessageReceivedEventArgs(message));
    }
    public void ReceiveMessage(string message)
    {
        OnMessageReceived?.Invoke(new MessageReceivedEventArgs(message));
    }
}