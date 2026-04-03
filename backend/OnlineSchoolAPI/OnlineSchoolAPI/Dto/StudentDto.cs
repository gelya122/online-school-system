namespace OnlineSchoolAPI.Dto;

public class StudentDto
{
    public int StudentId { get; set; }
    public int UserId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Phone { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? AvatarUrl { get; set; }
    public int ClassNumber { get; set; }
    public string? ParentPhone { get; set; }
    public string? ParentEmail { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class CreateStudentDto
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Phone { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? AvatarUrl { get; set; }
    public int ClassNumber { get; set; }
    public string? ParentPhone { get; set; }
    public string? ParentEmail { get; set; }
}

public class UpdateStudentDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? AvatarUrl { get; set; }
    public int? ClassNumber { get; set; }
    public string? ParentPhone { get; set; }
    public string? ParentEmail { get; set; }
}

