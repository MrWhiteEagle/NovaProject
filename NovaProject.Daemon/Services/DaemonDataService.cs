using Microsoft.EntityFrameworkCore;
using NovaProject.Core.Infrastructure.Local;
using NovaProject.Daemon.Database;

namespace NovaProject.Daemon.Services;

public class DaemonDataService
{
    private readonly string _dbPath;

    public DaemonDataService(string dbPath)
    {
        this._dbPath = dbPath;
        InitializaDatabase();
    }

    private void InitializaDatabase()
    {
        
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
        
        db.Messages.Add(entry);
        await db.SaveChangesAsync();
    }

    public void EditMessage(MessageData message)
    {
        
    }

    public void DeleteMessage(MessageData message)
    {
        
    }

    public async Task<List<StoredMessage>> GetReceivedMessages(LocalUid user)
    {
        using var db = new DaemonDbContext();
        return await db.Messages
            .Where(m => m.From == user.ToString())
            .OrderBy(m => m.TimeStamp)
            .ToListAsync();
    }

    public async Task<List<StoredMessage>> GetSentMessages(LocalUid user)
    {
        using var db = new DaemonDbContext();
        return await db.Messages
            .Where(m => m.To == user.ToString())
            .OrderBy(m => m.TimeStamp)
            .ToListAsync();
    }
    
}