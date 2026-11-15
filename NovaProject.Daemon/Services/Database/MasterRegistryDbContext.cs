using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NovaProject.Core.Infrastructure.Local;

namespace NovaProject.Daemon.Services.Database;

public class MasterRegistryDbContext : DbContext
{
    private readonly string _connectionString;
    public DbSet<UserProfileMeta> Profiles => Set<UserProfileMeta>();

    public MasterRegistryDbContext(string? dbPath = null)
    {
        var path = dbPath ?? Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData), 
            "NovaApp", 
            "nova_profiles.db");
        
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var connectionStringBuilder = new SqliteConnectionStringBuilder()
        {
            DataSource = path,
            Mode = SqliteOpenMode.ReadWriteCreate,
        };
        
        _connectionString = connectionStringBuilder.ConnectionString;
    }

    public MasterRegistryDbContext(DbContextOptions<MasterRegistryDbContext> options) : base(options)
    {
        _connectionString = string.Empty;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite(_connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserProfileMeta>(entity =>
        {
            entity.HasKey(p => p.ProfileId);
            entity.HasIndex(p => p.Username);
            entity.Property(p => p.ProfileId).IsRequired().HasMaxLength(64);
            entity.Property(p => p.AuthSalt).IsRequired();
            entity.Property(p => p.VaultSalt).IsRequired();
            entity.Property(p => p.PasswordVerificationHash).IsRequired();
        });
    }
}