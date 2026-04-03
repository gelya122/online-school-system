namespace OnlineSchoolAPI.Dto;

public class StudentProgressDto
{
    public int ProgressId { get; set; }
    public int EnrollmentId { get; set; }
    public int LessonId { get; set; }
    public int AccessId { get; set; }
    public bool? IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? WatchTimeSeconds { get; set; }
    public DateTime? LastAccessed { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class CreateStudentProgressDto
{
    public int EnrollmentId { get; set; }
    public int LessonId { get; set; }
    public int AccessId { get; set; }
    public bool? IsCompleted { get; set; }
    public int? WatchTimeSeconds { get; set; }
}

public class UpdateStudentProgressDto
{
    public bool? IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? WatchTimeSeconds { get; set; }
    public DateTime? LastAccessed { get; set; }
}

