using System.IO;

namespace online_school_admin.Infrastructure;

/// <summary>
/// Простое файловое логирование ошибок (%LocalAppData%/online_school_admin/logs).
/// </summary>
public static class AppLogger
{
    private static readonly object Gate = new();

    static AppLogger()
    {
        try
        {
            Directory.CreateDirectory(LogDirectory);
        }
        catch
        {
            // ignore
        }
    }

    public static string LogDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "online_school_admin", "logs");

    private static string LogFilePath =>
        Path.Combine(LogDirectory, $"app_{DateTime.UtcNow:yyyyMMdd}.log");

    public static void Log(string message, Exception? ex = null)
    {
        var line = $"{DateTime.UtcNow:O}\t{message}";
        if (ex != null)
            line += Environment.NewLine + ex;

        lock (Gate)
        {
            try
            {
                File.AppendAllText(LogFilePath, line + Environment.NewLine);
            }
            catch
            {
                // ignore logging failures
            }
        }
    }

    public static void Log(Exception ex, string? context = null)
        => Log(context == null ? ex.Message : $"{context}: {ex.Message}", ex);
}
