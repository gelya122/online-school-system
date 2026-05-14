using System;
using System.Collections.Generic;

namespace OnlineSchoolAPI.Models;

public partial class TestQuestion
{
    public int QuestionId { get; set; }

    public int AssignmentId { get; set; }

    public string QuestionText { get; set; } = null!;

    public int QuestionTypeId { get; set; }

    public decimal MaxPoints { get; set; }

    public int QuestionOrder { get; set; }

    public string? Explanation { get; set; }

    /// <summary>Для авто-проверки: один или несколько вариантов через | (как раньше несколько test_text_answer).</summary>
    public string? CorrectAnswer { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Assignment Assignment { get; set; } = null!;

    public virtual QuestionType QuestionType { get; set; } = null!;

    public virtual ICollection<TestStudentAnswer> TestStudentAnswers { get; set; } = new List<TestStudentAnswer>();
}
