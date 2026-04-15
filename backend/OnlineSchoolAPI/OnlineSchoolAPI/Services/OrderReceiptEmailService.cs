using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace OnlineSchoolAPI.Services;

public class OrderReceiptEmailService : IOrderReceiptEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<OrderReceiptEmailService> _logger;

    public OrderReceiptEmailService(IConfiguration config, ILogger<OrderReceiptEmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendCheckoutReceiptAsync(
        string recipientEmail,
        string orderNumber,
        IReadOnlyList<(string CourseTitle, decimal UnitPrice, int Qty)> lines,
        decimal subtotal,
        decimal discount,
        decimal finalAmount,
        string? promoMessage,
        bool isInstallment,
        int? installmentCount,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(recipientEmail) || !EmailValidator.IsValid(recipientEmail))
        {
            _logger.LogWarning("Не удалось отправить чек: некорректный email получателя для заказа {OrderNumber}", orderNumber);
            return;
        }

        var section = _config.GetSection("Smtp");
        var host = section["Host"];
        if (string.IsNullOrWhiteSpace(host))
        {
            _logger.LogInformation(
                "SMTP не настроен (Smtp:Host пустой). Чек для заказа {OrderNumber} сформирован, но письмо не отправлено.",
                orderNumber);
            return;
        }

        var port = int.TryParse(section["Port"], out var p) ? p : 587;
        var enableSsl = bool.TryParse(section["EnableSsl"], out var ssl) && ssl;
        var user = section["User"];
        var password = section["Password"];
        var fromAddress = section["FromAddress"];
        var fromName = section["FromName"] ?? "Онлайн-школа";

        if (string.IsNullOrWhiteSpace(fromAddress) || !EmailValidator.IsValid(fromAddress))
        {
            _logger.LogWarning("Smtp:FromAddress не задан или некорректен. Чек для {OrderNumber} не отправлен.", orderNumber);
            return;
        }

        var (plain, html) = BuildBodies(
            orderNumber,
            lines,
            subtotal,
            discount,
            finalAmount,
            promoMessage,
            isInstallment,
            installmentCount);

        using var message = new MailMessage
        {
            From = new MailAddress(fromAddress.Trim(), fromName),
            Subject = $"Чек по заказу {orderNumber}",
            BodyEncoding = Encoding.UTF8,
            SubjectEncoding = Encoding.UTF8,
        };

        message.To.Add(recipientEmail.Trim());
        message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(plain, Encoding.UTF8, "text/plain"));
        message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(html, Encoding.UTF8, "text/html"));

        using var client = new SmtpClient(host.Trim(), port)
        {
            EnableSsl = enableSsl,
        };

        if (!string.IsNullOrWhiteSpace(user))
            client.Credentials = new NetworkCredential(user, password);

        try
        {
            await client.SendMailAsync(message, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Чек по заказу {OrderNumber} отправлен на {Email}", orderNumber, recipientEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка отправки чека для заказа {OrderNumber}", orderNumber);
        }
    }

    private static (string Plain, string Html) BuildBodies(
        string orderNumber,
        IReadOnlyList<(string CourseTitle, decimal UnitPrice, int Qty)> lines,
        decimal subtotal,
        decimal discount,
        decimal finalAmount,
        string? promoMessage,
        bool isInstallment,
        int? installmentCount)
    {
        var ru = CultureInfo.GetCultureInfo("ru-RU");
        var sb = new StringBuilder();
        sb.AppendLine($"Заказ {orderNumber}");
        sb.AppendLine($"Дата: {DateTime.UtcNow:dd.MM.yyyy HH:mm} (UTC)");
        sb.AppendLine();
        foreach (var (title, unit, qty) in lines)
        {
            var lineSum = unit * qty;
            sb.AppendLine($"{title} × {qty} — {lineSum.ToString("N2", ru)} ₽");
        }

        sb.AppendLine();
        sb.AppendLine($"Сумма: {subtotal.ToString("N2", ru)} ₽");
        if (discount > 0)
            sb.AppendLine($"Скидка: −{discount.ToString("N2", ru)} ₽");
        if (!string.IsNullOrWhiteSpace(promoMessage))
            sb.AppendLine(promoMessage);
        sb.AppendLine($"Итого к оплате: {finalAmount.ToString("N2", ru)} ₽");
        if (isInstallment && installmentCount is >= 2)
            sb.AppendLine($"Оплата: рассрочка, {installmentCount} платежей (график в личном кабинете).");
        else
            sb.AppendLine("Оплата: получена.");

        sb.AppendLine();
        sb.AppendLine("Спасибо за покупку!");

        var plain = sb.ToString();

        var htmlSb = new StringBuilder();
        htmlSb.Append("<!DOCTYPE html><html><head><meta charset=\"utf-8\" /></head><body style=\"font-family:sans-serif;line-height:1.5;color:#111827\">");
        htmlSb.Append($"<h2>Чек по заказу {System.Net.WebUtility.HtmlEncode(orderNumber)}</h2>");
        htmlSb.Append($"<p>{System.Net.WebUtility.HtmlEncode(DateTime.UtcNow.ToString("dd.MM.yyyy HH:mm", ru))} (UTC)</p>");
        htmlSb.Append("<table style=\"border-collapse:collapse;width:100%;max-width:480px\">");
        htmlSb.Append("<thead><tr><th align=\"left\">Курс</th><th align=\"right\">Сумма</th></tr></thead><tbody>");
        foreach (var (title, unit, qty) in lines)
        {
            var lineSum = unit * qty;
            htmlSb.Append("<tr><td>")
                .Append(System.Net.WebUtility.HtmlEncode($"{title} × {qty}"))
                .Append("</td><td align=\"right\">")
                .Append(System.Net.WebUtility.HtmlEncode($"{lineSum.ToString("N2", ru)} ₽"))
                .Append("</td></tr>");
        }

        htmlSb.Append("</tbody></table>");
        htmlSb.Append($"<p><strong>Сумма:</strong> {subtotal.ToString("N2", ru)} ₽</p>");
        if (discount > 0)
            htmlSb.Append($"<p><strong>Скидка:</strong> −{discount.ToString("N2", ru)} ₽</p>");
        if (!string.IsNullOrWhiteSpace(promoMessage))
            htmlSb.Append($"<p>{System.Net.WebUtility.HtmlEncode(promoMessage)}</p>");
        htmlSb.Append($"<p><strong>Итого к оплате:</strong> {finalAmount.ToString("N2", ru)} ₽</p>");
        if (isInstallment && installmentCount is >= 2)
            htmlSb.Append(
                $"<p>Оплата: рассрочка, {installmentCount} платежей.</p>");
        else
            htmlSb.Append("<p>Оплата: получена.</p>");
        htmlSb.Append("<p>Спасибо за покупку!</p></body></html>");

        return (plain, htmlSb.ToString());
    }
}
