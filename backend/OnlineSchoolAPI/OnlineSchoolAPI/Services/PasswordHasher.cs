using System.Security.Cryptography;
using System.Text;

namespace OnlineSchoolAPI.Services;

public static class PasswordHasher
{
    public static string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);

    /// <summary>Проверка пароля: BCrypt (новые пользователи) или SHA-256 hex (старые записи в БД).</summary>
    public static bool Verify(string password, string? storedHash)
    {
        if (string.IsNullOrEmpty(storedHash))
            return false;

        if (storedHash.StartsWith("$2", StringComparison.Ordinal))
            return BCrypt.Net.BCrypt.Verify(password, storedHash);

        if (storedHash.Length == 64 && storedHash.All(Uri.IsHexDigit))
        {
            var computed = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            var computedHex = Convert.ToHexString(computed);
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(storedHash.ToUpperInvariant()),
                Encoding.UTF8.GetBytes(computedHex.ToUpperInvariant()));
        }

        return false;
    }
}
