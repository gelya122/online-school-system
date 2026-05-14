using System.Text;
using System.Text.Json;

namespace online_school_admin.Infrastructure;

/// <summary>
/// Читает claim exp из JWT без дополнительных пакетов (payload — JSON в Base64Url).
/// </summary>
public static class JwtExpiryReader
{
    public static DateTime? TryGetExpiryUtc(string? jwt)
    {
        if (string.IsNullOrWhiteSpace(jwt))
            return null;

        var parts = jwt.Split('.');
        if (parts.Length < 2)
            return null;

        try
        {
            var json = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("exp", out var expEl))
                return null;

            long exp;
            if (expEl.ValueKind == JsonValueKind.Number)
                exp = expEl.GetInt64();
            else if (!long.TryParse(expEl.GetString(), out exp))
                return null;

            return DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
        }
        catch
        {
            return null;
        }
    }

    private static byte[] Base64UrlDecode(string input)
    {
        var s = input.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4)
        {
            case 2: s += "=="; break;
            case 3: s += "="; break;
        }
        return Convert.FromBase64String(s);
    }
}
