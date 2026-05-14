namespace OnlineSchoolAPI.Dto;

public sealed class AuthLoginRequestDto
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public sealed class AuthRegisterEmployeeRequestDto
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Patronymic { get; set; }
    public string Phone { get; set; } = null!;
    /// <summary>Дата рождения в формате ГГГГ-ММ-ДД (опционально).</summary>
    public string? BirthDate { get; set; }
    /// <summary>Стаж работы в годах (опционально).</summary>
    public int? Experience { get; set; }
    /// <summary>URL аватара (опционально).</summary>
    public string? AvatarUrl { get; set; }
    /// <summary>Data URL или чистый base64 изображения (опционально).</summary>
    public string? AvatarBase64 { get; set; }
    public int RoleId { get; set; }
}

public sealed class AuthCurrentUserDto
{
    public int UserId { get; set; }
    public int? EmployeeId { get; set; }
    public string Email { get; set; } = "";
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }
    public int RoleId { get; set; }
    public string RoleName { get; set; } = "";
}

public sealed class AuthLoginResponseDto
{
    public string AccessToken { get; set; } = "";
    public AuthCurrentUserDto User { get; set; } = new();
}

