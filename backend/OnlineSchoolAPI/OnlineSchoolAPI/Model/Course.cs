using System;
using System.Collections.Generic;

namespace OnlineSchoolAPI.Models;

public partial class Course
{
    public int CourseId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? ShortDescription { get; set; }

    public int CategoryId { get; set; }

    public string? CoverImgUrl { get; set; }

    public decimal Price { get; set; }

    public decimal? DiscountPrice { get; set; }

    public int? TotalHours { get; set; }

    public string? WhatYouGet { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual CourseCategory Category { get; set; } = null!;

    public virtual ICollection<CourseInstance> CourseInstances { get; set; } = new List<CourseInstance>();

    public virtual ICollection<CourseModule> CourseModules { get; set; } = new List<CourseModule>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
