using online_school_admin.ViewModels;

namespace online_school_admin.Services;

public sealed class AdminDashboardService
{
    private readonly ApiClient _api;

    public AdminDashboardService(ApiClient api)
    {
        _api = api;
    }

    public Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<DashboardSummaryDto>("api/admin/dashboard/summary", cancellationToken);

    public Task<List<RecentApplicationRowDto>> GetRecentApplicationsAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<List<RecentApplicationRowDto>>("api/admin/dashboard/recent-applications", cancellationToken);

    public Task<List<HomeworkReviewRowDto>> GetHomeworkReviewQueueAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<List<HomeworkReviewRowDto>>("api/admin/dashboard/homework-review-queue", cancellationToken);

    public Task<List<UpcomingInstanceRowDto>> GetUpcomingInstancesAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<List<UpcomingInstanceRowDto>>("api/admin/dashboard/upcoming-instances", cancellationToken);
}

