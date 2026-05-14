using System.Net;
using online_school_admin.Models;

namespace online_school_admin.Services;

public sealed class AdminPaymentsService
{
    private readonly ApiClient _api;

    public AdminPaymentsService(ApiClient api)
    {
        _api = api;
    }

    public Task<List<AdminOrderListRowDto>> GetOrdersAsync(string? search = null, CancellationToken cancellationToken = default)
    {
        var url = "api/admin/orders";
        if (!string.IsNullOrWhiteSpace(search))
            url += "?search=" + WebUtility.UrlEncode(search.Trim());
        return _api.GetAsync<List<AdminOrderListRowDto>>(url, cancellationToken);
    }

    public Task<AdminOrderDetailsDto> GetOrderAsync(int id, CancellationToken cancellationToken = default)
        => _api.GetAsync<AdminOrderDetailsDto>($"api/admin/orders/{id}", cancellationToken);

    public Task<AdminOrderDetailsDto> CreateOrderAsync(AdminOrderCreateDto dto, CancellationToken cancellationToken = default)
        => _api.PostAsync<AdminOrderCreateDto, AdminOrderDetailsDto>("api/admin/orders", dto, cancellationToken);

    public Task UpdateOrderAsync(int id, AdminOrderUpdateDto dto, CancellationToken cancellationToken = default)
        => _api.PutAsync($"api/admin/orders/{id}", dto, cancellationToken);

    public Task PatchOrderStatusAsync(int id, AdminOrderStatusPatchDto dto, CancellationToken cancellationToken = default)
        => _api.PatchAsync($"api/admin/orders/{id}/status", dto, cancellationToken);

    public Task ApplyPromoCodeAsync(int id, AdminApplyPromoCodeDto dto, CancellationToken cancellationToken = default)
        => _api.PostAsync($"api/admin/orders/{id}/apply-promo-code", dto, cancellationToken);

    public Task MarkPaidAsync(int id, CancellationToken cancellationToken = default)
        => _api.PostAsync($"api/admin/orders/{id}/mark-paid", cancellationToken);

    public Task CancelOrderAsync(int id, CancellationToken cancellationToken = default)
        => _api.PostAsync($"api/admin/orders/{id}/cancel", cancellationToken);

    public Task<List<AdminPaymentListRowDto>> GetPaymentsAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminPaymentListRowDto>>("api/admin/payments", cancellationToken);

    public Task<AdminPaymentDetailsDto> GetPaymentAsync(int id, CancellationToken cancellationToken = default)
        => _api.GetAsync<AdminPaymentDetailsDto>($"api/admin/payments/{id}", cancellationToken);

    public Task<AdminPaymentDetailsDto> CreatePaymentAsync(AdminPaymentCreateDto dto, CancellationToken cancellationToken = default)
        => _api.PostAsync<AdminPaymentCreateDto, AdminPaymentDetailsDto>("api/admin/payments", dto, cancellationToken);

    public Task PatchPaymentStatusAsync(int id, AdminPaymentStatusPatchDto dto, CancellationToken cancellationToken = default)
        => _api.PatchAsync($"api/admin/payments/{id}/status", dto, cancellationToken);

    public Task PatchPaymentOrderAsync(int id, AdminPaymentOrderPatchDto dto, CancellationToken cancellationToken = default)
        => _api.PatchAsync($"api/admin/payments/{id}/order", dto, cancellationToken);

    public Task<List<AdminInstallmentListRowDto>> GetInstallmentsAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminInstallmentListRowDto>>("api/admin/installments", cancellationToken);

    public Task<AdminInstallmentDetailsDto> GetInstallmentAsync(int id, CancellationToken cancellationToken = default)
        => _api.GetAsync<AdminInstallmentDetailsDto>($"api/admin/installments/{id}", cancellationToken);

    public Task<AdminInstallmentDetailsDto> CreateInstallmentAsync(AdminInstallmentCreateDto dto, CancellationToken cancellationToken = default)
        => _api.PostAsync<AdminInstallmentCreateDto, AdminInstallmentDetailsDto>("api/admin/installments", dto, cancellationToken);

    public Task PatchInstallmentStatusAsync(int id, AdminInstallmentStatusPatchDto dto, CancellationToken cancellationToken = default)
        => _api.PatchAsync($"api/admin/installments/{id}/status", dto, cancellationToken);

    public Task PatchInstallmentPaymentStatusAsync(int installmentPaymentId, AdminInstallmentPaymentStatusPatchDto dto,
        CancellationToken cancellationToken = default)
        => _api.PatchAsync($"api/admin/installment-payments/{installmentPaymentId}/status", dto, cancellationToken);
}

