using System.Net;
using online_school_admin.Models;

namespace online_school_admin.Services;

public sealed class AdminAnalyticsService
{
    private readonly ApiClient _api;

    public AdminAnalyticsService(ApiClient api)
    {
        _api = api;
    }

    private static string BuildQuery(string? period, string? fromText, string? toText)
    {
        var qs = new List<string>();
        if (!string.IsNullOrWhiteSpace(period)) qs.Add($"period={WebUtility.UrlEncode(period.Trim())}");
        if (!string.IsNullOrWhiteSpace(fromText)) qs.Add($"from={WebUtility.UrlEncode(fromText.Trim())}");
        if (!string.IsNullOrWhiteSpace(toText)) qs.Add($"to={WebUtility.UrlEncode(toText.Trim())}");
        return qs.Count == 0 ? "" : "?" + string.Join("&", qs);
    }

    public Task<AdminAnalyticsSummaryDto> GetSummaryAsync(string period, string? fromText, string? toText, CancellationToken cancellationToken = default)
        => _api.GetAsync<AdminAnalyticsSummaryDto>($"api/admin/analytics/summary{BuildQuery(period, fromText, toText)}", cancellationToken);

    public Task<List<AdminDateCountPointDto>> GetApplicationsAsync(string period, string? fromText, string? toText, CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminDateCountPointDto>>($"api/admin/analytics/applications{BuildQuery(period, fromText, toText)}", cancellationToken);

    public Task<List<AdminDateAmountPointDto>> GetRevenueAsync(string period, string? fromText, string? toText, CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminDateAmountPointDto>>($"api/admin/analytics/revenue{BuildQuery(period, fromText, toText)}", cancellationToken);

    public Task<List<AdminNameCountPointDto>> GetOrdersAsync(string period, string? fromText, string? toText, CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminNameCountPointDto>>($"api/admin/analytics/orders{BuildQuery(period, fromText, toText)}", cancellationToken);

    public Task<List<AdminNamePercentPointDto>> GetStudentProgressAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminNamePercentPointDto>>("api/admin/analytics/student-progress", cancellationToken);

    public Task<List<AdminNameCountPointDto>> GetHomeworkAsync(string period, string? fromText, string? toText, CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminNameCountPointDto>>($"api/admin/analytics/homework{BuildQuery(period, fromText, toText)}", cancellationToken);

    public Task<List<AdminPromoCodeUsageAggDto>> GetPromoCodesAsync(string period, string? fromText, string? toText, CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminPromoCodeUsageAggDto>>($"api/admin/analytics/promo-codes{BuildQuery(period, fromText, toText)}", cancellationToken);

    public Task<List<AdminNameCountPointDto>> GetPopularCoursesAsync(string period, string? fromText, string? toText, CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminNameCountPointDto>>($"api/admin/analytics/popular-courses{BuildQuery(period, fromText, toText)}", cancellationToken);
}

