namespace online_school_admin.Models;

public sealed class AuthLoginRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

public sealed class AuthLoginResponse
{
    public string AccessToken { get; set; } = "";
    public CurrentUserModel User { get; set; } = new();
}

public sealed class AuthRegisterEmployeeRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? Patronymic { get; set; }
    public string Phone { get; set; } = "";
    public string? BirthDate { get; set; }
    public int? Experience { get; set; }
    public string? AvatarUrl { get; set; }
    public string? AvatarBase64 { get; set; }
    public int RoleId { get; set; }
}

