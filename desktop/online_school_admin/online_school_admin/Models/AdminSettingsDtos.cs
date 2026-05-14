namespace online_school_admin.Models;

public sealed class AdminSchoolSettingsDto
{
    public int SettingId { get; set; }
    public string SchoolName { get; set; } = "";
    public string? LogoUrl { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? AboutText { get; set; }
    public string? PrivacyPolicyUrl { get; set; }
    public string? TermsUrl { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public sealed class AdminSchoolSettingsUpdateDto
{
    public string SchoolName { get; set; } = "";
    public string? LogoUrl { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? AboutText { get; set; }
    public string? PrivacyPolicyUrl { get; set; }
    public string? TermsUrl { get; set; }
}

public sealed class AdminSiteSettingsDto
{
    public int SettingId { get; set; }
    public string? SiteName { get; set; }
    public string? MainPageTitle { get; set; }
    public string? MainPageDescription { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public string? VkUrl { get; set; }
    public string? TelegramUrl { get; set; }
    public string? YoutubeUrl { get; set; }
    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }
    public bool MaintenanceMode { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class AdminSiteSettingsUpdateDto
{
    public string? SiteName { get; set; }
    public string? MainPageTitle { get; set; }
    public string? MainPageDescription { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public string? VkUrl { get; set; }
    public string? TelegramUrl { get; set; }
    public string? YoutubeUrl { get; set; }
    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }
    public bool MaintenanceMode { get; set; }
}

public sealed class AdminSiteBannerRowDto
{
    public int BannerId { get; set; }
    public string Title { get; set; } = "";
    public string? Subtitle { get; set; }
    public string? ImageUrl { get; set; }
    public string? ButtonText { get; set; }
    public string? ButtonUrl { get; set; }
    public int BannerOrder { get; set; }
    public bool IsActive { get; set; }
}

public sealed class AdminSiteBannerUpsertDto
{
    public string Title { get; set; } = "";
    public string? Subtitle { get; set; }
    public string? ImageUrl { get; set; }
    public string? ButtonText { get; set; }
    public string? ButtonUrl { get; set; }
    public int BannerOrder { get; set; }
    public bool IsActive { get; set; }
}

public sealed class AdminReorderItemDto2
{
    public int Id { get; set; }
    public int Order { get; set; }
}

public sealed class AdminReorderRequestDto2
{
    public IReadOnlyList<AdminReorderItemDto2> Items { get; set; } = [];
}

