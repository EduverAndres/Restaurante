using System.Security.Cryptography;
using Restaurante.Application.Interfaces;

namespace Restaurante.Infrastructure.Services;

public class PasswordService : IPasswordService
{
    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string hash)
    {
        var parts = hash.Split('.');
        if (parts.Length != 2) return false;
        var storedHash = Convert.FromBase64String(parts[1]);
        var computed = Rfc2898DeriveBytes.Pbkdf2(password, Convert.FromBase64String(parts[0]), 100_000, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(storedHash, computed);
    }
}
