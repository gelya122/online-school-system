using System;
using System.Collections.Generic;

namespace OnlineSchoolAPI.Models;

public partial class Student
{
    public int StudentId { get; set; }

    public int UserId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? Phone { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? AvatarUrl { get; set; }

    public int ClassNumber { get; set; }

    public string? ParentPhone { get; set; }

    public string? ParentEmail { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<AppOrder> AppOrders { get; set; } = new List<AppOrder>();

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual User User { get; set; } = null!;
}
