namespace OnlineSchoolAPI.Services;

/// <summary>Соответствие seed script4.sql / question_type.</summary>
public static class HomeworkQuestionTypeIds
{
    /// <summary>«Краткий ответ» — авто-проверка по correct_answer.</summary>
    public const int AutoShortAnswer = 3;

    /// <summary>«Развёрнутый ответ» — ручная проверка.</summary>
    public const int ManualLongAnswer = 4;

    public static int FromTaskTypeSlug(string? taskType) =>
        string.Equals(taskType?.Trim(), "detailed_answer", StringComparison.OrdinalIgnoreCase)
            ? ManualLongAnswer
            : AutoShortAnswer;

    /// <summary>
    /// Для API/клиентов: short_answer | detailed_answer.
    /// В script4 часть авто-вопросов помечена question_type_id=4, но с заполненным correct_answer — считаем short_answer.
    /// </summary>
    public static string ToTaskTypeSlug(int questionTypeId, string? correctAnswer)
    {
        if (questionTypeId == AutoShortAnswer) return "short_answer";
        if (questionTypeId == ManualLongAnswer && string.IsNullOrWhiteSpace(correctAnswer))
            return "detailed_answer";
        return "short_answer";
    }
}
