namespace OnlineSchoolAPI.Dto;

public class CourseSchedulePlanDto
{
    public int PlanId { get; set; }
    public int InstanceId { get; set; }
    public int LessonId { get; set; }
    public int ReleaseDayOffset { get; set; }
    public TimeOnly? ReleaseTime { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class CreateCourseSchedulePlanDto
{
    public int InstanceId { get; set; }
    public int LessonId { get; set; }
    public int ReleaseDayOffset { get; set; }
    public TimeOnly? ReleaseTime { get; set; }
}

public class UpdateCourseSchedulePlanDto
{
    public int? ReleaseDayOffset { get; set; }
    public TimeOnly? ReleaseTime { get; set; }
}

