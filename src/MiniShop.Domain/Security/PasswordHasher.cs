using System.Security.Cryptography;

namespace MiniShop.Domain.Security;

// PBKDF2 password hashing (no external packages needed).
public static class PasswordHasher
{
    private const int Iterations = 100_000;

    public static (string hash, string salt) Hash(string password)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(16);
        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, Iterations, HashAlgorithmName.SHA256, 32);
        return (Convert.ToBase64String(hashBytes), Convert.ToBase64String(saltBytes));
    }

    public static bool Verify(string password, string hash, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, Iterations, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(hashBytes, Convert.FromBase64String(hash));
    }
}
