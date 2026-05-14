namespace online_school_admin.Services;

using online_school_admin.Models;

public sealed class AdminEmployeesService
{
    private readonly ApiClient _api;

    public AdminEmployeesService(ApiClient api)
    {
        _api = api;
    }

    public Task<List<AdminEmployeeListRowDto>> GetEmployeesAsync(string? search = null, int? roleId = null, CancellationToken cancellationToken = default)
    {
        var qs = new List<string>();
        if (!string.IsNullOrWhiteSpace(search))
            qs.Add($"search={Uri.EscapeDataString(search.Trim())}");
        if (roleId.HasValue)
            qs.Add($"roleId={roleId.Value}");

        var url = "api/admin/employees";
        if (qs.Count > 0)
            url += "?" + string.Join("&", qs);

        return _api.GetAsync<List<AdminEmployeeListRowDto>>(url, cancellationToken);
    }

    public Task<AdminEmployeeDetailsDto> GetEmployeeAsync(int id, CancellationToken cancellationToken = default)
        => _api.GetAsync<AdminEmployeeDetailsDto>($"api/admin/employees/{id}", cancellationToken);

    public Task<AdminEmployeeDetailsDto> CreateAsync(AdminEmployeeCreateDto dto, CancellationToken cancellationToken = default)
        => _api.PostAsync<AdminEmployeeCreateDto, AdminEmployeeDetailsDto>("api/admin/employees", dto, cancellationToken);

    public Task UpdateAsync(int id, AdminEmployeeUpdateDto dto, CancellationToken cancellationToken = default)
        => _api.PutAsync($"api/admin/employees/{id}", dto, cancellationToken);

    public Task ActivateAsync(int id, CancellationToken cancellationToken = default)
        => _api.PatchAsync($"api/admin/employees/{id}/activate", cancellationToken);

    public Task DeactivateAsync(int id, CancellationToken cancellationToken = default)
        => _api.PatchAsync($"api/admin/employees/{id}/deactivate", cancellationToken);

    public Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
        => _api.PatchAsync($"api/admin/employees/{id}/soft-delete", cancellationToken);

    public Task ChangeRoleAsync(int id, int roleId, CancellationToken cancellationToken = default)
        => _api.PatchAsync($"api/admin/employees/{id}/role", new AdminEmployeeChangeRoleDto { RoleId = roleId }, cancellationToken);
}

