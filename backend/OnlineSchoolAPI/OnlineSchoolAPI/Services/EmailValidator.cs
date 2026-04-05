using System.Net.Mail;

namespace OnlineSchoolAPI.Services;

public static class EmailValidator
{
    /// <summary>Проверка email: структура + успешный разбор MailAddress (без сравнения с исходной строкой — MailAddress может нормализовать адрес).</summary>
    public static bool IsValid(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var trimmed = email.Trim();
        if (trimmed.Length > 254)
            return false;

        var at = trimmed.LastIndexOf('@');
        if (at <= 0 || at >= trimmed.Length - 1)
            return false;

        var local = trimmed[..at];
        var domain = trimmed[(at + 1)..];
        if (string.IsNullOrWhiteSpace(local) || string.IsNullOrWhiteSpace(domain))
            return false;
        if (local.Length > 64 || domain.Length > 253)
            return false;
        if (!domain.Contains('.'))
            return false;

        try
        {
            _ = new MailAddress(trimmed);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
