namespace NovaProject.Models;

public class MessageOutbound(string content, User sender, User recipient) : MessageIo(content, sender, recipient)
{
    public void EditMessage(string newContent)
    {
        this.Content = newContent;
        this.Edited = true;
        //Emit signal on edit
    }
}