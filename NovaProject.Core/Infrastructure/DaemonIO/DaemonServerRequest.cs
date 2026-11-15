namespace NovaProject.Core.Infrastructure.DaemonIO;

public class DaemonServerRequest
{
    public DaemonRequestType RequestType { get; }
    public string Payload { get; }

    public DaemonServerRequest(DaemonRequestType requestType = DaemonRequestType.Ping, string payload = "")
    {
        RequestType = requestType;
        Payload = payload;
    }
}

public enum DaemonRequestType
{
    Ping,
    LoginUser,
    RegisterUser,
    LockVault,
    LoadServerList,
    LoadLocalUserList,
    LoadUserConversation,
    MessageOutbound,
    MessageInbound
}