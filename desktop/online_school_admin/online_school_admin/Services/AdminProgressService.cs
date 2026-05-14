using System.Net;
using online_school_admin.Models;

namespace online_school_admin.Services;

public sealed class AdminProgressService
{
    private readonly ApiClient _api;

    public AdminProgressService(ApiClient api)
    {
        _api = api;
    }

    public Task<List<AdminStudentProgressListRowDto>> GetProgressAsync(string? search = null, int? courseId = null, int? instanceId = null,
        CancellationToken cancellationToken = default)
    {
        var qs = new List<string>();
        if (!string.IsNullOrWhiteSpace(search)) qs.Add($"search={WebUtility.UrlEncode(search.Trim())}");
        if (courseId.HasValue) qs.Add($"courseId={courseId.Value}");
        if (instanceId.HasValue) qs.Add($"instanceId={instanceId.Value}");
        var url = "api/admin/student-progress";
        if (qs.Count > 0) url += "?" + string.Join("&", qs);
        return _api.GetAsync<List<AdminStudentProgressListRowDto>>(url, cancellationToken);
    }

    public Task<List<AdminStudentProgressListRowDto>> GetStudentProgressAsync(int studentId, CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminStudentProgressListRowDto>>($"api/admin/students/{studentId}/progress", cancellationToken);

    public Task<AdminEnrollmentProgressDto> GetEnrollmentProgressAsync(int enrollmentId, CancellationToken cancellationToken = default)
        => _api.GetAsync<AdminEnrollmentProgressDto>($"api/admin/enrollments/{enrollmentId}/progress", cancellationToken);
}

