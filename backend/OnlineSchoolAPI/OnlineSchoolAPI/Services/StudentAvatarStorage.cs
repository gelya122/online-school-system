namespace OnlineSchoolAPI.Services;

public static class StudentAvatarStorage
{
    private const int MaxBytes = 512 * 1024;
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];

    /// <summary>Сохраняет изображение из base64 (с префиксом data URL или без) в wwwroot/avatars. Возвращает относительный URL для клиента.</summary>
    public static async Task<string?> TrySaveFromBase64Async(
        IWebHostEnvironment env,
        string? base64OrDataUrl,
        int userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(base64OrDataUrl))
            return null;

        var trimmed = base64OrDataUrl.Trim();
        if (trimmed.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            var comma = trimmed.IndexOf(',');
            if (comma < 0)
                throw new ArgumentException("Некорректный формат data URL для аватара.");
            trimmed = trimmed[(comma + 1)..];
        }

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(trimmed);
        }
        catch
        {
            throw new ArgumentException("Аватар: неверная строка base64.");
        }

        if (bytes.Length == 0 || bytes.Length > MaxBytes)
            throw new ArgumentException("Размер аватара должен быть от 1 байта до 512 КБ.");

        var ext = DetectExtension(bytes);
        if (ext == null || !AllowedExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
            throw new ArgumentException("Допустимые форматы аватара: JPEG, PNG, GIF, WebP.");

        var webRoot = env.WebRootPath;
        if (string.IsNullOrEmpty(webRoot))
            throw new InvalidOperationException("WebRootPath не задан.");

        var dir = Path.Combine(webRoot, "avatars");
        Directory.CreateDirectory(dir);

        var fileName = $"{userId}_{Guid.NewGuid():N}{ext}";
        var physical = Path.Combine(dir, fileName);
        await File.WriteAllBytesAsync(physical, bytes, cancellationToken);

        return $"/avatars/{fileName}";
    }

    private static string? DetectExtension(ReadOnlySpan<byte> data)
    {
        if (data.Length >= 3 && data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF)
            return ".jpg";
        if (data.Length >= 8 && data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47)
            return ".png";
        if (data.Length >= 6 && data[0] == 0x47 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x38)
            return ".gif";
        if (data.Length >= 12 && data[0] == 0x52 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x46
            && data[8] == 0x57 && data[9] == 0x45 && data[10] == 0x42 && data[11] == 0x50)
            return ".webp";
        return null;
    }
}
