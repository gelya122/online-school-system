using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;
using OnlineSchoolAPI.Services;

namespace OnlineSchoolAPI.Controllers;

/// <summary>
/// Админ-редактор ДЗ: assignment + test_question (поле correct_answer, без test_text_answer) + question_type — см. script4.sql.
/// HomeworkId/TaskId в DTO = assignment_id / question_id.
/// </summary>
[ApiController]
[Authorize]
public sealed class AdminHomeworksController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    /// <summary>Максимум вариантов в correct_answer (через |); id варианта = questionId * 1000 + index.</summary>
    private const int AnswerIdStride = 1000;

    public AdminHomeworksController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    private static readonly HashSet<string> AllowedTaskTypes = ["short_answer", "detailed_answer"];

    private static List<string> SplitCorrectVariants(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return [];
        return raw.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    private static string JoinCorrectVariants(IReadOnlyList<string> parts) =>
        string.Join("|", parts.Select(p => p.Trim()).Where(p => p.Length > 0));

    private static int EncodeAnswerId(int questionId, int index0) => questionId * AnswerIdStride + index0;

    private static bool TryDecodeAnswerId(int answerId, int expectedQuestionId, out int index0)
    {
        index0 = answerId % AnswerIdStride;
        var qid = answerId / AnswerIdStride;
        return qid == expectedQuestionId && index0 >= 0 && index0 < AnswerIdStride - 1;
    }

    private async Task<decimal> SumQuestionMaxPointsAsync(int assignmentId, int? excludeQuestionId, CancellationToken ct)
    {
        var q = _context.TestQuestions.AsNoTracking().Where(t => t.AssignmentId == assignmentId);
        if (excludeQuestionId.HasValue)
            q = q.Where(t => t.QuestionId != excludeQuestionId.Value);
        return await q.SumAsync(t => (decimal?)t.MaxPoints, ct) ?? 0m;
    }

    [HttpGet("api/admin/lessons/{lessonId:int}/homeworks")]
    public async Task<ActionResult<IReadOnlyList<AdminHomeworkRowDto>>> GetHomeworks(int lessonId, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var lessonExists = await _context.Lessons.AnyAsync(l => l.LessonId == lessonId, cancellationToken);
        if (!lessonExists) return NotFound();

        var list = await _context.Assignments
            .AsNoTracking()
            .Where(a => a.LessonId == lessonId)
            .OrderBy(a => a.AssignmentId)
            .Select(a => new AdminHomeworkRowDto
            {
                HomeworkId = a.AssignmentId,
                LessonId = a.LessonId,
                Title = a.Title,
                Description = a.Description,
                AssignmentTypeId = 0,
                MaxScore = a.MaxScore,
                DueDaysAfterLesson = a.DueDaysAfterLesson,
                IsRequired = true,
                HomeworkOrder = a.AssignmentId,
                IsActive = true
            })
            .ToListAsync(cancellationToken);

        return Ok(list);
    }

    [HttpPost("api/admin/lessons/{lessonId:int}/homeworks")]
    public async Task<ActionResult<AdminHomeworkRowDto>> CreateHomework(int lessonId, [FromBody] AdminHomeworkUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var lessonExists = await _context.Lessons.AnyAsync(l => l.LessonId == lessonId, cancellationToken);
        if (!lessonExists) return NotFound();

        if (await _context.Assignments.AnyAsync(a => a.LessonId == lessonId, cancellationToken))
            return BadRequest("К одному уроку можно добавить только одно домашнее задание.");

        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest("Укажите название домашнего задания.");

        if (dto.MaxScore < 0)
            return BadRequest("MaxScore должен быть >= 0.");

        if (dto.DueDaysAfterLesson < 0)
            return BadRequest("DueDaysAfterLesson должен быть >= 0.");

        var now = DateTime.UtcNow;
        var assignment = new Assignment
        {
            LessonId = lessonId,
            Title = dto.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description,
            MaxScore = dto.MaxScore,
            DueDaysAfterLesson = dto.DueDaysAfterLesson,
            CreatedAt = now
        };

        _context.Assignments.Add(assignment);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new AdminHomeworkRowDto
        {
            HomeworkId = assignment.AssignmentId,
            LessonId = assignment.LessonId,
            Title = assignment.Title,
            Description = assignment.Description,
            AssignmentTypeId = 0,
            MaxScore = assignment.MaxScore,
            DueDaysAfterLesson = assignment.DueDaysAfterLesson,
            IsRequired = dto.IsRequired,
            HomeworkOrder = assignment.AssignmentId,
            IsActive = true
        });
    }

    [HttpPut("api/admin/homeworks/{homeworkId:int}")]
    public async Task<IActionResult> UpdateHomework(int homeworkId, [FromBody] AdminHomeworkUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var assignment = await _context.Assignments.FirstOrDefaultAsync(a => a.AssignmentId == homeworkId, cancellationToken);
        if (assignment == null) return NotFound();

        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest("Укажите название домашнего задания.");

        if (dto.MaxScore < 0)
            return BadRequest("MaxScore должен быть >= 0.");

        if (dto.DueDaysAfterLesson < 0)
            return BadRequest("DueDaysAfterLesson должен быть >= 0.");

        var sumQuestions = await SumQuestionMaxPointsAsync(homeworkId, null, cancellationToken);
        if (dto.MaxScore < sumQuestions)
            return BadRequest($"MaxScore ДЗ не может быть меньше суммы max_points по заданиям ({sumQuestions}).");

        assignment.Title = dto.Title.Trim();
        assignment.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description;
        assignment.MaxScore = dto.MaxScore;
        assignment.DueDaysAfterLesson = dto.DueDaysAfterLesson;

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("api/admin/homeworks/{homeworkId:int}")]
    public async Task<IActionResult> DeleteHomework(int homeworkId, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var assignment = await _context.Assignments
            .Include(a => a.TestQuestions)
            .FirstOrDefaultAsync(a => a.AssignmentId == homeworkId, cancellationToken);
        if (assignment == null) return NotFound();

        if (await _context.Submissions.AnyAsync(s => s.AssignmentId == homeworkId, cancellationToken))
            return BadRequest("Есть отправки студентов по этому заданию — удаление отменено.");

        _context.TestQuestions.RemoveRange(assignment.TestQuestions);
        _context.Assignments.Remove(assignment);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("api/admin/homeworks/{homeworkId:int}/tasks")]
    public async Task<ActionResult<IReadOnlyList<AdminHomeworkTaskRowDto>>> GetTasks(int homeworkId, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var exists = await _context.Assignments.AnyAsync(a => a.AssignmentId == homeworkId, cancellationToken);
        if (!exists) return NotFound();

        var list = await _context.TestQuestions
            .AsNoTracking()
            .Where(q => q.AssignmentId == homeworkId)
            .OrderBy(q => q.QuestionOrder)
            .ThenBy(q => q.QuestionId)
            .Select(q => new AdminHomeworkTaskRowDto
            {
                TaskId = q.QuestionId,
                HomeworkId = q.AssignmentId,
                TaskType = HomeworkQuestionTypeIds.ToTaskTypeSlug(q.QuestionTypeId, q.CorrectAnswer),
                Title = null,
                TaskText = q.QuestionText,
                Explanation = q.Explanation,
                MaxScore = (int)Math.Round(q.MaxPoints, MidpointRounding.AwayFromZero),
                TaskOrder = q.QuestionOrder,
                CorrectAnswer = q.CorrectAnswer,
                AllowPartialCredit = false,
                NumericTolerance = null,
                AutoCheckEnabled = true,
                IsActive = true
            })
            .ToListAsync(cancellationToken);

        return Ok(list);
    }

    [HttpPost("api/admin/homeworks/{homeworkId:int}/tasks")]
    public async Task<ActionResult<AdminHomeworkTaskRowDto>> CreateTask(int homeworkId, [FromBody] AdminHomeworkTaskUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var assignment = await _context.Assignments.AsNoTracking()
            .FirstOrDefaultAsync(a => a.AssignmentId == homeworkId, cancellationToken);
        if (assignment == null) return NotFound();

        var taskType = (dto.TaskType ?? "").Trim();
        if (!AllowedTaskTypes.Contains(taskType))
            return BadRequest("Допустимы только типы: short_answer, detailed_answer.");

        if (string.IsNullOrWhiteSpace(dto.TaskText))
            return BadRequest("Укажите текст задания.");

        if (dto.MaxScore < 0)
            return BadRequest("MaxScore должен быть >= 0.");

        if (taskType == "detailed_answer" && dto.MaxScore <= 0)
            return BadRequest("Для detailed_answer укажите max_points > 0.");

        var sum = await SumQuestionMaxPointsAsync(homeworkId, null, cancellationToken);
        if (sum + dto.MaxScore > assignment.MaxScore)
            return BadRequest("Сумма max_points заданий превышает max_score домашнего задания.");

        var typeId = HomeworkQuestionTypeIds.FromTaskTypeSlug(taskType);
        if (!await _context.QuestionTypes.AsNoTracking().AnyAsync(t => t.QuestionTypeId == typeId, cancellationToken))
            return BadRequest("Справочник question_type не содержит нужный тип. Выполните seed script4.sql.");

        var correct = taskType == "short_answer"
            ? (string.IsNullOrWhiteSpace(dto.CorrectAnswer) ? null : dto.CorrectAnswer.Trim())
            : null;

        var q = new TestQuestion
        {
            AssignmentId = homeworkId,
            QuestionText = dto.TaskText,
            QuestionTypeId = typeId,
            MaxPoints = dto.MaxScore,
            QuestionOrder = dto.TaskOrder,
            Explanation = string.IsNullOrWhiteSpace(dto.Explanation) ? null : dto.Explanation,
            CorrectAnswer = correct,
            CreatedAt = DateTime.UtcNow
        };

        _context.TestQuestions.Add(q);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new AdminHomeworkTaskRowDto
        {
            TaskId = q.QuestionId,
            HomeworkId = q.AssignmentId,
            TaskType = HomeworkQuestionTypeIds.ToTaskTypeSlug(q.QuestionTypeId, q.CorrectAnswer),
            Title = null,
            TaskText = q.QuestionText,
            Explanation = q.Explanation,
            MaxScore = (int)Math.Round(q.MaxPoints, MidpointRounding.AwayFromZero),
            TaskOrder = q.QuestionOrder,
            CorrectAnswer = q.CorrectAnswer,
            AllowPartialCredit = false,
            NumericTolerance = null,
            AutoCheckEnabled = true,
            IsActive = true
        });
    }

    [HttpPut("api/admin/homework-tasks/{taskId:int}")]
    public async Task<IActionResult> UpdateTask(int taskId, [FromBody] AdminHomeworkTaskUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var q = await _context.TestQuestions
            .FirstOrDefaultAsync(x => x.QuestionId == taskId, cancellationToken);
        if (q == null) return NotFound();

        var taskType = (dto.TaskType ?? "").Trim();
        if (!AllowedTaskTypes.Contains(taskType))
            return BadRequest("Допустимы только типы: short_answer, detailed_answer.");

        if (string.IsNullOrWhiteSpace(dto.TaskText))
            return BadRequest("Укажите текст задания.");

        if (dto.MaxScore < 0)
            return BadRequest("MaxScore должен быть >= 0.");

        if (taskType == "detailed_answer" && dto.MaxScore <= 0)
            return BadRequest("Для detailed_answer укажите max_points > 0.");

        var assignment = await _context.Assignments.AsNoTracking()
            .FirstOrDefaultAsync(a => a.AssignmentId == q.AssignmentId, cancellationToken);
        if (assignment == null) return NotFound();

        var sumOthers = await SumQuestionMaxPointsAsync(q.AssignmentId, taskId, cancellationToken);
        if (sumOthers + dto.MaxScore > assignment.MaxScore)
            return BadRequest("Сумма max_points заданий превышает max_score домашнего задания.");

        var typeId = HomeworkQuestionTypeIds.FromTaskTypeSlug(taskType);
        if (!await _context.QuestionTypes.AsNoTracking().AnyAsync(t => t.QuestionTypeId == typeId, cancellationToken))
            return BadRequest("Справочник question_type не содержит нужный тип.");

        if (taskType == "short_answer")
        {
            var variants = SplitCorrectVariants(dto.CorrectAnswer ?? q.CorrectAnswer);
            if (variants.Count == 0)
                return BadRequest("Для short_answer укажите correct_answer (один или несколько вариантов через |) или добавьте варианты через API ответов.");
        }

        q.QuestionTypeId = typeId;
        q.QuestionText = dto.TaskText;
        q.Explanation = string.IsNullOrWhiteSpace(dto.Explanation) ? null : dto.Explanation;
        q.MaxPoints = dto.MaxScore;
        q.QuestionOrder = dto.TaskOrder;
        q.CorrectAnswer = taskType == "short_answer"
            ? (string.IsNullOrWhiteSpace(dto.CorrectAnswer) ? q.CorrectAnswer : dto.CorrectAnswer.Trim())
            : null;

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("api/admin/homework-tasks/{taskId:int}")]
    public async Task<IActionResult> DeleteTask(int taskId, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var q = await _context.TestQuestions
            .FirstOrDefaultAsync(x => x.QuestionId == taskId, cancellationToken);
        if (q == null) return NotFound();

        if (await _context.TestStudentAnswers.AnyAsync(a => a.QuestionId == taskId, cancellationToken))
            return BadRequest("Есть ответы студентов на этот вопрос — удаление отменено.");

        _context.TestQuestions.Remove(q);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPatch("api/admin/homework-tasks/reorder")]
    public async Task<IActionResult> ReorderTasks([FromBody] AdminReorderRequestDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        if (dto.Items.Count == 0) return BadRequest("Пустой список.");

        var ids = dto.Items.Select(x => x.Id).ToList();
        var questions = await _context.TestQuestions.Where(t => ids.Contains(t.QuestionId)).ToListAsync(cancellationToken);
        if (questions.Count != ids.Count) return BadRequest("Некоторые задания не найдены.");

        var assignmentIds = questions.Select(t => t.AssignmentId).Distinct().ToList();
        if (assignmentIds.Count != 1)
            return BadRequest("Все задания в одном запросе должны относиться к одному ДЗ.");

        foreach (var item in dto.Items)
        {
            var t = questions.First(x => x.QuestionId == item.Id);
            t.QuestionOrder = item.Order;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("api/admin/homework-tasks/{taskId:int}/answers")]
    public async Task<ActionResult<IReadOnlyList<AdminHomeworkTaskAnswerRowDto>>> GetAnswers(int taskId, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var task = await _context.TestQuestions.AsNoTracking()
            .FirstOrDefaultAsync(t => t.QuestionId == taskId, cancellationToken);
        if (task == null) return NotFound();

        if (HomeworkQuestionTypeIds.ToTaskTypeSlug(task.QuestionTypeId, task.CorrectAnswer) != "short_answer")
            return Ok(Array.Empty<AdminHomeworkTaskAnswerRowDto>());

        var parts = SplitCorrectVariants(task.CorrectAnswer);
        var list = new List<AdminHomeworkTaskAnswerRowDto>();
        for (var i = 0; i < parts.Count; i++)
        {
            list.Add(new AdminHomeworkTaskAnswerRowDto
            {
                AnswerId = EncodeAnswerId(taskId, i),
                TaskId = taskId,
                AnswerText = parts[i],
                AnswerNumber = null,
                IsCorrect = true,
                AnswerOrder = i + 1
            });
        }

        return Ok(list);
    }

    [HttpPost("api/admin/homework-tasks/{taskId:int}/answers")]
    public async Task<ActionResult<AdminHomeworkTaskAnswerRowDto>> CreateAnswer(int taskId, [FromBody] AdminHomeworkTaskAnswerUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var task = await _context.TestQuestions.FirstOrDefaultAsync(t => t.QuestionId == taskId, cancellationToken);
        if (task == null) return NotFound();

        if (HomeworkQuestionTypeIds.ToTaskTypeSlug(task.QuestionTypeId, task.CorrectAnswer) != "short_answer")
            return BadRequest("Варианты ответов допустимы только для short_answer.");

        if (string.IsNullOrWhiteSpace(dto.AnswerText))
            return BadRequest("Укажите текст ответа.");

        var parts = SplitCorrectVariants(task.CorrectAnswer);
        parts.Add(dto.AnswerText.Trim());
        if (parts.Count >= AnswerIdStride - 1)
            return BadRequest("Слишком много вариантов ответа.");

        task.CorrectAnswer = JoinCorrectVariants(parts);
        await _context.SaveChangesAsync(cancellationToken);

        var index = parts.Count - 1;
        return Ok(new AdminHomeworkTaskAnswerRowDto
        {
            AnswerId = EncodeAnswerId(taskId, index),
            TaskId = taskId,
            AnswerText = parts[index],
            AnswerNumber = null,
            IsCorrect = true,
            AnswerOrder = index + 1
        });
    }

    [HttpPut("api/admin/homework-task-answers/{answerId:int}")]
    public async Task<IActionResult> UpdateAnswer(int answerId, [FromBody] AdminHomeworkTaskAnswerUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        if (string.IsNullOrWhiteSpace(dto.AnswerText))
            return BadRequest("Укажите текст ответа.");

        var questionId = answerId / AnswerIdStride;
        var task = await _context.TestQuestions.FirstOrDefaultAsync(t => t.QuestionId == questionId, cancellationToken);
        if (task == null) return NotFound();

        if (!TryDecodeAnswerId(answerId, task.QuestionId, out var index0))
            return BadRequest("Некорректный идентификатор ответа.");

        if (HomeworkQuestionTypeIds.ToTaskTypeSlug(task.QuestionTypeId, task.CorrectAnswer) != "short_answer")
            return BadRequest("Варианты ответов допустимы только для short_answer.");

        var parts = SplitCorrectVariants(task.CorrectAnswer);
        if (index0 < 0 || index0 >= parts.Count)
            return NotFound();

        parts[index0] = dto.AnswerText.Trim();
        task.CorrectAnswer = JoinCorrectVariants(parts);

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("api/admin/homework-task-answers/{answerId:int}")]
    public async Task<IActionResult> DeleteAnswer(int answerId, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        var questionId = answerId / AnswerIdStride;
        var task = await _context.TestQuestions.FirstOrDefaultAsync(t => t.QuestionId == questionId, cancellationToken);
        if (task == null) return NotFound();

        if (!TryDecodeAnswerId(answerId, task.QuestionId, out var index0))
            return BadRequest("Некорректный идентификатор ответа.");

        if (HomeworkQuestionTypeIds.ToTaskTypeSlug(task.QuestionTypeId, task.CorrectAnswer) != "short_answer")
            return NotFound();

        var parts = SplitCorrectVariants(task.CorrectAnswer);
        if (index0 < 0 || index0 >= parts.Count)
            return NotFound();

        if (parts.Count <= 1)
            return BadRequest("Нельзя удалить последний правильный ответ у задания short_answer.");

        parts.RemoveAt(index0);
        task.CorrectAnswer = JoinCorrectVariants(parts);

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
