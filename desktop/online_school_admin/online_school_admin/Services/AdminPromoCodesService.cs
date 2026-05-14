using System.Net;
using online_school_admin.Models;

namespace online_school_admin.Services;

public sealed class AdminPromoCodesService
{
    private readonly ApiClient _api;

    public AdminPromoCodesService(ApiClient api)
    {
        _api = api;
    }

    public Task<List<AdminPromoCodeListRowDto>> GetPromoCodesAsync(string? search = null, bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var qs = new List<string>();
        if (!string.IsNullOrWhiteSpace(search)) qs.Add($"search={WebUtility.UrlEncode(search.Trim())}");
        if (isActive.HasValue) qs.Add($"isActive={(isActive.Value ? "true" : "false")}");
        var url = "api/admin/promo-codes";
        if (qs.Count > 0) url += "?" + string.Join("&", qs);
        return _api.GetAsync<List<AdminPromoCodeListRowDto>>(url, cancellationToken);
    }

    public Task<List<AdminDiscountTypeDictDto>> GetDiscountTypesAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminDiscountTypeDictDto>>("api/admin/promo-codes/discount-types", cancellationToken);

    public Task<AdminPromoCodeDetailsDto> GetPromoCodeAsync(int id, CancellationToken cancellationToken = default)
        => _api.GetAsync<AdminPromoCodeDetailsDto>($"api/admin/promo-codes/{id}", cancellationToken);

    public Task<AdminPromoCodeDetailsDto> CreatePromoCodeAsync(AdminPromoCodeUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PostAsync<AdminPromoCodeUpsertDto, AdminPromoCodeDetailsDto>("api/admin/promo-codes", dto, cancellationToken);

    public Task UpdatePromoCodeAsync(int id, AdminPromoCodeUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PutAsync($"api/admin/promo-codes/{id}", dto, cancellationToken);

    public Task ActivatePromoCodeAsync(int id, CancellationToken cancellationToken = default)
        => _api.PatchAsync<object>($"api/admin/promo-codes/{id}/activate", new { }, cancellationToken);

    public Task DeactivatePromoCodeAsync(int id, CancellationToken cancellationToken = default)
        => _api.PatchAsync<object>($"api/admin/promo-codes/{id}/deactivate", new { }, cancellationToken);

    public Task<List<AdminPromoCodeUsageRowDto>> GetPromoCodeUsagesAsync(int id, CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminPromoCodeUsageRowDto>>($"api/admin/promo-codes/{id}/usages", cancellationToken);

    public Task DeletePromoCodeAsync(int id, CancellationToken cancellationToken = default)
        => _api.DeleteAsync($"api/admin/promo-codes/{id}", cancellationToken);
}

