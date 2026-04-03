namespace OnlineSchoolAPI.Dto;

public class UserDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = null!;
    public int RoleId { get; set; }
    public bool? IsEmailConfirmed { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class CreateUserDto
{
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public int RoleId { get; set; }
    public bool? IsEmailConfirmed { get; set; }
    public bool? IsActive { get; set; }
}

public class UpdateUserDto
{
    public string? Email { get; set; }
    public string? PasswordHash { get; set; }
    public int? RoleId { get; set; }
    public bool? IsEmailConfirmed { get; set; }
    public bool? IsActive { get; set; }
}

