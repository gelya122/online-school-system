using System.Net;
using online_school_admin.Models;

namespace online_school_admin.Services;

public sealed class AdminApplicationsService
{
    private readonly ApiClient _api;

    public AdminApplicationsService(ApiClient api)
    {
        _api = api;
    }

    public Task<List<AdminApplicationStatusDictDto>> GetStatusesAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminApplicationStatusDictDto>>("api/admin/applications/statuses", cancellationToken);

    public Task<List<AdminApplicationListRowDto>> GetApplicationsAsync(
        string? search = null,
        bool searchLastNameOnly = false,
        int? statusId = null,
        int? managerId = null,
        int? subjectId = null,
        string? createdFrom = null,
        string? createdTo = null,
        string? scope = null,
        CancellationToken cancellationToken = default)
    {
        var qs = new List<string>();
        if (!string.IsNullOrWhiteSpace(search)) qs.Add($"search={WebUtility.UrlEncode(search.Trim())}");
        if (searchLastNameOnly) qs.Add("searchLastNameOnly=true");
        if (statusId.HasValue) qs.Add($"statusId={statusId.Value}");
        if (managerId.HasValue) qs.Add($"managerId={managerId.Value}");
        if (subjectId.HasValue) qs.Add($"subjectId={subjectId.Value}");
        if (!string.IsNullOrWhiteSpace(createdFrom)) qs.Add($"createdFrom={WebUtility.UrlEncode(createdFrom.Trim())}");
        if (!string.IsNullOrWhiteSpace(createdTo)) qs.Add($"createdTo={WebUtility.UrlEncode(createdTo.Trim())}");
        if (!string.IsNullOrWhiteSpace(scope)) qs.Add($"scope={WebUtility.UrlEncode(scope.Trim())}");
        var url = "api/admin/applications";
        if (qs.Count > 0) url += "?" + string.Join("&", qs);
        return _api.GetAsync<List<AdminApplicationListRowDto>>(url, cancellationToken);
    }

    public Task<AdminApplicationDetailsDto> GetApplicationAsync(int id, CancellationToken cancellationToken = default)
        => _api.GetAsync<AdminApplicationDetailsDto>($"api/admin/applications/{id}", cancellationToken);

    public Task<AdminApplicationDetailsDto> CreateAsync(AdminApplicationUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PostAsync<AdminApplicationUpsertDto, AdminApplicationDetailsDto>("api/admin/applications", dto, cancellationToken);

    public Task UpdateAsync(int id, AdminApplicationUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PutAsync($"api/admin/applications/{id}", dto, cancellationToken);

    public Task PatchStatusAsync(int id, int statusId, string? reasonComment = null, CancellationToken cancellationToken = default)
        => _api.PatchAsync($"api/admin/applications/{id}/status",
            new AdminApplicationStatusPatchDto { StatusId = statusId, ReasonComment = reasonComment }, cancellationToken);

    public Task PatchManagerAsync(int id, int? managerId, string? reasonComment = null, CancellationToken cancellationToken = default)
        => _api.PatchAsync($"api/admin/applications/{id}/manager",
            new AdminApplicationManagerPatchDto { ManagerId = managerId, ReasonComment = reasonComment }, cancellationToken);

    public Task ClaimAsync(int id, CancellationToken cancellationToken = default)
        => _api.PostAsync($"api/admin/applications/{id}/claim", cancellationToken);

    public Task PatchContactAsync(int id, AdminApplicationContactPatchDto dto, CancellationToken cancellationToken = default)
        => _api.PatchAsync($"api/admin/applications/{id}/contact", dto, cancellationToken);

    public Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
        => _api.DeleteAsync($"api/admin/applications/{id}", cancellationToken);

    public Task<AdminApplicationCommentDto> AddCommentAsync(int id, string text, CancellationToken cancellationToken = default)
        => _api.PostAsync<AdminApplicationCommentCreateDto, AdminApplicationCommentDto>($"api/admin/applications/{id}/comments",
            new AdminApplicationCommentCreateDto { CommentText = text }, cancellationToken);

    public Task<AdminConvertApplicationToStudentResultDto> ConvertToStudentAsync(int id, CancellationToken cancellationToken = default)
        => _api.PostAsync<AdminConvertApplicationToStudentResultDto>($"api/admin/applications/{id}/convert-to-student", cancellationToken);
}
