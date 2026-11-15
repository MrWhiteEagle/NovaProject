using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace NovaProject.Daemon.Services.Security;

public static class HashingService
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 4;
    private const int MemorySizeKb = 65536;
    private const int DegreeOfParallelism = 1;

    public static byte[] GenerateNewSalt() => RandomNumberGenerator.GetBytes(SaltSize);
    
    public static string GenerateAuthHash(string password, byte[] userAuthSalt)
    {
        byte[] hash = GenerateArgon2(password, userAuthSalt);
        return Convert.ToBase64String(hash);
    }
    
    public static byte[] GetDatabaseKey(string password, byte[] userVaultSalt)
    {
        return GenerateArgon2(password, userVaultSalt);
    }

    public static bool AuthenticatePassword(string password, byte[] userAuthSalt, string storedHash)
    {
        try
        {
            byte[] expected = Convert.FromBase64String(storedHash);
            byte[] computed = GenerateArgon2(password, userAuthSalt);
            return CryptographicOperations.FixedTimeEquals(computed, expected);
        }
        catch
        {
            return false;
        }
    }
    
    private static byte[] GenerateArgon2(string text, byte[] salt)
    {
        byte[] passBytes = Encoding.UTF8.GetBytes(text);
        try
        {
            using var argon2 = new Argon2id(passBytes)
            {
                Salt = salt,
                DegreeOfParallelism = DegreeOfParallelism,
                Iterations = Iterations,
                MemorySize = MemorySizeKb
            };
            
            return argon2.GetBytes(HashSize);

        }
        finally
        {
            CryptographicOperations.ZeroMemory(passBytes);
        }
    }
}