using Microsoft.AspNetCore.Hosting;

namespace OnlineSchoolAPI.Services;

/// <summary>Сохранение файлов в wwwroot для публичной раздачи статики.</summary>
public static class PublicUploadStorage
{
    public const int MaxCourseCoverBytes = 2 * 1024 * 1024;
    public const int MaxLessonVideoBytes = 200 * 1024 * 1024;
    public const int MaxLessonMaterialBytes = 20 * 1024 * 1024;

    private static readonly string[] ImageExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];
    private static readonly string[] VideoExtensions = [".mp4", ".webm", ".mov", ".mkv"];

    public static async Task<string> SaveCourseCoverAsync(
        IWebHostEnvironment env,
        byte[] bytes,
        int courseId,
        CancellationToken cancellationToken = default)
    {
        ValidateSize(bytes, MaxCourseCoverBytes, "обложки курса");
        var ext = DetectImageExtension(bytes) ?? throw new ArgumentException("Обложка: допустимы JPEG, PNG, GIF, WebP.");
        return await SaveToSubfolderAsync(env, "course-covers", $"c{courseId}_{Guid.NewGuid():N}{ext}", bytes, cancellationToken);
    }

    public static async Task<string> SaveLessonVideoAsync(
        IWebHostEnvironment env,
        byte[] bytes,
        string originalFileName,
        int lessonId,
        CancellationToken cancellationToken = default)
    {
        ValidateSize(bytes, MaxLessonVideoBytes, "видео урока");
        var ext = Path.GetExtension(originalFileName);
        if (string.IsNullOrEmpty(ext) || !VideoExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
            ext = ".mp4";
        return await SaveToSubfolderAsync(env, "lesson-videos", $"l{lessonId}_{Guid.NewGuid():N}{ext}", bytes, cancellationToken);
    }

    public static async Task<(string Url, string FileName, string? ContentType, int SizeKb)> SaveLessonMaterialAsync(
        IWebHostEnvironment env,
        byte[] bytes,
        string originalFileName,
        int lessonId,
        CancellationToken cancellationToken = default)
    {
        ValidateSize(bytes, MaxLessonMaterialBytes, "материала урока");
        var ext = Path.GetExtension(originalFileName);
        if (string.IsNullOrEmpty(ext))
            ext = ".bin";
        var safeName = $"{Path.GetFileNameWithoutExtension(originalFileName)}".Trim();
        if (string.IsNullOrEmpty(safeName))
            safeName = "file";
        var stored = $"l{lessonId}_{Guid.NewGuid():N}{ext}";
        var url = await SaveToSubfolderAsync(env, "lesson-materials", stored, bytes, cancellationToken);
        var sizeKb = Math.Max(1, (int)Math.Ceiling(bytes.Length / 1024.0));
        return (url, $"{safeName}{ext}", GuessContentType(ext), sizeKb);
    }

    private static void ValidateSize(byte[] bytes, int max, string label)
    {
        if (bytes.Length == 0 || bytes.Length > max)
            throw new ArgumentException($"Размер файла ({label}) от 1 байта до {max / (1024 * 1024)} МБ.");
    }

    private static async Task<string> SaveToSubfolderAsync(
        IWebHostEnvironment env,
        string subfolder,
        string fileName,
        byte[] bytes,
        CancellationToken cancellationToken)
    {
        var webRoot = env.WebRootPath;
        if (string.IsNullOrEmpty(webRoot))
            throw new InvalidOperationException("WebRootPath не задан.");

        var dir = Path.Combine(webRoot, subfolder);
        Directory.CreateDirectory(dir);
        var physical = Path.Combine(dir, fileName);
        await File.WriteAllBytesAsync(physical, bytes, cancellationToken);
        return $"/{subfolder.Replace('\\', '/')}/{fileName}";
    }

    private static string? DetectImageExtension(ReadOnlySpan<byte> data)
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

    private static string? GuessContentType(string ext) => ext.ToLowerInvariant() switch
    {
        ".pdf" => "application/pdf",
        ".ppt" or ".pptx" => "application/vnd.ms-powerpoint",
        ".doc" or ".docx" => "application/msword",
        ".zip" => "application/zip",
        _ => "application/octet-stream"
    };
}
