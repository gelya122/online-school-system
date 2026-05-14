using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;
using OnlineSchoolAPI.Services;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/students/{studentId:int}/cabinet")]
public class StudentCabinetController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public StudentCabinetController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    private static List<string> SplitCorrectAnswerVariants(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return [];
        return raw.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    [HttpGet("courses")]
    public async Task<ActionResult<IEnumerable<StudentCabinetEnrollmentSummaryDto>>> GetMyCourses(int studentId)
    {
        if (!await _context.Students.AnyAsync(s => s.StudentId == studentId))
            return NotFound();

        var rows = await _context.Enrollments.AsNoTracking()
            .Where(e => e.StudentId == studentId)
            .OrderByDescending(e => e.EnrolledAt)
            .Select(e => new StudentCabinetEnrollmentSummaryDto
            {
                EnrollmentId = e.EnrollmentId,
                EnrolledAt = e.EnrolledAt,
                EnrollmentStatusId = e.EnrollmentStatusId,
                EnrollmentStatusName = e.EnrollmentStatus != null ? e.EnrollmentStatus.StatusName : null,
                Course = new StudentCabinetCourseSummaryDto
                {
                    CourseId = e.Instance.Course.CourseId,
                    Title = e.Instance.Course.Title,
                    Description = e.Instance.Course.Description,
                    ShortDescription = e.Instance.Course.ShortDescription,
                    CoverImgUrl = e.Instance.Course.CoverImgUrl,
                    TotalHours = e.Instance.Course.TotalHours,
                    WhatYouGet = e.Instance.Course.WhatYouGet
                },
                Instance = new StudentCabinetInstanceSummaryDto
                {
                    InstanceId = e.Instance.InstanceId,
                    InstanceName = e.Instance.InstanceName,
                    StartDate = e.Instance.StartDate,
                    EndDate = e.Instance.EndDate,
                    TotalWeeks = e.Instance.TotalWeeks,
                    LessonsPerWeek = e.Instance.LessonsPerWeek,
                    ScheduleDescription = e.Instance.ScheduleDescription
                }
            })
            .ToListAsync();

        return Ok(rows);
    }

    [HttpGet("enrollments/{enrollmentId:int}")]
    public async Task<ActionResult<StudentCabinetEnrollmentDetailDto>> GetEnrollmentCourse(int studentId, int enrollmentId)
    {
        if (!await _context.Students.AnyAsync(s => s.StudentId == studentId))
            return NotFound();

        var enrollment = await _context.Enrollments.AsNoTracking()
            .Include(e => e.Instance).ThenInclude(i => i.Course)
            .Include(e => e.EnrollmentStatus)
            .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId && e.StudentId == studentId);

        if (enrollment == null)
            return NotFound();

        var courseId = enrollment.Instance.CourseId;

        var modules = await _context.CourseModules.AsNoTracking()
            .Where(m => m.CourseId == courseId)
            .OrderBy(m => m.ModuleOrder)
            .Select(m => new StudentCabinetModuleOutlineDto
            {
                ModuleId = m.ModuleId,
                Title = m.Title,
                Description = m.Description,
                ModuleOrder = m.ModuleOrder,
                Lessons = m.Lessons
                    .OrderBy(l => l.LessonOrder)
                    .Select(l => new StudentCabinetLessonOutlineDto
                    {
                        LessonId = l.LessonId,
                        ModuleId = l.ModuleId,
                        Title = l.Title,
                        LessonOrder = l.LessonOrder,
                        LessonTypeId = l.LessonTypeId,
                        DurationMinutes = l.DurationMinutes
                    })
                    .ToList()
            })
            .ToListAsync();

        var dto = new StudentCabinetEnrollmentDetailDto
        {
            EnrollmentId = enrollment.EnrollmentId,
            EnrolledAt = enrollment.EnrolledAt,
            EnrollmentStatusId = enrollment.EnrollmentStatusId,
            EnrollmentStatusName = enrollment.EnrollmentStatus?.StatusName,
            Course = new StudentCabinetCourseSummaryDto
            {
                CourseId = enrollment.Instance.Course.CourseId,
                Title = enrollment.Instance.Course.Title,
                Description = enrollment.Instance.Course.Description,
                ShortDescription = enrollment.Instance.Course.ShortDescription,
                CoverImgUrl = enrollment.Instance.Course.CoverImgUrl,
                TotalHours = enrollment.Instance.Course.TotalHours,
                WhatYouGet = enrollment.Instance.Course.WhatYouGet
            },
            Instance = new StudentCabinetInstanceSummaryDto
            {
                InstanceId = enrollment.Instance.InstanceId,
                InstanceName = enrollment.Instance.InstanceName,
                StartDate = enrollment.Instance.StartDate,
                EndDate = enrollment.Instance.EndDate,
                TotalWeeks = enrollment.Instance.TotalWeeks,
                LessonsPerWeek = enrollment.Instance.LessonsPerWeek,
                ScheduleDescription = enrollment.Instance.ScheduleDescription
            },
            Modules = modules
        };

        return Ok(dto);
    }

    [HttpGet("enrollments/{enrollmentId:int}/lessons/{lessonId:int}")]
    public async Task<ActionResult<StudentCabinetLessonDetailDto>> GetLesson(
        int studentId,
        int enrollmentId,
        int lessonId)
    {
        if (!await _context.Students.AnyAsync(s => s.StudentId == studentId))
            return NotFound();

        var enrollment = await _context.Enrollments.AsNoTracking()
            .Include(e => e.Instance)
            .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId && e.StudentId == studentId);

        if (enrollment == null)
            return NotFound();

        var courseId = enrollment.Instance.CourseId;

        var lesson = await _context.Lessons.AsNoTracking()
            .Include(l => l.Module)
            .Include(l => l.LessonType)
            .FirstOrDefaultAsync(l => l.LessonId == lessonId && l.Module.CourseId == courseId);

        if (lesson == null)
            return NotFound();

        var plan = await _context.CourseSchedulePlans.AsNoTracking()
            .FirstOrDefaultAsync(p => p.InstanceId == enrollment.InstanceId && p.LessonId == lessonId);

        var progress = await _context.StudentProgresses.AsNoTracking()
            .FirstOrDefaultAsync(p => p.EnrollmentId == enrollmentId && p.LessonId == lessonId);

        var materials = await _context.LessonMaterials.AsNoTracking()
            .Where(m => m.LessonId == lessonId)
            .OrderBy(m => m.MaterialId)
            .Select(m => new StudentCabinetLessonMaterialDto
            {
                MaterialId = m.MaterialId,
                FileName = m.FileName,
                FileUrl = m.FileUrl,
                FileType = m.FileType,
                FileSizeKb = m.FileSizeKb,
                DownloadCount = m.DownloadCount,
                UploadedAt = m.UploadedAt
            })
            .ToListAsync();

        var assignments = await _context.Assignments.AsNoTracking()
            .Where(a => a.LessonId == lessonId)
            .OrderBy(a => a.AssignmentId)
            .Select(a => new
            {
                a.AssignmentId,
                a.Title,
                a.Description,
                a.MaxScore,
                a.DueDaysAfterLesson,
                a.CreatedAt
            })
            .ToListAsync();

        DateOnly? accessDate = plan != null
            ? (plan.ScheduledAt.HasValue
                ? DateOnly.FromDateTime(plan.ScheduledAt.Value.Date)
                : enrollment.Instance.StartDate.AddDays(plan.ReleaseDayOffset))
            : (progress != null ? enrollment.Instance.StartDate : (DateOnly?)null);
        var assignmentDtos = assignments.Select(a => new StudentCabinetAssignmentDto
        {
            AssignmentId = a.AssignmentId,
            Title = a.Title,
            Description = a.Description,
            AssignmentTypeId = 0,
            AssignmentTypeName = null,
            MaxScore = a.MaxScore,
            DueDaysAfterLesson = a.DueDaysAfterLesson,
            CalculatedDueDate = accessDate.HasValue && a.DueDaysAfterLesson.HasValue
                ? accessDate.Value.AddDays(a.DueDaysAfterLesson.Value)
                : null,
            CreatedAt = a.CreatedAt
        }).ToList();

        var assignmentIdsInLesson = assignments.Select(a => a.AssignmentId).ToList();
        var submissions = new List<StudentCabinetSubmissionDto>();
        if (assignmentIdsInLesson.Count > 0)
        {
            var rawSubs = await _context.Submissions.AsNoTracking()
                .Where(s => s.EnrollmentId == enrollmentId && assignmentIdsInLesson.Contains(s.AssignmentId))
                .OrderByDescending(s => s.SubmittedAt)
                .Select(s => new StudentCabinetSubmissionDto
                {
                    SubmissionId = s.SubmissionId,
                    AssignmentId = s.AssignmentId,
                    StudentAnswerText = _context.TestStudentAnswers
                        .Where(a => a.SubmissionId == s.SubmissionId)
                        .OrderBy(a => a.QuestionId)
                        .Select(a => a.ResponseText)
                        .FirstOrDefault(),
                    SubmittedAt = s.SubmittedAt,
                    Score = s.Score,
                    SubmissionStatusName = s.SubmissionStatus != null ? s.SubmissionStatus.StatusName : null,
                    TeacherComment = s.TeacherComment
                })
                .ToListAsync();
            submissions.AddRange(rawSubs);
        }

        var dto = new StudentCabinetLessonDetailDto
        {
            LessonId = lesson.LessonId,
            ModuleId = lesson.ModuleId,
            ModuleTitle = lesson.Module.Title,
            Title = lesson.Title,
            LessonTypeId = lesson.LessonTypeId,
            LessonTypeName = lesson.LessonType?.TypeName,
            Content = lesson.Content,
            VideoUrl = lesson.VideoUrl,
            DurationMinutes = lesson.DurationMinutes,
            LessonOrder = lesson.LessonOrder,
            CreatedAt = lesson.CreatedAt,
            Access = plan == null && progress == null
                ? null
                : new StudentCabinetLessonAccessDto
                {
                    AccessId = plan?.PlanId ?? 0,
                    PlannedAccessDate = accessDate ?? enrollment.Instance.StartDate,
                    PlannedAccessTime = plan?.ReleaseTime?.ToString("HH:mm"),
                    ActualOpenDatetime = plan?.ScheduledAt ?? progress?.LastAccessed,
                    IsAvailable = progress != null
                        || (plan != null && plan.IsPublished && CabinetScheduleReleased(plan, enrollment.Instance.StartDate))
                },
            Progress = progress == null
                ? null
                : new StudentCabinetLessonProgressDto
                {
                    ProgressId = progress.ProgressId,
                    IsCompleted = progress.IsCompleted,
                    CompletedAt = progress.CompletedAt,
                    WatchTimeSeconds = progress.WatchTimeSeconds,
                    LastAccessed = progress.LastAccessed
                },
            Materials = materials,
            Assignments = assignmentDtos,
            Submissions = submissions
        };

        return Ok(dto);
    }

    [HttpGet("homework")]
    public async Task<ActionResult<IEnumerable<StudentCabinetHomeworkRowDto>>> GetHomework(int studentId)
    {
        if (!await _context.Students.AnyAsync(s => s.StudentId == studentId))
            return NotFound();

        var baseRows = await (
            from e in _context.Enrollments.AsNoTracking()
            where e.StudentId == studentId
            join ci in _context.CourseInstances on e.InstanceId equals ci.InstanceId
            join c in _context.Courses on ci.CourseId equals c.CourseId
            join sp in _context.StudentProgresses on e.EnrollmentId equals sp.EnrollmentId
            join l in _context.Lessons on sp.LessonId equals l.LessonId
            join mod in _context.CourseModules on l.ModuleId equals mod.ModuleId
            where mod.CourseId == c.CourseId
            join a in _context.Assignments on l.LessonId equals a.LessonId
            join pl in _context.CourseSchedulePlans on new { InstanceId = ci.InstanceId, LessonId = l.LessonId } equals new { pl.InstanceId, pl.LessonId } into plj
            from pl in plj.DefaultIfEmpty()
            select new
            {
                e.EnrollmentId,
                LessonId = l.LessonId,
                CourseTitle = c.Title,
                ModuleTitle = mod.Title,
                LessonTitle = l.Title,
                a.AssignmentId,
                a.Title,
                a.Description,
                a.MaxScore,
                a.DueDaysAfterLesson,
                PlannedAccessDate = pl != null
                    ? (pl.ScheduledAt.HasValue
                        ? DateOnly.FromDateTime(pl.ScheduledAt.Value.Date)
                        : ci.StartDate.AddDays(pl.ReleaseDayOffset))
                    : ci.StartDate
            })
            .ToListAsync();

        var enrollmentIds = baseRows.Select(r => r.EnrollmentId).Distinct().ToList();
        var submissions = await _context.Submissions.AsNoTracking()
            .Where(s => s.EnrollmentId != null && enrollmentIds.Contains(s.EnrollmentId.Value))
            .Select(s => new
            {
                EnrollmentId = s.EnrollmentId!.Value,
                s.AssignmentId,
                s.SubmittedAt,
                s.Score,
                StatusName = s.SubmissionStatus != null ? s.SubmissionStatus.StatusName : null
            })
            .ToListAsync();

        var latestByKey = submissions
            .GroupBy(s => new { s.EnrollmentId, s.AssignmentId })
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(x => x.SubmittedAt ?? DateTime.MinValue).First());

        var result = baseRows.Select(r =>
        {
            StudentCabinetSubmissionDto? sub = null;
            if (latestByKey.TryGetValue(new { EnrollmentId = r.EnrollmentId, r.AssignmentId }, out var found))
            {
                sub = new StudentCabinetSubmissionDto
                {
                    SubmissionId = 0,
                    AssignmentId = r.AssignmentId,
                    StudentAnswerText = null,
                    SubmittedAt = found.SubmittedAt,
                    Score = found.Score,
                    SubmissionStatusName = found.StatusName,
                    TeacherComment = null
                };
            }

            DateOnly? calcDue = r.DueDaysAfterLesson.HasValue
                ? r.PlannedAccessDate.AddDays(r.DueDaysAfterLesson.Value)
                : null;

            return new StudentCabinetHomeworkRowDto
            {
                EnrollmentId = r.EnrollmentId,
                LessonId = r.LessonId,
                CourseTitle = r.CourseTitle,
                ModuleTitle = r.ModuleTitle,
                LessonTitle = r.LessonTitle,
                AssignmentId = r.AssignmentId,
                AssignmentTitle = r.Title,
                AssignmentDescription = r.Description,
                AssignmentTypeId = 0,
                AssignmentTypeName = (string?)null,
                MaxScore = r.MaxScore,
                DueDaysAfterLesson = r.DueDaysAfterLesson,
                PlannedLessonAccessDate = r.PlannedAccessDate,
                CalculatedDueDate = calcDue,
                SubmittedAt = sub?.SubmittedAt,
                SubmissionScore = sub?.Score,
                SubmissionStatusName = sub?.SubmissionStatusName
            };
        }).ToList();

        return Ok(result.OrderBy(x => x.CourseTitle).ThenBy(x => x.ModuleTitle).ThenBy(x => x.LessonTitle));
    }

    [HttpPost("enrollments/{enrollmentId:int}/lessons/{lessonId:int}/assignments/{assignmentId:int}/submit")]
    public async Task<ActionResult<StudentCabinetSubmissionDto>> SubmitAssignment(
        int studentId,
        int enrollmentId,
        int lessonId,
        int assignmentId,
        SubmitCabinetAssignmentDto dto)
    {
        if (!await _context.Students.AnyAsync(s => s.StudentId == studentId))
            return NotFound();

        var enrollment = await _context.Enrollments.AsNoTracking()
            .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId && e.StudentId == studentId);
        if (enrollment == null)
            return NotFound();

        var assignment = await _context.Assignments.AsNoTracking()
            .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId && a.LessonId == lessonId);
        if (assignment == null)
            return NotFound();

        var progress = await _context.StudentProgresses
            .FirstOrDefaultAsync(p => p.EnrollmentId == enrollmentId && p.LessonId == lessonId);
        if (progress == null)
        {
            progress = new StudentProgress
            {
                EnrollmentId = enrollmentId,
                LessonId = lessonId,
                IsCompleted = false,
                LastAccessed = DateTime.UtcNow
            };
            _context.StudentProgresses.Add(progress);
            await _context.SaveChangesAsync();
        }

        var submittedStatus = await _context.SubmissionStatuses.AsNoTracking()
            .Where(s => s.StatusName != null && s.StatusName.ToLower().Contains("отправ"))
            .OrderBy(s => s.StatusId)
            .Select(s => (int?)s.StatusId)
            .FirstOrDefaultAsync();

        // В БД submission.submission_status_id = NOT NULL. Если справочник статусов пустой/не содержит "отправ",
        // создаём дефолтный статус или берём первый доступный.
        if (!submittedStatus.HasValue)
        {
            var any = await _context.SubmissionStatuses.AsNoTracking()
                .OrderBy(s => s.StatusId)
                .Select(s => (int?)s.StatusId)
                .FirstOrDefaultAsync();

            if (any.HasValue)
            {
                submittedStatus = any.Value;
            }
            else
            {
                var st = new SubmissionStatus
                {
                    StatusName = "Отправлено",
                    Description = "Статус по умолчанию для отправленного ответа."
                };
                _context.SubmissionStatuses.Add(st);
                await _context.SaveChangesAsync();
                submittedStatus = st.StatusId;
            }
        }

        var answer = dto.AnswerText?.Trim();
        if (string.IsNullOrWhiteSpace(answer))
            return BadRequest("Введите ответ перед отправкой.");

        var firstQ = await _context.TestQuestions.AsNoTracking()
            .Where(q => q.AssignmentId == assignmentId)
            .OrderBy(q => q.QuestionOrder)
            .ThenBy(q => q.QuestionId)
            .FirstOrDefaultAsync();
        if (firstQ == null)
            return BadRequest("Домашнее задание не содержит вопросов.");

        var submission = await _context.Submissions
            .Where(s => s.EnrollmentId == enrollmentId && s.AssignmentId == assignmentId)
            .OrderByDescending(s => s.SubmittedAt ?? s.CreatedAt ?? DateTime.MinValue)
            .FirstOrDefaultAsync();
        if (submission == null)
        {
            submission = new Submission
            {
                EnrollmentId = enrollmentId,
                AssignmentId = assignmentId
            };
            _context.Submissions.Add(submission);
            await _context.SaveChangesAsync();
        }

        var slug = HomeworkQuestionTypeIds.ToTaskTypeSlug(firstQ.QuestionTypeId, firstQ.CorrectAnswer);
        var correctAnswers = SplitCorrectAnswerVariants(firstQ.CorrectAnswer);
        decimal? pointsAwarded = null;
        var isFullyAuto = false;
        if (string.Equals(slug, "short_answer", StringComparison.OrdinalIgnoreCase) && correctAnswers.Count > 0)
        {
            var ok = correctAnswers.Any(v => string.Equals(v, answer, StringComparison.OrdinalIgnoreCase));
            pointsAwarded = ok ? firstQ.MaxPoints : 0m;
            isFullyAuto = true;
        }

        var tsa = await _context.TestStudentAnswers
            .FirstOrDefaultAsync(a => a.SubmissionId == submission.SubmissionId && a.QuestionId == firstQ.QuestionId);
        if (tsa == null)
        {
            tsa = new TestStudentAnswer
            {
                SubmissionId = submission.SubmissionId,
                QuestionId = firstQ.QuestionId
            };
            _context.TestStudentAnswers.Add(tsa);
        }

        tsa.ResponseText = answer;
        tsa.PointsAwarded = pointsAwarded;
        tsa.IsFullyAutoGraded = isFullyAuto;
        tsa.AnsweredAt = DateTime.UtcNow;

        submission.SubmissionStatusId = submittedStatus.Value;
        submission.SubmittedAt = DateTime.UtcNow;
        var totalScore = await _context.TestStudentAnswers
            .Where(a => a.SubmissionId == submission.SubmissionId)
            .SumAsync(a => a.PointsAwarded ?? 0m);
        submission.Score = (int)Math.Round(totalScore, MidpointRounding.AwayFromZero);
        await _context.SaveChangesAsync();

        var statusName = submittedStatus.HasValue
            ? await _context.SubmissionStatuses.AsNoTracking()
                .Where(s => s.StatusId == submittedStatus.Value)
                .Select(s => s.StatusName)
                .FirstOrDefaultAsync()
            : null;

        return Ok(new StudentCabinetSubmissionDto
        {
            SubmissionId = submission.SubmissionId,
            AssignmentId = submission.AssignmentId,
            StudentAnswerText = answer,
            SubmittedAt = submission.SubmittedAt,
            Score = submission.Score,
            SubmissionStatusName = statusName,
            TeacherComment = submission.TeacherComment
        });
    }

    [HttpGet("enrollments/{enrollmentId:int}/lessons/{lessonId:int}/assignments/{assignmentId:int}/questions")]
    public async Task<ActionResult<IEnumerable<StudentCabinetQuestionDto>>> GetAssignmentQuestions(
        int studentId,
        int enrollmentId,
        int lessonId,
        int assignmentId)
    {
        if (!await _context.Students.AnyAsync(s => s.StudentId == studentId))
            return NotFound();

        var enrollment = await _context.Enrollments.AsNoTracking()
            .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId && e.StudentId == studentId);
        if (enrollment == null)
            return NotFound();

        var assignment = await _context.Assignments.AsNoTracking()
            .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId && a.LessonId == lessonId);
        if (assignment == null)
            return NotFound();

        try
        {
            _ = await EnsureProgressAsync(enrollmentId, lessonId);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        var submissionId = await EnsureSubmissionAsync(enrollmentId, assignmentId);

        var questions = await _context.TestQuestions.AsNoTracking()
            .Where(q => q.AssignmentId == assignmentId)
            .OrderBy(q => q.QuestionOrder)
            .ThenBy(q => q.QuestionId)
            .ToListAsync();

        var qIds = questions.Select(q => q.QuestionId).ToList();
        var answers = qIds.Count == 0
            ? new List<TestStudentAnswer>()
            : await _context.TestStudentAnswers.AsNoTracking()
                .Where(a => a.SubmissionId == submissionId && qIds.Contains(a.QuestionId))
                .ToListAsync();
        var byQuestion = answers.ToDictionary(a => a.QuestionId);

        return Ok(questions.Select(q =>
        {
            byQuestion.TryGetValue(q.QuestionId, out var sa);
            return new StudentCabinetQuestionDto
            {
                QuestionId = q.QuestionId,
                QuestionOrder = q.QuestionOrder,
                QuestionText = q.QuestionText,
                QuestionType = HomeworkQuestionTypeIds.ToTaskTypeSlug(q.QuestionTypeId, q.CorrectAnswer),
                MaxPoints = q.MaxPoints,
                CorrectAnswer = q.CorrectAnswer,
                StudentAnswer = sa?.ResponseText,
                PointsAwarded = sa?.PointsAwarded
            };
        }).ToList());
    }

    [HttpPost("enrollments/{enrollmentId:int}/lessons/{lessonId:int}/assignments/{assignmentId:int}/questions/{questionId:int}/answer")]
    public async Task<ActionResult<StudentCabinetQuestionDto>> SubmitQuestionAnswer(
        int studentId,
        int enrollmentId,
        int lessonId,
        int assignmentId,
        int questionId,
        SubmitCabinetQuestionAnswerDto dto)
    {
        if (!await _context.Students.AnyAsync(s => s.StudentId == studentId))
            return NotFound();

        var enrollment = await _context.Enrollments.AsNoTracking()
            .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId && e.StudentId == studentId);
        if (enrollment == null)
            return NotFound();

        var assignment = await _context.Assignments.AsNoTracking()
            .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId && a.LessonId == lessonId);
        if (assignment == null)
            return NotFound();

        var rawAnswer = dto.AnswerText?.Trim();
        if (string.IsNullOrWhiteSpace(rawAnswer))
            return BadRequest("Введите ответ перед отправкой.");
        var answer = rawAnswer.Length > 100 ? rawAnswer[..100] : rawAnswer;

        try
        {
            _ = await EnsureProgressAsync(enrollmentId, lessonId);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        var submissionId = await EnsureSubmissionAsync(enrollmentId, assignmentId);

        var question = await _context.TestQuestions.AsNoTracking()
            .FirstOrDefaultAsync(q => q.QuestionId == questionId && q.AssignmentId == assignmentId);
        if (question == null)
            return NotFound();

        var slug = HomeworkQuestionTypeIds.ToTaskTypeSlug(question.QuestionTypeId, question.CorrectAnswer);
        var correctAnswers = SplitCorrectAnswerVariants(question.CorrectAnswer);

        decimal? pointsAwarded = null;
        var isFullyAuto = false;
        if (string.Equals(slug, "short_answer", StringComparison.OrdinalIgnoreCase) && correctAnswers.Count > 0)
        {
            var isCorrect = correctAnswers.Any(a => string.Equals(a, answer, StringComparison.OrdinalIgnoreCase));
            pointsAwarded = isCorrect ? question.MaxPoints : 0m;
            isFullyAuto = true;
        }

        var existing = await _context.TestStudentAnswers
            .FirstOrDefaultAsync(a => a.SubmissionId == submissionId && a.QuestionId == questionId);
        if (existing == null)
        {
            existing = new TestStudentAnswer
            {
                SubmissionId = submissionId,
                QuestionId = questionId
            };
            _context.TestStudentAnswers.Add(existing);
        }

        existing.ResponseText = answer;
        existing.PointsAwarded = pointsAwarded;
        existing.IsFullyAutoGraded = isFullyAuto;
        existing.AnsweredAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var totalScore = await _context.TestStudentAnswers.AsNoTracking()
            .Where(a => a.SubmissionId == submissionId)
            .SumAsync(a => a.PointsAwarded ?? 0m);

        var submittedStatus = await _context.SubmissionStatuses.AsNoTracking()
            .Where(s => s.StatusName != null && s.StatusName.ToLower().Contains("отправ"))
            .OrderBy(s => s.StatusId)
            .Select(s => (int?)s.StatusId)
            .FirstOrDefaultAsync();

        var submission = await _context.Submissions.FirstAsync(s => s.SubmissionId == submissionId);
        submission.Score = (int)Math.Round(totalScore, MidpointRounding.AwayFromZero);
        if (!submittedStatus.HasValue)
        {
            var any = await _context.SubmissionStatuses.AsNoTracking()
                .OrderBy(s => s.StatusId)
                .Select(s => (int?)s.StatusId)
                .FirstOrDefaultAsync();

            if (any.HasValue)
            {
                submittedStatus = any.Value;
            }
            else
            {
                var st = new SubmissionStatus
                {
                    StatusName = "Отправлено",
                    Description = "Статус по умолчанию для отправленного ответа."
                };
                _context.SubmissionStatuses.Add(st);
                await _context.SaveChangesAsync();
                submittedStatus = st.StatusId;
            }
        }

        submission.SubmissionStatusId = submittedStatus.Value;
        submission.SubmittedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new StudentCabinetQuestionDto
        {
            QuestionId = questionId,
            QuestionOrder = question.QuestionOrder,
            QuestionText = question.QuestionText,
            QuestionType = slug,
            MaxPoints = question.MaxPoints,
            CorrectAnswer = correctAnswers.FirstOrDefault(),
            StudentAnswer = answer,
            PointsAwarded = pointsAwarded
        });
    }

    [HttpGet("enrollments/{enrollmentId:int}/lessons/{lessonId:int}/assignments/{assignmentId:int}/result")]
    public async Task<ActionResult<StudentCabinetAssignmentResultDto>> GetAssignmentResult(
        int studentId,
        int enrollmentId,
        int lessonId,
        int assignmentId)
    {
        if (!await _context.Students.AnyAsync(s => s.StudentId == studentId))
            return NotFound();

        var enrollment = await _context.Enrollments.AsNoTracking()
            .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId && e.StudentId == studentId);
        if (enrollment == null)
            return NotFound();

        var assignment = await _context.Assignments.AsNoTracking()
            .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId && a.LessonId == lessonId);
        if (assignment == null)
            return NotFound();

        try
        {
            _ = await EnsureProgressAsync(enrollmentId, lessonId);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        var submissionId = await EnsureSubmissionAsync(enrollmentId, assignmentId);

        var total = await _context.TestStudentAnswers.AsNoTracking()
            .Where(a => a.SubmissionId == submissionId)
            .SumAsync(a => a.PointsAwarded ?? 0m);

        var max = await _context.TestQuestions.AsNoTracking()
            .Where(q => q.AssignmentId == assignmentId)
            .SumAsync(q => q.MaxPoints);

        return Ok(new StudentCabinetAssignmentResultDto
        {
            AssignmentId = assignmentId,
            TotalScore = total,
            MaxScore = max
        });
    }

    private async Task<int> EnsureProgressAsync(int enrollmentId, int lessonId)
    {
        var progress = await _context.StudentProgresses
            .FirstOrDefaultAsync(p => p.EnrollmentId == enrollmentId && p.LessonId == lessonId);
        if (progress == null)
        {
            progress = new StudentProgress
            {
                EnrollmentId = enrollmentId,
                LessonId = lessonId,
                IsCompleted = false,
                LastAccessed = DateTime.UtcNow
            };
            _context.StudentProgresses.Add(progress);
            await _context.SaveChangesAsync();
        }

        return progress.ProgressId;
    }

    private async Task<int> EnsureSubmissionAsync(int enrollmentId, int assignmentId)
    {
        var submission = await _context.Submissions
            .Where(s => s.EnrollmentId == enrollmentId && s.AssignmentId == assignmentId)
            .OrderByDescending(s => s.SubmittedAt ?? s.CreatedAt ?? DateTime.MinValue)
            .FirstOrDefaultAsync();
        if (submission != null)
            return submission.SubmissionId;

        var initialStatusId = await _context.SubmissionStatuses.AsNoTracking()
            .OrderBy(s => s.StatusId)
            .Select(s => (int?)s.StatusId)
            .FirstOrDefaultAsync();

        if (!initialStatusId.HasValue)
        {
            var st = new SubmissionStatus
            {
                StatusName = "Черновик",
                Description = "Статус по умолчанию при создании submission."
            };
            _context.SubmissionStatuses.Add(st);
            await _context.SaveChangesAsync();
            initialStatusId = st.StatusId;
        }

        var created = new Submission
        {
            EnrollmentId = enrollmentId,
            AssignmentId = assignmentId,
            SubmissionStatusId = initialStatusId.Value
        };
        _context.Submissions.Add(created);
        await _context.SaveChangesAsync();
        return created.SubmissionId;
    }

    [HttpGet("progress")]
    public async Task<ActionResult<IEnumerable<StudentCabinetProgressRowDto>>> GetProgress(int studentId)
    {
        if (!await _context.Students.AnyAsync(s => s.StudentId == studentId))
            return NotFound();

        var rows = await (
            from e in _context.Enrollments.AsNoTracking()
            where e.StudentId == studentId
            join ci in _context.CourseInstances on e.InstanceId equals ci.InstanceId
            join c in _context.Courses on ci.CourseId equals c.CourseId
            join sp in _context.StudentProgresses on e.EnrollmentId equals sp.EnrollmentId
            join l in _context.Lessons on sp.LessonId equals l.LessonId
            join mod in _context.CourseModules on l.ModuleId equals mod.ModuleId
            where mod.CourseId == c.CourseId
            orderby c.Title, mod.ModuleOrder, l.LessonOrder
            select new StudentCabinetProgressRowDto
            {
                EnrollmentId = e.EnrollmentId,
                LessonId = l.LessonId,
                CourseTitle = c.Title,
                ModuleTitle = mod.Title,
                ModuleOrder = mod.ModuleOrder,
                LessonTitle = l.Title,
                LessonOrder = l.LessonOrder,
                IsCompleted = sp.IsCompleted,
                CompletedAt = sp.CompletedAt,
                WatchTimeSeconds = sp.WatchTimeSeconds,
                LastAccessed = sp.LastAccessed
            })
            .ToListAsync();

        return Ok(rows);
    }

    [HttpGet("progress/dashboard")]
    public async Task<ActionResult<List<StudentCabinetProgressDashboardDto>>> GetProgressDashboard(int studentId)
    {
        if (!await _context.Students.AnyAsync(s => s.StudentId == studentId))
            return NotFound();

        var myEnrollments = await _context.Enrollments.AsNoTracking()
            .Include(e => e.Instance).ThenInclude(i => i.Course)
            .Where(e => e.StudentId == studentId)
            .ToListAsync();

        if (myEnrollments.Count == 0)
            return Ok(new List<StudentCabinetProgressDashboardDto>());

        var instanceIds = myEnrollments.Select(e => e.InstanceId).Distinct().ToList();
        var courseIds = myEnrollments.Select(e => e.Instance.CourseId).Distinct().ToList();

        var cohortRows = await _context.Enrollments.AsNoTracking()
            .Where(e => instanceIds.Contains(e.InstanceId))
            .Select(e => new { e.InstanceId, e.EnrollmentId })
            .ToListAsync();

        var cohortByInstance = cohortRows
            .GroupBy(x => x.InstanceId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.EnrollmentId).Distinct().ToList());

        var lessonCounts = await (
            from l in _context.Lessons.AsNoTracking()
            join mod in _context.CourseModules.AsNoTracking() on l.ModuleId equals mod.ModuleId
            where courseIds.Contains(mod.CourseId)
            group l by mod.CourseId into g
            select new { CourseId = g.Key, Count = g.Count() }
        ).ToDictionaryAsync(x => x.CourseId, x => x.Count);

        var cohortEnrollmentIds = cohortRows.Select(x => x.EnrollmentId).Distinct().ToList();

        var progressFlatRaw = await (
            from sp in _context.StudentProgresses.AsNoTracking()
            join l in _context.Lessons.AsNoTracking() on sp.LessonId equals l.LessonId
            join mod in _context.CourseModules.AsNoTracking() on l.ModuleId equals mod.ModuleId
            where cohortEnrollmentIds.Contains(sp.EnrollmentId) && courseIds.Contains(mod.CourseId)
            select new
            {
                sp.EnrollmentId,
                mod.CourseId,
                sp.IsCompleted,
                sp.CompletedAt,
                sp.WatchTimeSeconds,
            }
        ).ToListAsync();

        var progressFlat = progressFlatRaw
            .Select(x => new ProgressFlatRow(x.EnrollmentId, x.CourseId, x.IsCompleted, x.CompletedAt, x.WatchTimeSeconds))
            .ToList();

        var subFlatRaw = await (
            from s in _context.Submissions.AsNoTracking()
            where s.EnrollmentId != null && cohortEnrollmentIds.Contains(s.EnrollmentId.Value)
            join e in _context.Enrollments.AsNoTracking() on s.EnrollmentId equals e.EnrollmentId
            join a in _context.Assignments.AsNoTracking() on s.AssignmentId equals a.AssignmentId
            join l in _context.Lessons.AsNoTracking() on a.LessonId equals l.LessonId
            join mod in _context.CourseModules.AsNoTracking() on l.ModuleId equals mod.ModuleId
            where courseIds.Contains(mod.CourseId)
            select new
            {
                e.EnrollmentId,
                mod.CourseId,
                s.AssignmentId,
                s.SubmittedAt,
                s.Score,
                a.MaxScore,
            }
        ).ToListAsync();

        var subFlat = subFlatRaw
            .Select(x => new SubFlatRow(x.EnrollmentId, x.CourseId, x.AssignmentId, x.SubmittedAt, x.Score, x.MaxScore))
            .ToList();

        var latestSubs = subFlat
            .GroupBy(x => new { x.EnrollmentId, x.CourseId, x.AssignmentId })
            .Select(g => g.OrderByDescending(x => x.SubmittedAt ?? DateTime.MinValue).First())
            .ToList();

        var result = new List<StudentCabinetProgressDashboardDto>();

        foreach (var en in myEnrollments)
        {
            var courseId = en.Instance.CourseId;
            var instanceId = en.InstanceId;
            var enrollmentId = en.EnrollmentId;
            lessonCounts.TryGetValue(courseId, out var totalLessons);

            var myProg = progressFlat.Where(p => p.EnrollmentId == enrollmentId && p.CourseId == courseId).ToList();
            var completedLessons = myProg.Count(p => p.IsCompleted == true);
            var lessonPct = totalLessons > 0 ? Math.Round((decimal)completedLessons * 100m / totalLessons, 1) : 0m;
            var studySeconds = myProg.Sum(p => p.WatchTimeSeconds ?? 0);

            var myLatest = latestSubs.Where(s => s.EnrollmentId == enrollmentId && s.CourseId == courseId).ToList();
            var graded = myLatest.Where(s => s.Score.HasValue && s.MaxScore > 0).ToList();
            decimal? avgAssignPct = graded.Count == 0
                ? null
                : Math.Round(graded.Average(s => (decimal)s.Score!.Value * 100m / s.MaxScore), 1);
            var submittedCount = myLatest.Count(s => s.SubmittedAt.HasValue || s.Score.HasValue);
            decimal successPct = 0;
            if (graded.Count > 0)
            {
                var ok = graded.Count(s => (decimal)s.Score!.Value / s.MaxScore > 0.5m);
                successPct = Math.Round((decimal)ok * 100m / graded.Count, 1);
            }

            cohortByInstance.TryGetValue(instanceId, out var cohortIds);
            cohortIds ??= new List<int>();

            var cohortLessonPcts = new List<decimal>();
            var cohortAssignPcts = new List<decimal>();
            var cohortStudyHours = new List<decimal>();
            foreach (var eid in cohortIds)
            {
                var pRows = progressFlat.Where(p => p.EnrollmentId == eid && p.CourseId == courseId).ToList();
                var done = pRows.Count(x => x.IsCompleted == true);
                cohortLessonPcts.Add(totalLessons > 0 ? (decimal)done * 100m / totalLessons : 0m);
                cohortStudyHours.Add(Math.Round(pRows.Sum(x => x.WatchTimeSeconds ?? 0) / 3600m, 2));

                var subs = latestSubs.Where(s => s.EnrollmentId == eid && s.CourseId == courseId).ToList();
                var g = subs.Where(s => s.Score.HasValue && s.MaxScore > 0).ToList();
                if (g.Count > 0)
                    cohortAssignPcts.Add(Math.Round(g.Average(s => (decimal)s.Score!.Value * 100m / s.MaxScore), 1));
            }

            var cohortDto = new StudentCabinetProgressCohortDto
            {
                GroupSize = cohortIds.Count,
                GroupAvgLessonPercent = cohortLessonPcts.Count == 0 ? 0 : Math.Round(cohortLessonPcts.Average(), 1),
                GroupAvgAssignmentPercent = cohortAssignPcts.Count == 0 ? 0 : Math.Round(cohortAssignPcts.Average(), 1),
                GroupAvgStudyHours = cohortStudyHours.Count == 0 ? 0 : Math.Round(cohortStudyHours.Average(), 2),
            };

            var start = en.Instance.StartDate;
            var weekCount = ResolveWeekCount(start, en.Instance.EndDate, en.Instance.TotalWeeks);

            var weeklyLesson = new List<StudentCabinetProgressWeekChartPointDto>();
            var weeklyPerf = new List<StudentCabinetProgressWeekChartPointDto>();

            for (var w = 1; w <= weekCount; w++)
            {
                var weekEnd = start.AddDays(w * 7);
                var stLesson = LessonCumulativePercent(myProg, totalLessons, weekEnd);
                var grpLesson = cohortIds.Count == 0
                    ? 0m
                    : Math.Round(cohortIds.Average(eid =>
                        LessonCumulativePercent(
                            progressFlat.Where(p => p.EnrollmentId == eid && p.CourseId == courseId).ToList(),
                            totalLessons,
                            weekEnd)), 1);

                weeklyLesson.Add(new StudentCabinetProgressWeekChartPointDto
                {
                    WeekNumber = w,
                    Label = $"Неделя {w}",
                    StudentPercent = stLesson,
                    GroupAveragePercent = grpLesson,
                });

                var stPerf = WeeklyPerfAverage(myLatest, start, w);
                var grpPerf = cohortIds.Count == 0
                    ? 0m
                    : Math.Round(cohortIds.Average(eid =>
                        WeeklyPerfAverage(
                            latestSubs.Where(s => s.EnrollmentId == eid && s.CourseId == courseId).ToList(),
                            start,
                            w)), 1);

                weeklyPerf.Add(new StudentCabinetProgressWeekChartPointDto
                {
                    WeekNumber = w,
                    Label = $"Неделя {w}",
                    StudentPercent = stPerf,
                    GroupAveragePercent = grpPerf,
                });
            }

            result.Add(new StudentCabinetProgressDashboardDto
            {
                EnrollmentId = enrollmentId,
                CourseTitle = en.Instance.Course.Title,
                InstanceName = en.Instance.InstanceName,
                InstanceId = instanceId,
                TotalLessons = totalLessons,
                CompletedLessons = completedLessons,
                LessonProgressPercent = lessonPct,
                FinalScore = en.FinalScore,
                TotalStudySeconds = studySeconds,
                AverageAssignmentPercent = avgAssignPct,
                SubmittedAssignmentsCount = submittedCount,
                SuccessfulAssignmentsPercent = successPct,
                WeeklyLessonProgress = weeklyLesson,
                WeeklyPerformance = weeklyPerf,
                Cohort = cohortDto,
            });
        }

        return Ok(result);
    }

    private sealed record ProgressFlatRow(int EnrollmentId, int CourseId, bool? IsCompleted, DateTime? CompletedAt, int? WatchTimeSeconds);

    private sealed record SubFlatRow(int EnrollmentId, int CourseId, int AssignmentId, DateTime? SubmittedAt, int? Score, int MaxScore);

    private static int ResolveWeekCount(DateOnly start, DateOnly? end, int? totalWeeks)
    {
        if (totalWeeks is > 0)
            return totalWeeks.Value;
        if (end.HasValue)
        {
            var days = end.Value.DayNumber - start.DayNumber;
            return Math.Max(1, (int)Math.Ceiling(days / 7d));
        }

        return 8;
    }

    private static decimal LessonCumulativePercent(List<ProgressFlatRow> rows, int totalLessons, DateOnly weekEnd)
    {
        if (totalLessons <= 0)
            return 0;
        var done = rows.Count(p =>
        {
            if (p.IsCompleted != true)
                return false;
            if (p.CompletedAt.HasValue)
                return DateOnly.FromDateTime(p.CompletedAt.Value.Date) <= weekEnd;
            return true;
        });
        return Math.Round((decimal)done * 100m / totalLessons, 1);
    }

    private static decimal WeeklyPerfAverage(List<SubFlatRow> latestForEnrollment, DateOnly instanceStart, int weekNumber)
    {
        var weekStart = instanceStart.AddDays((weekNumber - 1) * 7);
        var weekEndExcl = instanceStart.AddDays(weekNumber * 7);

        var inWeek = latestForEnrollment.Where(s =>
        {
            if (!s.SubmittedAt.HasValue || !s.Score.HasValue || s.MaxScore <= 0)
                return false;
            var d = DateOnly.FromDateTime(s.SubmittedAt.Value.Date);
            return d >= weekStart && d < weekEndExcl;
        }).ToList();

        if (inWeek.Count == 0)
            return 0;
        return Math.Round(inWeek.Average(s => (decimal)s.Score!.Value * 100m / s.MaxScore), 1);
    }

    private static bool CabinetScheduleReleased(CourseSchedulePlan plan, DateOnly instanceStart)
    {
        if (!plan.IsPublished) return false;
        if (plan.ScheduledAt.HasValue)
            return plan.ScheduledAt.Value <= DateTime.UtcNow;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return instanceStart.AddDays(plan.ReleaseDayOffset) <= today;
    }
}
