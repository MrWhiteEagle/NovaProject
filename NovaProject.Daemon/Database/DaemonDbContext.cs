using Microsoft.EntityFrameworkCore;

namespace NovaProject.Daemon.Database;

public class DaemonDbContext : DbContext
{
    public DbSet<StoredMessage> Messages => Set<StoredMessage>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string dbpath = Path.Combine(baseDir, "nova.db");
        optionsBuilder.UseSqlite($"Data Source={dbpath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StoredMessage>()
            .HasIndex(m => m.To);
        modelBuilder.Entity<StoredMessage>()
            .HasIndex(m => m.From);
        modelBuilder.Entity<StoredMessage>()
            .HasIndex(m => m.TimeStamp);
    }
}