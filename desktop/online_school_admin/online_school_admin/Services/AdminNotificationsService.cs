using online_school_admin.Models;

namespace online_school_admin.Services;

public sealed class AdminNotificationsService
{
    private readonly ApiClient _api;

    public AdminNotificationsService(ApiClient api)
    {
        _api = api;
    }

    public Task<List<AdminNotificationListRowDto>> GetNotificationsAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminNotificationListRowDto>>("api/admin/notifications", cancellationToken);

    public Task CreateNotificationAsync(AdminCreateNotificationDto dto, CancellationToken cancellationToken = default)
        => _api.PostAsync("api/admin/notifications", dto, cancellationToken);

    public Task MarkReadAsync(int id, CancellationToken cancellationToken = default)
        => _api.PatchAsync<object>($"api/admin/notifications/{id}/read", new { }, cancellationToken);

    public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        => _api.DeleteAsync($"api/admin/notifications/{id}", cancellationToken);

    public Task<List<AdminMailingCampaignListRowDto>> GetCampaignsAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminMailingCampaignListRowDto>>("api/admin/mailing-campaigns", cancellationToken);

    public Task<AdminMailingCampaignDetailsDto> GetCampaignAsync(int id, CancellationToken cancellationToken = default)
        => _api.GetAsync<AdminMailingCampaignDetailsDto>($"api/admin/mailing-campaigns/{id}", cancellationToken);

    public Task<AdminMailingCampaignDetailsDto> CreateCampaignAsync(AdminMailingCampaignUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PostAsync<AdminMailingCampaignUpsertDto, AdminMailingCampaignDetailsDto>("api/admin/mailing-campaigns", dto, cancellationToken);

    public Task UpdateCampaignAsync(int id, AdminMailingCampaignUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PutAsync($"api/admin/mailing-campaigns/{id}", dto, cancellationToken);

    public Task SendCampaignAsync(int id, CancellationToken cancellationToken = default)
        => _api.PostAsync($"api/admin/mailing-campaigns/{id}/send", cancellationToken);

    public Task CancelCampaignAsync(int id, CancellationToken cancellationToken = default)
        => _api.PostAsync($"api/admin/mailing-campaigns/{id}/cancel", cancellationToken);

    public Task<List<AdminMailingRecipientRowDto>> GetRecipientsAsync(int id, CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminMailingRecipientRowDto>>($"api/admin/mailing-campaigns/{id}/recipients", cancellationToken);
}

