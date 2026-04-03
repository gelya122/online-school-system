namespace OnlineSchoolAPI.Dto;

public class EnrollmentDto
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public int InstanceId { get; set; }
    public int? AssignedTeacherId { get; set; }
    public DateTime? EnrolledAt { get; set; }
    public int? EnrollmentStatusId { get; set; }
    public DateTime? CompletedAt { get; set; }
    public decimal? FinalScore { get; set; }
}

public class CreateEnrollmentDto
{
    public int StudentId { get; set; }
    public int InstanceId { get; set; }
    public int? AssignedTeacherId { get; set; }
    public int? EnrollmentStatusId { get; set; }
}

public class UpdateEnrollmentDto
{
    public int? AssignedTeacherId { get; set; }
    public int? EnrollmentStatusId { get; set; }
    public DateTime? CompletedAt { get; set; }
    public decimal? FinalScore { get; set; }
}

