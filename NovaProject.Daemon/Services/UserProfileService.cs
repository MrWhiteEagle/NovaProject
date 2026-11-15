using Microsoft.EntityFrameworkCore;
using NovaProject.Core.Infrastructure.Data.Shared;
using NovaProject.Core.Infrastructure.Local;
using NovaProject.Daemon.Database;
using NovaProject.Daemon.Services.Database;
using NovaProject.Daemon.Services.Security;

namespace NovaProject.Daemon.Services;

public class UserProfileService
{
    private readonly MasterRegistryDbContext _masterRegistry;
    private readonly UserVaultManager _userVaultManager;
    private readonly SemaphoreSlim _registryLock = new(1, 1);

    public UserProfileService(MasterRegistryDbContext masterRegistry, UserVaultManager userVaultManager)
    {
        _masterRegistry = masterRegistry;
        _userVaultManager = userVaultManager;
    }

    public async Task InitializeAsync()
    {
        await _registryLock.WaitAsync();
        try
        {
            await _masterRegistry.Database.EnsureCreatedAsync();
            await _masterRegistry.Database.ExecuteSqlRawAsync("PRAGMA journal_mode = WAL; PRAGMA synchronous = NORMAL;");
        }
        finally
        {
            _registryLock.Release();
        }
    }

    public async Task<ProfileResult> RegisterUserAsync(string username, string password)
    {
        await _registryLock.WaitAsync();
        try
        {
            bool exists = await _masterRegistry.Profiles
                .AnyAsync(x => x.Username.ToLower() == username.ToLower());
            if(exists)
                return new ProfileResult(ProfileAccessStatus.AlreadyExists, null);

            byte[] authSalt = HashingService.GenerateNewSalt();
            byte[] vaultSalt =  HashingService.GenerateNewSalt();

            var profile = new UserProfileMeta()
            {
                ProfileId = Guid.NewGuid().ToString("N"),
                Username = username,
                AuthSalt = Convert.ToBase64String(authSalt),
                VaultSalt = Convert.ToBase64String(vaultSalt),
                PasswordVerificationHash = HashingService.GenerateAuthHash(password, authSalt),
                Status = VaultStatus.Healthy
            };
            
            _masterRegistry.Profiles.Add(profile);
            await _masterRegistry.SaveChangesAsync();

            byte[] dbKey = HashingService.GetDatabaseKey(password, vaultSalt);
            await _userVaultManager.CreateVaultAsync(profile.ProfileId, dbKey);

            return new ProfileResult(ProfileAccessStatus.Success, profile.ProfileId);
        }
        finally
        {
            _registryLock.Release();    
        }
    }
    
    public async Task<ProfileResult> UnlockProfileAsync(string username, string password, bool allowDegraded = false)
    {
        await _registryLock.WaitAsync();
        UserProfileMeta? profile;
        try
        {
            profile =
                await _masterRegistry.Profiles.FirstOrDefaultAsync(x => x.Username.ToLower() == username.ToLower());
        }
        finally
        {
            _registryLock.Release();
        }
        
        if(profile == null)
            return new ProfileResult(ProfileAccessStatus.NotFound, null);
        
        byte[] authSalt = Convert.FromBase64String(profile.AuthSalt);
        if(!HashingService.AuthenticatePassword(password, authSalt, profile.PasswordVerificationHash))
            return new ProfileResult(ProfileAccessStatus.InvalidPassword, profile.ProfileId);
        
        byte[] vaultSalt = Convert.FromBase64String(profile.VaultSalt);
        byte[] dbKey = HashingService.GetDatabaseKey(password, vaultSalt);
        
        bool checkPassed = await _userVaultManager.UnlockVaultAsync(profile.ProfileId, dbKey, allowDegraded);

        if (!checkPassed)
        {
            await MarkProfileTamperedAsync(profile.ProfileId);
            if(!allowDegraded)
                return new ProfileResult(ProfileAccessStatus.IntegrityCheckFail, profile.ProfileId);
        }

        var luid = new LocalUid(profile.Username, profile.Username, "0000", "local");
        
        return new ProfileResult(ProfileAccessStatus.Success, profile.ProfileId, luid);
    }

    public async Task WipeAndResetProfileAsync(string username, string password)
    {
        await _registryLock.WaitAsync();
        try
        {
            var profile =
                await _masterRegistry.Profiles.FirstOrDefaultAsync(x => x.Username.ToLower() == username.ToLower());
            if (profile == null) throw new InvalidOperationException("User not found");

            await _userVaultManager.PurgeVaultFileAsync(profile.ProfileId);

            byte[] newVaultSalt = HashingService.GenerateNewSalt();
            profile.VaultSalt = Convert.ToBase64String(newVaultSalt);
            profile.Status = VaultStatus.Healthy;
            profile.LastIntegrityCheckFailure = null;

            await _masterRegistry.SaveChangesAsync();

            byte[] dbKey = HashingService.GetDatabaseKey(password, newVaultSalt);
            await _userVaultManager.CreateVaultAsync(profile.ProfileId, dbKey);
        }
        finally
        {
            _registryLock.Release();
        }
    }

    private async Task MarkProfileTamperedAsync(string profileId)
    {
        await _registryLock.WaitAsync();
        try
        {
            var profile = await _masterRegistry.Profiles.FindAsync(profileId);
            if (profile != null)
            {
                profile.Status = VaultStatus.Tampered;
                profile.LastIntegrityCheckFailure = DateTime.UtcNow;
                await _masterRegistry.SaveChangesAsync();
            }
        }
        finally
        {
            _registryLock.Release();
        }
    }
}

public enum ProfileAccessStatus
{
    Success,
    NotFound,
    InvalidPassword,
    AlreadyExists,
    IntegrityCheckFail
}

public record ProfileResult(ProfileAccessStatus Status, string? Pid, LocalUid? LUid = null);