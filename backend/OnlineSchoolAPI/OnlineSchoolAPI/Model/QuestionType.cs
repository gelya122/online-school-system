namespace OnlineSchoolAPI.Models;

/// <summary>Справочник типов вопроса ДЗ (таблица question_type).</summary>
public partial class QuestionType
{
    public int QuestionTypeId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<TestQuestion> TestQuestions { get; set; } = new List<TestQuestion>();
}
