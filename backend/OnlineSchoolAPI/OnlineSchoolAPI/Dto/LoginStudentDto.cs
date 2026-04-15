namespace OnlineSchoolAPI.Dto;

public class LoginStudentDto
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class LoginStudentResponseDto
{
    public int UserId { get; set; }
    public int? StudentId { get; set; }
    public string Email { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }

    /// <summary>Отображаемое название роли: описание из справочника или имя роли.</summary>
    public string? RoleLabel { get; set; }
}
