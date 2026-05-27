namespace NovaProject.Daemon.Database;

public class StoredMessage
{
    public int Id { get; set; }
    public string Content { get; set; }
    public DateTime TimeStamp { get; set; }
    public string To { get; set; }
    public string From { get; set; }
    
    //Sender Info
    public string SenderName{ get; set; }
    public string SenderTag { get; set; }
    
    public string Relay { get; set; }
    
    public string ReceiverName { get; set; }
    public string ReceiverTag { get; set; }
}