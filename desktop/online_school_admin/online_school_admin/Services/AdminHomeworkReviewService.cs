using System.Net;
using online_school_admin.Models;

namespace online_school_admin.Services;

public sealed class AdminHomeworkReviewService
{
    private readonly ApiClient _api;

    public AdminHomeworkReviewService(ApiClient api)
    {
        _api = api;
    }

    public Task<List<AdminHomeworkAnswerReviewQueueRowDto>> GetQueueAsync(
        string? search = null,
        string? status = null,
        string? reviewState = null,
        int? courseId = null,
        int? instanceId = null,
        int? reviewerId = null,
        int? mentorId = null,
        int? studentId = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var qs = new List<string>();
        if (!string.IsNullOrWhiteSpace(search)) qs.Add($"search={WebUtility.UrlEncode(search.Trim())}");
        if (!string.IsNullOrWhiteSpace(status)) qs.Add($"status={WebUtility.UrlEncode(status.Trim())}");
        if (!string.IsNullOrWhiteSpace(reviewState)) qs.Add($"reviewState={WebUtility.UrlEncode(reviewState.Trim())}");
        if (courseId.HasValue) qs.Add($"courseId={courseId.Value}");
        if (instanceId.HasValue) qs.Add($"instanceId={instanceId.Value}");
        if (reviewerId.HasValue) qs.Add($"reviewerId={reviewerId.Value}");
        if (mentorId.HasValue) qs.Add($"mentorId={mentorId.Value}");
        if (studentId.HasValue) qs.Add($"studentId={studentId.Value}");
        if (from.HasValue) qs.Add($"from={WebUtility.UrlEncode(from.Value.ToString("o"))}");
        if (to.HasValue) qs.Add($"to={WebUtility.UrlEncode(to.Value.ToString("o"))}");

        var url = "api/admin/homework-submissions/review-queue";
        if (qs.Count > 0) url += "?" + string.Join("&", qs);
        return _api.GetAsync<List<AdminHomeworkAnswerReviewQueueRowDto>>(url, cancellationToken);
    }

    public Task<AdminHomeworkSubmissionDetailsDto> GetSubmissionAsync(int submissionId, CancellationToken cancellationToken = default)
        => _api.GetAsync<AdminHomeworkSubmissionDetailsDto>($"api/admin/homework-submissions/{submissionId}", cancellationToken);

    public Task ReviewTaskAsync(int taskSubmissionId, AdminHomeworkTaskSubmissionReviewDto dto, CancellationToken cancellationToken = default)
        => _api.PutAsync($"api/admin/homework-task-submissions/{taskSubmissionId}/review", dto, cancellationToken);

    public Task ApproveAsync(int submissionId, CancellationToken cancellationToken = default)
        => _api.PostAsync($"api/admin/homework-submissions/{submissionId}/approve", cancellationToken);

    public Task RequestRevisionAsync(int submissionId, CancellationToken cancellationToken = default)
        => _api.PostAsync($"api/admin/homework-submissions/{submissionId}/request-revision", cancellationToken);

    public Task RejectAsync(int submissionId, CancellationToken cancellationToken = default)
        => _api.PostAsync($"api/admin/homework-submissions/{submissionId}/reject", cancellationToken);

    public Task AssignReviewerAsync(int submissionId, int? reviewerEmployeeId, CancellationToken cancellationToken = default)
        => _api.PatchAsync($"api/admin/homework-submissions/{submissionId}/assign-reviewer",
            new AdminAssignReviewerDto { ReviewerEmployeeId = reviewerEmployeeId }, cancellationToken);
}
