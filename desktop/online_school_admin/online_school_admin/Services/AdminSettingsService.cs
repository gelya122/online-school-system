using online_school_admin.Models;

namespace online_school_admin.Services;

public sealed class AdminSettingsService
{
    private readonly ApiClient _api;

    public AdminSettingsService(ApiClient api)
    {
        _api = api;
    }

    public Task<AdminSchoolSettingsDto> GetSchoolSettingsAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<AdminSchoolSettingsDto>("api/admin/school-settings", cancellationToken);

    public Task UpdateSchoolSettingsAsync(AdminSchoolSettingsUpdateDto dto, CancellationToken cancellationToken = default)
        => _api.PutAsync("api/admin/school-settings", dto, cancellationToken);

    public Task<AdminSiteSettingsDto> GetSiteSettingsAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<AdminSiteSettingsDto>("api/admin/site-settings", cancellationToken);

    public Task UpdateSiteSettingsAsync(AdminSiteSettingsUpdateDto dto, CancellationToken cancellationToken = default)
        => _api.PutAsync("api/admin/site-settings", dto, cancellationToken);

    public Task<List<AdminSiteBannerRowDto>> GetBannersAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminSiteBannerRowDto>>("api/admin/site-banners", cancellationToken);

    public Task<AdminSiteBannerRowDto> CreateBannerAsync(AdminSiteBannerUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PostAsync<AdminSiteBannerUpsertDto, AdminSiteBannerRowDto>("api/admin/site-banners", dto, cancellationToken);

    public Task UpdateBannerAsync(int id, AdminSiteBannerUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PutAsync($"api/admin/site-banners/{id}", dto, cancellationToken);

    public Task DeleteBannerAsync(int id, CancellationToken cancellationToken = default)
        => _api.DeleteAsync($"api/admin/site-banners/{id}", cancellationToken);

    public Task ReorderBannersAsync(AdminReorderRequestDto2 dto, CancellationToken cancellationToken = default)
        => _api.PatchAsync("api/admin/site-banners/reorder", dto, cancellationToken);

    public Task<List<AdminFaqCategoryDto>> GetFaqTreeAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminFaqCategoryDto>>("api/admin/faq", cancellationToken);

    public Task<AdminFaqCategoryDto> CreateFaqCategoryAsync(AdminFaqCategoryUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PostAsync<AdminFaqCategoryUpsertDto, AdminFaqCategoryDto>("api/admin/faq/categories", dto, cancellationToken);

    public Task UpdateFaqCategoryAsync(int id, AdminFaqCategoryUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PutAsync($"api/admin/faq/categories/{id}", dto, cancellationToken);

    public Task DeleteFaqCategoryAsync(int id, CancellationToken cancellationToken = default)
        => _api.DeleteAsync($"api/admin/faq/categories/{id}", cancellationToken);

    public Task<AdminFaqItemDto> CreateFaqItemAsync(AdminFaqItemUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PostAsync<AdminFaqItemUpsertDto, AdminFaqItemDto>("api/admin/faq/items", dto, cancellationToken);

    public Task UpdateFaqItemAsync(int id, AdminFaqItemUpsertDto dto, CancellationToken cancellationToken = default)
        => _api.PutAsync($"api/admin/faq/items/{id}", dto, cancellationToken);

    public Task DeleteFaqItemAsync(int id, CancellationToken cancellationToken = default)
        => _api.DeleteAsync($"api/admin/faq/items/{id}", cancellationToken);

    public Task<List<AdminReviewListRowDto>> GetReviewsAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<List<AdminReviewListRowDto>>("api/admin/reviews", cancellationToken);

    public Task SetReviewPublishedAsync(int id, bool isPublished, CancellationToken cancellationToken = default)
        => _api.PatchAsync($"api/admin/reviews/{id}/published", new AdminReviewPublishedDto { IsPublished = isPublished }, cancellationToken);
}

