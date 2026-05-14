namespace online_school_admin.Models;

public sealed class CurrentUserModel
{
    public int UserId { get; set; }
    public int? EmployeeId { get; set; }
    public string Email { get; set; } = "";
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }
    public int RoleId { get; set; }
    public string RoleName { get; set; } = "";

    public string DisplayName
    {
        get
        {
            var fn = (FirstName ?? "").Trim();
            var ln = (LastName ?? "").Trim();
            var full = $"{fn} {ln}".Trim();
            return string.IsNullOrWhiteSpace(full) ? Email : full;
        }
    }
}

