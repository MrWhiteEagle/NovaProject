using System.Security;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using NovaProject.Core.Infrastructure.Local;
using NovaProject.Daemon.Services.Database;
using NovaProject.Daemon.Services.Security;

namespace NovaProject.Daemon.Database;

public class UserVaultManager : IAsyncDisposable
{
    private readonly string _vaultsDirectory;
    private readonly SemaphoreSlim _vaultLock = new(1, 1);

    private string? _activeProfileId;
    private string? _activeHexKey;
    
    bool IsUnlocked => _activeProfileId != null && _activeHexKey != null;
    public string? CurrentProfileId => _activeProfileId;
    public bool IntegrityViolated { get; private set; }

    public UserVaultManager(string appDataDirectory)
    {
        _vaultsDirectory = Path.Combine(appDataDirectory, "vaults");
        Directory.CreateDirectory(_vaultsDirectory);
    }

    public async Task CreateVaultAsync(string profileId, byte[] rawKey)
    {
        string hexKey = Convert.ToHexString(rawKey);
        string vaultPath = GetVaultPath(profileId);

        try
        {
            await using var vault = new UserVaultDbContext(vaultPath, hexKey);
            await vault.Database.EnsureCreatedAsync();
            await vault.Database.ExecuteSqlRawAsync("PRAGMA journal_mode = WAL; PRAGMA synchronous = NORMAL;");
        }
        finally
        {
            CryptographicOperations.ZeroMemory(rawKey);
        }
        
    }

    public async Task<bool> UnlockVaultAsync(string profileId, byte[] rawKey, bool allowFailedIntegrityOpen = false)
    {
        await _vaultLock.WaitAsync();
        try
        {
            LockInternal();

            string hexKey = Convert.ToHexString(rawKey);
            string vaultPath = GetVaultPath(profileId);

            if (!File.Exists(vaultPath))
                throw new FileNotFoundException($"Vault not found for profile {profileId}");

            bool checkPassed = true;

            await using (var testVault = new UserVaultDbContext(vaultPath, hexKey))
            {
                await testVault.Database.OpenConnectionAsync();
                var connection = testVault.Database.GetDbConnection();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = $"PRAGMA cipher_integrity_check;";

                var cipherResult = await cmd.ExecuteScalarAsync();
                
                if(cipherResult != null && !string.Equals(cipherResult.ToString(), "ok", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(cipherResult.ToString()))
                    checkPassed = false;

                if (checkPassed)
                {
                    cmd.CommandText = "PRAGMA quick_check;";
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var status = reader.GetString(0);
                        if (!string.Equals(status, "ok", StringComparison.OrdinalIgnoreCase))
                        {
                            checkPassed = false;
                            break;
                        }
                    }
                }
                
                
                if (checkPassed)
                {
                    cmd.CommandText = "PRAGMA journal_mode = WAL; PRAGMA synchronous = NORMAL;";
                    await cmd.ExecuteNonQueryAsync();
                }
                
                await testVault.Database.CloseConnectionAsync();
            }

            if (!checkPassed && !allowFailedIntegrityOpen)
            {
                IntegrityViolated = true;
                return false;
            }

            _activeProfileId = profileId;
            _activeHexKey = hexKey;
            IntegrityViolated = !checkPassed;

            return checkPassed;
        }
        finally
        {
            CryptographicOperations.ZeroMemory(rawKey);
            _vaultLock.Release();
        }
    }

    public async Task PurgeVaultFileAsync(string profileId)
    {
        await _vaultLock.WaitAsync();
        try
        {
            if (_activeProfileId == profileId)
            {
                LockInternal();
            }

            string vaultPath = GetVaultPath(profileId);
            string walPath = $"{vaultPath}-wal";
            string shmPath = $"{vaultPath}-shm";

            if (File.Exists(vaultPath)) File.Delete(vaultPath);
            if (File.Exists(walPath)) File.Delete(walPath);
            if (File.Exists(shmPath)) File.Delete(shmPath);
        }
        finally
        {
            _vaultLock.Release();
        }
    }

    public async Task<T> ExecuteAsync<T>(Func<UserVaultDbContext, Task<T>> operation)
    {
        await _vaultLock.WaitAsync();
        try
        {
            if (!IsUnlocked)
                throw new InvalidOperationException("No vault is currently unlocked.");
            string vaultPath = GetVaultPath(_activeProfileId!);

            await using var context = new UserVaultDbContext(vaultPath, _activeHexKey!);
            return await operation(context);
        }
        finally
        {
            _vaultLock.Release();
        }
    }

    public async Task ExecuteAsync(Func<UserVaultDbContext, Task> operation)
    {
        await ExecuteAsync(async context =>
        {
            await operation(context);
            return true;
        });
    }

    private void LockInternal()
    {
        _activeProfileId = null;
        _activeHexKey = null;
        IntegrityViolated = false;
    }
    private async Task LockVaultAsync()
    {
        await _vaultLock.WaitAsync();
        try
        {
            LockInternal();
        }
        finally
        {
            _vaultLock.Release();
        }
    }
    
    private string GetVaultPath(string profileId) => Path.Combine(_vaultsDirectory, $"{profileId}.db");

    public async ValueTask DisposeAsync()
    {
        await LockVaultAsync();
        _vaultLock.Dispose();
        GC.SuppressFinalize(this);
    }
}