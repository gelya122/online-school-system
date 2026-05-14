using System.Net;
using System.Net.Http;
using System.Text.Json;
using online_school_admin.Services;

namespace online_school_admin.Infrastructure;

public static class ApiErrorFormatter
{
    public static string Format(Exception ex)
    {
        return ex switch
        {
            ApiException api => Format(api),
            HttpRequestException http => NetworkMessage(http),
            TaskCanceledException tc when !tc.CancellationToken.IsCancellationRequested =>
                "Превышено время ожидания ответа сервера. Проверьте сеть и повторите попытку.",
            OperationCanceledException => "Операция отменена.",
            _ => ex.Message
        };
    }

    public static string Format(ApiException ex)
    {
        if (!string.IsNullOrWhiteSpace(ex.Message))
            return ex.Message;

        return ex.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "Требуется вход или сессия истекла.",
            HttpStatusCode.Forbidden => "Недостаточно прав для выполнения операции.",
            HttpStatusCode.NotFound => "Запрашиваемые данные не найдены.",
            HttpStatusCode.BadRequest => "Запрос отклонён сервером.",
            HttpStatusCode.InternalServerError => "Ошибка на сервере. Попробуйте позже.",
            HttpStatusCode.ServiceUnavailable => "Сервер временно недоступен.",
            _ => $"Ошибка сервера ({(int)ex.StatusCode})."
        };
    }

    /// <summary>
    /// Пытается вытащить текст из JSON ProblemDetails / произвольного объекта с title/detail/message.
    /// </summary>
    public static string? TryParseServerDetail(string? rawBody)
    {
        if (string.IsNullOrWhiteSpace(rawBody))
            return null;
        var t = rawBody.Trim();
        if (!t.StartsWith('{'))
            return null;
        try
        {
            using var doc = JsonDocument.Parse(t);
            var root = doc.RootElement;
            foreach (var prop in new[] { "detail", "title", "message", "error" })
            {
                if (root.TryGetProperty(prop, out var el) && el.ValueKind == JsonValueKind.String)
                {
                    var s = el.GetString();
                    if (!string.IsNullOrWhiteSpace(s))
                        return s;
                }
            }
        }
        catch
        {
            // ignore
        }
        return null;
    }

    private static string NetworkMessage(HttpRequestException ex)
    {
        var inner = ex.InnerException?.Message ?? "";
        if (inner.Contains("actively refused", StringComparison.OrdinalIgnoreCase))
            return "Сервер API недоступен (соединение отклонено). Запустите backend или проверьте адрес в appsettings.json.";
        if (ex.Message.Contains("Name or service not known", StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains("No such host", StringComparison.OrdinalIgnoreCase))
            return "Не удалось найти сервер по указанному адресу. Проверьте Api:BaseUrl.";
        return "Нет соединения с сервером. Проверьте интернет, VPN и доступность API.";
    }
}
