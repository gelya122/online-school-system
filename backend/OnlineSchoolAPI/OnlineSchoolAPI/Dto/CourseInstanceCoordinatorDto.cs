namespace OnlineSchoolAPI.Dto;

public class CourseInstanceCoordinatorDto
{
    public int CoordinatorId { get; set; }
    public int InstanceId { get; set; }
    public int EmployeeId { get; set; }
    public bool? IsLead { get; set; }
    public DateTime? AssignedAt { get; set; }
}

public class CreateCourseInstanceCoordinatorDto
{
    public int InstanceId { get; set; }
    public int EmployeeId { get; set; }
    public bool? IsLead { get; set; }
}

public class UpdateCourseInstanceCoordinatorDto
{
    public bool? IsLead { get; set; }
}

