namespace OnlineSchoolAPI.Dto;

public class StudentLessonAccessDto
{
    public int AccessId { get; set; }
    public int EnrollmentId { get; set; }
    public int LessonId { get; set; }
    public int? PlanId { get; set; }
    public DateOnly PlannedAccessDate { get; set; }
    public TimeOnly? PlannedAccessTime { get; set; }
    public DateTime? ActualOpenDatetime { get; set; }
    public bool? IsAvailable { get; set; }
    public int? OpenedByEmployeeId { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class CreateStudentLessonAccessDto
{
    public int EnrollmentId { get; set; }
    public int LessonId { get; set; }
    public int? PlanId { get; set; }
    public DateOnly PlannedAccessDate { get; set; }
    public TimeOnly? PlannedAccessTime { get; set; }
    public bool? IsAvailable { get; set; }
}

public class UpdateStudentLessonAccessDto
{
    public DateOnly? PlannedAccessDate { get; set; }
    public TimeOnly? PlannedAccessTime { get; set; }
    public DateTime? ActualOpenDatetime { get; set; }
    public bool? IsAvailable { get; set; }
    public int? OpenedByEmployeeId { get; set; }
}

