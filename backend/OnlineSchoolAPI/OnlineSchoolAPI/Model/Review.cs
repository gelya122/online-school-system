using System;
using System.Collections.Generic;

namespace OnlineSchoolAPI.Models;

public partial class Review
{
    public int ReviewId { get; set; }

    public int StudentId { get; set; }

    public int CourseId { get; set; }

    public int? Rating { get; set; }

    public string? Comment { get; set; }

    public bool? IsPublished { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
