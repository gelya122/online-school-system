using System;
using System.Collections.Generic;

namespace OnlineSchoolAPI.Models;

public partial class SchoolSetting
{
    public int SettingId { get; set; }

    public string SchoolName { get; set; } = null!;

    public string? LogoUrl { get; set; }

    public string? ContactPhone { get; set; }

    public string? ContactEmail { get; set; }

    public string? Address { get; set; }

    public string? AboutSchoolText { get; set; }

    public string? PrivacyPolicyUrl { get; set; }

    public string? TermsOfUseUrl { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
