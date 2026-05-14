using online_school_admin.Models;

namespace online_school_admin.Services;

public sealed class AdminProfileService
{
    private readonly ApiClient _api;

    public AdminProfileService(ApiClient api)
    {
        _api = api;
    }

    public Task<AdminProfileDto> GetAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<AdminProfileDto>("api/admin/profile", cancellationToken);

    public Task UpdateAsync(AdminProfileUpdateDto dto, CancellationToken cancellationToken = default)
        => _api.PutAsync("api/admin/profile", dto, cancellationToken);

    public Task ChangePasswordAsync(AdminChangePasswordDto dto, CancellationToken cancellationToken = default)
        => _api.PostAsync("api/admin/profile/change-password", dto, cancellationToken);

    public Task<AdminAvatarUploadResultDto> UploadAvatarAsync(AdminUploadAvatarDto dto, CancellationToken cancellationToken = default)
        => _api.PostAsync<AdminUploadAvatarDto, AdminAvatarUploadResultDto>("api/admin/profile/avatar", dto, cancellationToken);
}

