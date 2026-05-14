using System.Net;
using online_school_admin.Models;

namespace online_school_admin.Services;

public sealed class AdminAuditLogService
{
    private readonly ApiClient _api;

    public AdminAuditLogService(ApiClient api)
    {
        _api = api;
    }

    public Task<AdminAuditLogPageDto> GetPageAsync(
        int? employeeId,
        int? userId,
        string? entityType,
        string? from,
        string? to,
        string? search,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var qs = new List<string>();
        if (employeeId is > 0) qs.Add($"employeeId={employeeId.Value}");
        if (userId is > 0) qs.Add($"userId={userId.Value}");
        if (!string.IsNullOrWhiteSpace(entityType)) qs.Add($"entityType={WebUtility.UrlEncode(entityType.Trim())}");
        if (!string.IsNullOrWhiteSpace(from)) qs.Add($"from={WebUtility.UrlEncode(from.Trim())}");
        if (!string.IsNullOrWhiteSpace(to)) qs.Add($"to={WebUtility.UrlEncode(to.Trim())}");
        if (!string.IsNullOrWhiteSpace(search)) qs.Add($"search={WebUtility.UrlEncode(search.Trim())}");
        qs.Add($"skip={skip}");
        qs.Add($"take={take}");
        var url = "api/admin/audit-log?" + string.Join("&", qs);
        return _api.GetAsync<AdminAuditLogPageDto>(url, cancellationToken);
    }
}
