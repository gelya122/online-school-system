namespace OnlineSchoolAPI.Dto;

public class UserRoleDto
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = null!;
    public string? Description { get; set; }
}

public class CreateUserRoleDto
{
    public string RoleName { get; set; } = null!;
    public string? Description { get; set; }
}

public class UpdateUserRoleDto
{
    public string? RoleName { get; set; }
    public string? Description { get; set; }
}

