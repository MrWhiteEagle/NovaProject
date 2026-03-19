namespace NovaProject.Core.Infrastructure.Daemon;

public class DaemonServerResponse
{
    public DaemonResponseType ResponseType { get; set; }
    public string Payload { get; set; }

    public DaemonServerResponse(DaemonResponseType responseType, string payload)
    {
        ResponseType = responseType;
        Payload = payload;
    }
}

public enum DaemonResponseType
{
    PingSuccess
}