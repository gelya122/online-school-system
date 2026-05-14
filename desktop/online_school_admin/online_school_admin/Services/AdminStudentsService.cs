using online_school_admin.Models;

namespace online_school_admin.Services;

public sealed class AdminStudentsService
{
    private readonly ApiClient _api;

    public AdminStudentsService(ApiClient api)
    {
        _api = api;
    }

    public Task<List<AdminStudentListRowDto>> GetStudentsAsync(string? search = null, bool? isActive = null, int? classNumber = null, CancellationToken cancellationToken = default)
    {
        var qs = new List<string>();
        if (!string.IsNullOrWhiteSpace(search))
            qs.Add($"search={Uri.EscapeDataString(search.Trim())}");
        if (isActive.HasValue)
            qs.Add($"isActive={isActive.Value.ToString().ToLowerInvariant()}");
        if (classNumber.HasValue)
            qs.Add($"classNumber={classNumber.Value}");

        var url = "api/admin/students";
        if (qs.Count > 0)
            url += "?" + string.Join("&", qs);

        return _api.GetAsync<List<AdminStudentListRowDto>>(url, cancellationToken);
    }

    public Task<AdminStudentDetailsDto> GetStudentAsync(int id, CancellationToken cancellationToken = default)
        => _api.GetAsync<AdminStudentDetailsDto>($"api/admin/students/{id}", cancellationToken);

    public Task<AdminStudentDetailsDto> CreateStudentAsync(AdminStudentCreateDto dto, CancellationToken cancellationToken = default)
        => _api.PostAsync<AdminStudentCreateDto, AdminStudentDetailsDto>("api/admin/students", dto, cancellationToken);

    public Task UpdateStudentAsync(int id, AdminStudentUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PutAsync($"api/admin/students/{id}", dto, cancellationToken);

    public Task ActivateAsync(int id, CancellationToken cancellationToken = default) => _api.PatchAsync($"api/admin/students/{id}/activate", cancellationToken);
    public Task DeactivateAsync(int id, CancellationToken cancellationToken = default) => _api.PatchAsync($"api/admin/students/{id}/deactivate", cancellationToken);

    public Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
        => _api.PatchAsync($"api/admin/students/{id}/soft-delete", cancellationToken);

    public Task<List<AdminStudentNoteDto>> GetNotesAsync(int id, CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminStudentNoteDto>>($"api/admin/students/{id}/notes", cancellationToken);

    public Task<AdminStudentNoteDto> AddNoteAsync(int id, AdminStudentNoteCreateDto dto, CancellationToken cancellationToken = default)
        => _api.PostAsync<AdminStudentNoteCreateDto, AdminStudentNoteDto>($"api/admin/students/{id}/notes", dto, cancellationToken);
}

