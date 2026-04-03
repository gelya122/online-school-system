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

    public int? TotalWeeks { get; set; }

    public int? LessonsPerWeek { get; set; }

    public string? ScheduleDescription { get; set; }

    public int? MaxStudents { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual ICollection<CourseInstanceCoordinator> CourseInstanceCoordinators { get; set; } = new List<CourseInstanceCoordinator>();

    public virtual ICollection<CourseSchedulePlan> CourseSchedulePlans { get; set; } = new List<CourseSchedulePlan>();

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
