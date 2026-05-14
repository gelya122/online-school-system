namespace OnlineSchoolAPI.Dto;

/// <summary>Регистрация сотрудника для входа в админ-приложение (не ученик).</summary>
public class RegisterAdminDto
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Patronymic { get; set; }
    public string Phone { get; set; } = null!;
    public int RoleId { get; set; }
}

public class RegisterAdminResponseDto
{
    public int UserId { get; set; }
    public int EmployeeId { get; set; }
    public string Email { get; set; } = null!;
    public string? RoleLabel { get; set; }
}

public class LoginAdminResponseDto
{
    public int UserId { get; set; }
    public int EmployeeId { get; set; }
    public string Email { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? RoleLabel { get; set; }
}
