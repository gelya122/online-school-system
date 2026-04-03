namespace OnlineSchoolAPI.Dto;

public class CourseInstanceDto
{
    public int InstanceId { get; set; }
    public int CourseId { get; set; }
    public string InstanceName { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int? TotalWeeks { get; set; }
    public int? LessonsPerWeek { get; set; }
    public string? ScheduleDescription { get; set; }
    public int? MaxStudents { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class CreateCourseInstanceDto
{
    public int CourseId { get; set; }
    public string InstanceName { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int? TotalWeeks { get; set; }
    public int? LessonsPerWeek { get; set; }
    public string? ScheduleDescription { get; set; }
    public int? MaxStudents { get; set; }
    public bool? IsActive { get; set; }
}

public class UpdateCourseInstanceDto
{
    public string? InstanceName { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int? TotalWeeks { get; set; }
    public int? LessonsPerWeek { get; set; }
    public string? ScheduleDescription { get; set; }
    public int? MaxStudents { get; set; }
    public bool? IsActive { get; set; }
}

