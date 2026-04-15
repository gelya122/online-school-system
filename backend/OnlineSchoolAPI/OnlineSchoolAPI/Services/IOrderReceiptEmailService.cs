namespace OnlineSchoolAPI.Services;

public interface IOrderReceiptEmailService
{
    Task SendCheckoutReceiptAsync(
        string recipientEmail,
        string orderNumber,
        IReadOnlyList<(string CourseTitle, decimal UnitPrice, int Qty)> lines,
        decimal subtotal,
        decimal discount,
        decimal finalAmount,
        string? promoMessage,
        bool isInstallment,
        int? installmentCount,
        CancellationToken cancellationToken = default);
}
