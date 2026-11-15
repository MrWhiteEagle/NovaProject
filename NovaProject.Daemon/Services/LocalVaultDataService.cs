using Microsoft.EntityFrameworkCore;
using NovaProject.Core.Infrastructure.Local;
using NovaProject.Daemon.Database;

namespace NovaProject.Daemon.Services;

public class LocalVaultDataService(UserVaultManager userVaultManager)
{
    private readonly UserVaultManager _userVaultManager = userVaultManager;

    public Task SaveMessageAsync(MessageEntity message)
    {
        return _userVaultManager.ExecuteAsync(async db =>
        {
            db.Messages.Add(message);
            await db.SaveChangesAsync();
        });
    }

    public Task<List<MessageEntity>> GetMessagesAsync(string conversationId, int limit = 50)
    {
        return _userVaultManager.ExecuteAsync(async db =>
        {
            return await db.Messages
                .AsNoTracking()
                .Where(m => m.ConversationId == conversationId)
                .OrderByDescending(m => m.TimeStamp)
                .Take(limit)
                .OrderBy(m => m.TimeStamp)
                .ToListAsync();
        });
    }

    public Task<List<ServerEntity>> GetServersAsync()
    {
        return _userVaultManager.ExecuteAsync(async db =>
        {
            return await db.Servers.AsNoTracking().ToListAsync();
        });
    }

    public Task AddServerAsync(ServerEntity server)
    {
        return _userVaultManager.ExecuteAsync(async db =>
        {
            db.Servers.Add(server);
            await db.SaveChangesAsync();
        });
    }

    public Task RemoveServerAsync(string serverId)
    {
        return _userVaultManager.ExecuteAsync(async db =>
        {
            var server = await db.Servers.FindAsync(serverId);
            if (server != null)
            {
                db.Servers.Remove(server);
                await db.SaveChangesAsync();
            }
        });
    }
}