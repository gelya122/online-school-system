using System.Net;
using online_school_admin.Models;

namespace online_school_admin.Services;

public sealed class AdminInstancesService
{
    private readonly ApiClient _api;

    public AdminInstancesService(ApiClient api)
    {
        _api = api;
    }

    public Task<List<AdminCourseInstanceListRowDto>> GetInstancesAsync(string? search = null, int? courseId = null, string? status = null,
        bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(search)) query.Add($"search={WebUtility.UrlEncode(search)}");
        if (courseId.HasValue) query.Add($"courseId={courseId.Value}");
        if (!string.IsNullOrWhiteSpace(status)) query.Add($"status={WebUtility.UrlEncode(status)}");
        if (isActive.HasValue) query.Add($"isActive={isActive.Value.ToString().ToLower()}");

        var qs = query.Count == 0 ? "" : "?" + string.Join("&", query);
        return _api.GetAsync<List<AdminCourseInstanceListRowDto>>($"api/admin/course-instances{qs}", cancellationToken);
    }

    public Task<AdminCourseInstanceDetailsDto> GetInstanceAsync(int id, CancellationToken cancellationToken = default)
        => _api.GetAsync<AdminCourseInstanceDetailsDto>($"api/admin/course-instances/{id}", cancellationToken);

    public Task<AdminCourseInstanceDetailsDto> CreateInstanceAsync(AdminCourseInstanceUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PostAsync<AdminCourseInstanceUpsertDto, AdminCourseInstanceDetailsDto>("api/admin/course-instances", dto, cancellationToken);

    public Task<AdminCourseInstanceDetailsDto> CreateBootstrapAsync(AdminCourseInstanceBootstrapDto dto, CancellationToken cancellationToken = default)
        => _api.PostAsync<AdminCourseInstanceBootstrapDto, AdminCourseInstanceDetailsDto>("api/admin/course-instances/bootstrap", dto, cancellationToken);

    public Task UpdateInstanceAsync(int id, AdminCourseInstanceUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PutAsync($"api/admin/course-instances/{id}", dto, cancellationToken);

    public Task PatchStatusAsync(int id, AdminInstanceStatusPatchDto dto, CancellationToken cancellationToken = default)
        => _api.PatchAsync($"api/admin/course-instances/{id}/status", dto, cancellationToken);

    public Task ArchiveAsync(int id, CancellationToken cancellationToken = default)
        => _api.PatchAsync($"api/admin/course-instances/{id}/archive", cancellationToken);

    public Task PatchInstanceActiveAsync(int id, AdminInstanceIsActivePatchDto dto, CancellationToken cancellationToken = default)
        => _api.PatchAsync($"api/admin/course-instances/{id}/active", dto, cancellationToken);

    public Task AddTeacherAsync(int id, AdminAssignTeacherDto dto, CancellationToken cancellationToken = default)
        => _api.PostAsync($"api/admin/course-instances/{id}/teachers", dto, cancellationToken);

    public Task RemoveTeacherAsync(int id, int employeeId, CancellationToken cancellationToken = default)
        => _api.DeleteAsync($"api/admin/course-instances/{id}/teachers/{employeeId}", cancellationToken);

    public Task AddCoordinatorAsync(int id, AdminAssignCoordinatorDto dto, CancellationToken cancellationToken = default)
        => _api.PostAsync($"api/admin/course-instances/{id}/coordinators", dto, cancellationToken);

    public Task RemoveCoordinatorAsync(int id, int employeeId, CancellationToken cancellationToken = default)
        => _api.DeleteAsync($"api/admin/course-instances/{id}/coordinators/{employeeId}", cancellationToken);

    // students in instance
    public Task<List<AdminInstanceStudentRowDto>> GetInstanceStudentsAsync(int instanceId, CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminInstanceStudentRowDto>>($"api/admin/course-instances/{instanceId}/students", cancellationToken);

    public Task EnrollStudentAsync(int instanceId, int studentId, CancellationToken cancellationToken = default)
        => _api.PostAsync($"api/admin/course-instances/{instanceId}/students", new AdminEnrollStudentDto { StudentId = studentId }, cancellationToken);

    public Task EnrollStudentsBulkAsync(int instanceId, List<int> studentIds, CancellationToken cancellationToken = default)
        => _api.PostAsync($"api/admin/course-instances/{instanceId}/students/bulk", new AdminEnrollStudentsBulkDto { StudentIds = studentIds }, cancellationToken);

    public Task PatchEnrollmentStatusAsync(int enrollmentId, AdminEnrollmentStatusPatchDto dto,
        CancellationToken cancellationToken = default)
        => _api.PatchAsync($"api/admin/enrollments/{enrollmentId}/status", dto, cancellationToken);

    public Task PatchEnrollmentAssignedTeacherAsync(int enrollmentId, AdminEnrollmentAssignedTeacherPatchDto dto,
        CancellationToken cancellationToken = default)
        => _api.PatchAsync($"api/admin/enrollments/{enrollmentId}/assigned-teacher", dto, cancellationToken);

    public Task DeleteEnrollmentAsync(int enrollmentId, CancellationToken cancellationToken = default)
        => _api.DeleteAsync($"api/admin/enrollments/{enrollmentId}", cancellationToken);

    // schedule
    public Task<List<AdminInstanceScheduleRowDto>> GetScheduleAsync(int instanceId, CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminInstanceScheduleRowDto>>($"api/admin/course-instances/{instanceId}/schedule", cancellationToken);

    public Task GenerateScheduleAsync(int instanceId, AdminGenerateInstanceScheduleDto dto, CancellationToken cancellationToken = default)
        => _api.PostAsync($"api/admin/course-instances/{instanceId}/schedule/generate", dto, cancellationToken);

    public Task UpdateScheduleAsync(int scheduleId, AdminUpdateInstanceScheduleDto dto, CancellationToken cancellationToken = default)
        => _api.PutAsync($"api/admin/course-instance-schedule/{scheduleId}", dto, cancellationToken);

    public Task OpenLessonForAllAsync(int scheduleId, CancellationToken cancellationToken = default)
        => _api.PostAsync($"api/admin/course-instance-schedule/{scheduleId}/open-for-all", cancellationToken);

    public Task OpenLessonForStudentAsync(int scheduleId, int studentId, CancellationToken cancellationToken = default)
        => _api.PostAsync($"api/admin/course-instance-schedule/{scheduleId}/open-for-student", new AdminOpenForStudentDto { StudentId = studentId },
            cancellationToken);

    public Task CloseLessonForStudentAsync(int scheduleId, int studentId, CancellationToken cancellationToken = default)
        => _api.PostAsync($"api/admin/course-instance-schedule/{scheduleId}/close-for-student", new AdminOpenForStudentDto { StudentId = studentId },
            cancellationToken);
}

