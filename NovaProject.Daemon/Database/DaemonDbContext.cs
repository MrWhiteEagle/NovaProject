using Microsoft.EntityFrameworkCore;

namespace NovaProject.Daemon.Database;

public class DaemonDbContext : DbContext
{
    public DbSet<StoredMessage> Messages => Set<StoredMessage>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=nova.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StoredMessage>()
            .HasIndex(m => new { m.Relay, m.SenderName, m.SenderTag });
    }
}