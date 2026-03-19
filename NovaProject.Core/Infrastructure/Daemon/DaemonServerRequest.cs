namespace NovaProject.Core.Infrastructure.Daemon;

public class DaemonServerRequest
{
    public DaemonRequestType RequestType { get; }
    public string RelayServer { get; }
    public string Payload { get; }

    public DaemonServerRequest(DaemonRequestType requestType = DaemonRequestType.Ping, string relayServer = "", string payload = "")
    {
        RequestType = requestType;
        RelayServer = relayServer;
        Payload = payload;
    }
}

public enum DaemonRequestType
{
    Ping,
    LoadServerList,
    MessageUser,
    ReceiveUserMessage,
}