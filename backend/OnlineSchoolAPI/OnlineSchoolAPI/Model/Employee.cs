using System;
using System.Collections.Generic;

namespace OnlineSchoolAPI.Models;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public int UserId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? Patronymic { get; set; }

    public string? Phone { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? AvatarUrl { get; set; }

    public int? WorkExperience { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<CourseInstanceCoordinator> CourseInstanceCoordinators { get; set; } = new List<CourseInstanceCoordinator>();

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<StudentLessonAccess> StudentLessonAccesses { get; set; } = new List<StudentLessonAccess>();

    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();

    public virtual ICollection<TrialApplication> TrialApplications { get; set; } = new List<TrialApplication>();

    public virtual User User { get; set; } = null!;
}
