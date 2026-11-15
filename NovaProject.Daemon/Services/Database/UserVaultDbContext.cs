using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NovaProject.Core.Infrastructure.Local;

namespace NovaProject.Daemon.Services.Database;

public class UserVaultDbContext : DbContext
{
    private readonly string _connectionString;
    
    public DbSet<MessageEntity> Messages => Set<MessageEntity>();
    public DbSet<ServerEntity> Servers => Set<ServerEntity>();
    
    public UserVaultDbContext(string vaultDbPath, string hexEncryptionKey)
    {
        var builder = new SqliteConnectionStringBuilder()
        {
            DataSource = vaultDbPath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Password = hexEncryptionKey,
            Pooling = false
        };
        
        _connectionString = builder.ConnectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(_connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<MessageEntity>()
            .HasIndex(m => new {m.ConversationId, m.TimeStamp});
    }
}