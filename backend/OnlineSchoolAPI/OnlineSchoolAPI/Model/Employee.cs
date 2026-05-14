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

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<User> CreatedUsers { get; set; } = new List<User>();

    public virtual ICollection<CourseInstanceStaff> CourseInstanceStaffAssignments { get; set; } = new List<CourseInstanceStaff>();

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();

    public virtual ICollection<TrialApplication> TrialApplications { get; set; } = new List<TrialApplication>();

    public virtual ICollection<StudentNote> StudentNotes { get; set; } = new List<StudentNote>();

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<TestStudentAnswer> TestStudentAnswersReviewed { get; set; } = new List<TestStudentAnswer>();

    public virtual ICollection<MailingCampaign> MailingCampaignsCreated { get; set; } = new List<MailingCampaign>();

    public virtual ICollection<SiteSetting> SiteSettingsUpdated { get; set; } = new List<SiteSetting>();

    public virtual User User { get; set; } = null!;
}
