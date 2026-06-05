using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NovaProject.Core.Infrastructure.Local;
using NovaProject.Core.Services;
using NovaProject.Daemon.Database;

namespace NovaProject.Daemon.Services;

public class DaemonDataService
{
    private readonly string _dbPath;

    public DaemonDataService(string dbPath)
    {
        this._dbPath = dbPath;
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var db = new DaemonDbContext();
    
        //bypass Migrations for now
        Logger.LogInfo("Attempting EnsureCreated (Bypassing Migrations)...");
    
        //TODO: Implement migrations for future-proofing
        db.Database.EnsureCreated(); 
    
        Logger.LogInfo("Database Check/Creation Complete.");
        
    }

    public async Task SaveMessage(MessageData message)
    {
        using var db = new DaemonDbContext();
        
        var entry = new StoredMessage
        {
            TimeStamp = DateTime.UtcNow,
            Content = message.Content,
            SenderName = message.Sender.Name,
            SenderTag = message.Sender.Tag,
            ReceiverName = message.Recipient.Name,
            ReceiverTag = message.Recipient.Tag,
            Relay = message.Recipient.RelayAddress,
            To = message.Recipient.ToString(),
            From = message.Sender.ToString(),
        };
        try
        {
            db.Messages.Add(entry);
            Logger.LogInfo("Added Message entry for Receiver: " + message.Recipient.Name);
            await db.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Logger.LogError(e.Message);
        }
    }

    public void EditMessage(MessageData message)
    {
        //TODO: implement editing along with notifying the relay
    }

    public void DeleteMessage(MessageData message)
    {
        //TODO: implement deleting along with notifying the relay
    }

    public async Task<List<StoredMessage>> GetMessageHistory(string targetUserUid)
    {
        using var db = new DaemonDbContext();

        //Return only messages from or to the target user sorted by timestamp and limited by the app setting
        //TODO: Implement another limiting mechanism allowing a steady flow of all history without bottlenecking the machine or relay
        var messages = await db.Messages
            .Where(m =>
                m.To == targetUserUid ||
                m.From == targetUserUid)
            .OrderByDescending(m => m.TimeStamp)
            .Take(100)
            .ToListAsync();
        return messages.OrderBy(m => m.TimeStamp).ToList();
    }
    
}