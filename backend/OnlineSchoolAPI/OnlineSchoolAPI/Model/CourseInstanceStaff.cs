using System;

namespace OnlineSchoolAPI.Models;

/// <summary>
/// Назначение сотрудника на поток (роль из user_role, напр. 8 — преподаватель, 6 — наставник).
/// </summary>
public partial class CourseInstanceStaff
{
    public int StaffAssignmentId { get; set; }

    public int InstanceId { get; set; }

    public int EmployeeId { get; set; }

    public int RoleId { get; set; }

    public DateTime AssignedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual CourseInstance Instance { get; set; } = null!;

    public virtual Employee Employee { get; set; } = null!;

    public virtual UserRole Role { get; set; } = null!;
}
