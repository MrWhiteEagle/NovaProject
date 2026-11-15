using System.ComponentModel.DataAnnotations;

namespace NovaProject.Core.Infrastructure.Local;

public class MessageEntity
{
    [Key] public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string ConversationId { get; set; } = string.Empty;
    public bool IsOutbound { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
}

public class ServerEntity
{
    [Key] public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public class UserProfileMeta
{
    [Key] public string ProfileId { get; set; } = Guid.NewGuid().ToString();
    [Required][MaxLength(64)] public string Username { get; set; } = string.Empty;
    [Required] public string AuthSalt { get; set; } = string.Empty;
    [Required] public string VaultSalt { get; set; } = string.Empty;
    [Required] public string PasswordVerificationHash { get; set; } = string.Empty;

    public VaultStatus Status { get; set; } = VaultStatus.Healthy;
    public DateTime? LastIntegrityCheckFailure { get; set; }
}

public enum VaultStatus
{
    Healthy = 0,
    Corrupted = 1,
    Tampered = 2,
    LockedDown = 3
}