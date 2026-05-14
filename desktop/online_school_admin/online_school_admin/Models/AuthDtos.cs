namespace online_school_admin.Models;

public sealed class LoginAdminResponse
{
    public int UserId { get; set; }
    public int EmployeeId { get; set; }
    public string Email { get; set; } = "";
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? RoleLabel { get; set; }
}

public sealed class RegisterAdminResponse
{
    public int UserId { get; set; }
    public int EmployeeId { get; set; }
    public string Email { get; set; } = "";
    public string? RoleLabel { get; set; }
}

public sealed class UserRoleDto
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = "";
    public string? Description { get; set; }
}
