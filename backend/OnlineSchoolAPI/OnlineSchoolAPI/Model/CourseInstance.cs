using System;
using System.Collections.Generic;

namespace OnlineSchoolAPI.Models;

public partial class CourseInstance
{
    public int InstanceId { get; set; }

    public int CourseId { get; set; }

    public string InstanceName { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    /// <summary>FK на course_instance_status.status_id (script5).</summary>
    public int StatusId { get; set; }

    public DateOnly? EnrollmentStartDate { get; set; }

    public DateOnly? EnrollmentEndDate { get; set; }

    public string? Timezone { get; set; }

    public int? TotalWeeks { get; set; }

    public int? LessonsPerWeek { get; set; }

    public string? ScheduleDescription { get; set; }

    public int? MaxStudents { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? CreatedByEmployeeId { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    /// <summary>JSON с правилами расписания (день недели / время), см. script3.</summary>
    public string? ScheduleRulesJson { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual CourseInstanceStatus InstanceStatus { get; set; } = null!;

    public virtual Employee? CreatedByEmployee { get; set; }

    public virtual ICollection<CourseInstanceStaff> CourseInstanceStaff { get; set; } = new List<CourseInstanceStaff>();

    public virtual ICollection<CourseSchedulePlan> CourseSchedulePlans { get; set; } = new List<CourseSchedulePlan>();

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
