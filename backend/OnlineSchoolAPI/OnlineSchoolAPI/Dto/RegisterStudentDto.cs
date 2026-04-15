using System.Text.Json.Serialization;

namespace OnlineSchoolAPI.Dto;

public class RegisterStudentDto
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = null!;

    [JsonPropertyName("password")]
    public string Password { get; set; } = null!;

    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = null!;

    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = null!;

    [JsonPropertyName("phone")]
    public string Phone { get; set; } = null!;

    /// <summary>Строка yyyy-MM-dd с фронта (input type=&quot;date&quot;).</summary>
    [JsonPropertyName("dateOfBirth")]
    public string? DateOfBirth { get; set; }

    [JsonPropertyName("classNumber")]
    public int ClassNumber { get; set; }

    [JsonPropertyName("parentPhone")]
    public string ParentPhone { get; set; } = null!;

    [JsonPropertyName("parentEmail")]
    public string ParentEmail { get; set; } = null!;

    [JsonPropertyName("avatarBase64")]
    public string? AvatarBase64 { get; set; }
}

public class RegisterStudentResponseDto
{
    public int UserId { get; set; }
    public int StudentId { get; set; }
    public string Email { get; set; } = null!;
    public string? AvatarUrl { get; set; }

    /// <summary>Отображаемое название роли: описание из справочника или имя роли.</summary>
    public string? RoleLabel { get; set; }
}
