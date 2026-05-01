using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;
using System.Data;

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

        var access = await _context.StudentLessonAccesses.AsNoTracking()
            .FirstOrDefaultAsync(a => a.EnrollmentId == enrollmentId && a.LessonId == lessonId);

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
                a.AssignmentTypeId,
                TypeName = a.AssignmentType.TypeName,
                a.MaxScore,
                a.DueDaysAfterLesson,
                a.CreatedAt
            })
            .ToListAsync();

        DateOnly? accessDate = access?.PlannedAccessDate;
        var assignmentDtos = assignments.Select(a => new StudentCabinetAssignmentDto
        {
            AssignmentId = a.AssignmentId,
            Title = a.Title,
            Description = a.Description,
            AssignmentTypeId = a.AssignmentTypeId,
            AssignmentTypeName = a.TypeName,
            MaxScore = a.MaxScore,
            DueDaysAfterLesson = a.DueDaysAfterLesson,
            CalculatedDueDate = accessDate.HasValue && a.DueDaysAfterLesson.HasValue
                ? accessDate.Value.AddDays(a.DueDaysAfterLesson.Value)
                : null,
            CreatedAt = a.CreatedAt
        }).ToList();

        var submissions = new List<StudentCabinetSubmissionDto>();
        if (progress != null)
        {
            var rawSubs = await _context.Submissions.AsNoTracking()
                .Where(s => s.ProgressId == progress.ProgressId)
                .OrderByDescending(s => s.SubmittedAt)
                .Select(s => new StudentCabinetSubmissionDto
                {
                    SubmissionId = s.SubmissionId,
                    AssignmentId = s.AssignmentId,
                    StudentAnswerText = s.StudentAnswerText,
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
            IsFreePreview = lesson.IsFreePreview,
            CreatedAt = lesson.CreatedAt,
            Access = access == null
                ? null
                : new StudentCabinetLessonAccessDto
                {
                    AccessId = access.AccessId,
                    PlannedAccessDate = access.PlannedAccessDate,
                    PlannedAccessTime = access.PlannedAccessTime?.ToString("HH:mm"),
                    ActualOpenDatetime = access.ActualOpenDatetime,
                    IsAvailable = access.IsAvailable
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
            from sla in _context.StudentLessonAccesses.Where(s => s.EnrollmentId == e.EnrollmentId)
            join l in _context.Lessons on sla.LessonId equals l.LessonId
            join mod in _context.CourseModules on l.ModuleId equals mod.ModuleId
            where mod.CourseId == c.CourseId
            join a in _context.Assignments on l.LessonId equals a.LessonId
            join at in _context.AssignmentTypes on a.AssignmentTypeId equals at.TypeId into atj
            from at in atj.DefaultIfEmpty()
            join sp in _context.StudentProgresses on new { e.EnrollmentId, l.LessonId } equals new { sp.EnrollmentId, sp.LessonId } into spj
            from sp in spj.DefaultIfEmpty()
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
                a.AssignmentTypeId,
                AssignmentTypeName = at != null ? at.TypeName : null,
                a.MaxScore,
                a.DueDaysAfterLesson,
                sla.PlannedAccessDate,
                ProgressId = sp != null ? (int?)sp.ProgressId : null
            })
            .ToListAsync();

        var progressIds = baseRows.Where(r => r.ProgressId.HasValue).Select(r => r.ProgressId!.Value).Distinct().ToList();
        var submissions = await _context.Submissions.AsNoTracking()
            .Where(s => progressIds.Contains(s.ProgressId))
            .Select(s => new
            {
                s.ProgressId,
                s.AssignmentId,
                s.SubmittedAt,
                s.Score,
                StatusName = s.SubmissionStatus != null ? s.SubmissionStatus.StatusName : null
            })
            .ToListAsync();

        var latestByKey = submissions
            .GroupBy(s => new { s.ProgressId, s.AssignmentId })
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(x => x.SubmittedAt ?? DateTime.MinValue).First());

        var result = baseRows.Select(r =>
        {
            StudentCabinetSubmissionDto? sub = null;
            if (r.ProgressId.HasValue &&
                latestByKey.TryGetValue(new { ProgressId = r.ProgressId.Value, r.AssignmentId }, out var found))
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
                AssignmentTypeId = r.AssignmentTypeId,
                AssignmentTypeName = r.AssignmentTypeName,
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

        var access = await _context.StudentLessonAccesses
            .FirstOrDefaultAsync(a => a.EnrollmentId == enrollmentId && a.LessonId == lessonId);
        if (access == null)
            return BadRequest("Для урока не настроен доступ.");

        var progress = await _context.StudentProgresses
            .FirstOrDefaultAsync(p => p.EnrollmentId == enrollmentId && p.LessonId == lessonId);
        if (progress == null)
        {
            progress = new StudentProgress
            {
                EnrollmentId = enrollmentId,
                LessonId = lessonId,
                AccessId = access.AccessId,
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

        var answer = dto.AnswerText?.Trim();
        if (string.IsNullOrWhiteSpace(answer))
            return BadRequest("Введите ответ перед отправкой.");

        var score = (int?)null;
        if (!string.IsNullOrWhiteSpace(assignment.CorrectAnswer))
        {
            score = string.Equals(
                answer,
                assignment.CorrectAnswer.Trim(),
                StringComparison.OrdinalIgnoreCase)
                ? assignment.MaxScore
                : 0;
        }

        var submission = await _context.Submissions
            .Where(s => s.ProgressId == progress.ProgressId && s.AssignmentId == assignmentId)
            .OrderByDescending(s => s.SubmittedAt ?? s.CreatedAt ?? DateTime.MinValue)
            .FirstOrDefaultAsync();
        if (submission == null)
        {
            submission = new Submission
            {
                ProgressId = progress.ProgressId,
                AssignmentId = assignmentId
            };
            _context.Submissions.Add(submission);
        }

        submission.StudentAnswerText = answer;
        submission.SubmissionStatusId = submittedStatus;
        submission.Score = score;
        submission.SubmittedAt = DateTime.UtcNow;
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
            StudentAnswerText = submission.StudentAnswerText,
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

        int progressId;
        try
        {
            progressId = await EnsureProgressAsync(enrollmentId, lessonId);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        var submissionId = await EnsureSubmissionAsync(progressId, assignmentId);

        var sql = @"
SELECT
    q.question_id AS QuestionId,
    q.question_order AS QuestionOrder,
    q.question_text AS QuestionText,
    q.question_type AS QuestionType,
    q.max_points AS MaxPoints,
    sa.response_text AS StudentAnswer,
    sa.points_awarded AS PointsAwarded
FROM test_question q
LEFT JOIN test_student_answer sa
    ON sa.question_id = q.question_id
   AND sa.submission_id = @submissionId
WHERE q.assignment_id = @assignmentId
ORDER BY q.question_order, q.question_id;";

        var rows = new List<StudentCabinetQuestionDto>();
        await using var conn = _context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.Add(new SqlParameter("@submissionId", submissionId));
        cmd.Parameters.Add(new SqlParameter("@assignmentId", assignmentId));
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            rows.Add(new StudentCabinetQuestionDto
            {
                QuestionId = reader.GetInt32(0),
                QuestionOrder = reader.GetInt32(1),
                QuestionText = reader.IsDBNull(2) ? "" : reader.GetString(2),
                QuestionType = reader.IsDBNull(3) ? null : reader.GetString(3),
                MaxPoints = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4),
                StudentAnswer = reader.IsDBNull(5) ? null : reader.GetString(5),
                PointsAwarded = reader.IsDBNull(6) ? null : reader.GetDecimal(6)
            });
        }

        return Ok(rows);
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

        int progressId;
        try
        {
            progressId = await EnsureProgressAsync(enrollmentId, lessonId);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        var submissionId = await EnsureSubmissionAsync(progressId, assignmentId);

        await using var conn = _context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync();

        var questionCmd = conn.CreateCommand();
        questionCmd.CommandText = @"
SELECT TOP 1 question_id, question_order, question_text, question_type, max_points, case_insensitive_text
FROM test_question
WHERE question_id = @questionId AND assignment_id = @assignmentId;";
        questionCmd.Parameters.Add(new SqlParameter("@questionId", questionId));
        questionCmd.Parameters.Add(new SqlParameter("@assignmentId", assignmentId));
        await using var qr = await questionCmd.ExecuteReaderAsync();
        if (!await qr.ReadAsync())
            return NotFound();

        var questionOrder = qr.GetInt32(1);
        var questionText = qr.IsDBNull(2) ? "" : qr.GetString(2);
        var questionType = qr.IsDBNull(3) ? null : qr.GetString(3);
        var maxPoints = qr.IsDBNull(4) ? 0m : qr.GetDecimal(4);
        var caseInsensitive = qr.IsDBNull(5) || qr.GetBoolean(5);

        await qr.CloseAsync();

        var correctAnswers = new List<string>();
        var ansCmd = conn.CreateCommand();
        ansCmd.CommandText = @"
SELECT answer_text
FROM test_text_answer
WHERE question_id = @questionId
ORDER BY answer_order, text_answer_id;";
        ansCmd.Parameters.Add(new SqlParameter("@questionId", questionId));
        await using (var ar = await ansCmd.ExecuteReaderAsync())
        {
            while (await ar.ReadAsync())
            {
                if (!ar.IsDBNull(0))
                    correctAnswers.Add(ar.GetString(0).Trim());
            }
        }

        decimal? pointsAwarded = null;
        var autoGradeState = "manual_required";
        if (correctAnswers.Count > 0)
        {
            var comparison = caseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var isCorrect = correctAnswers.Any(a => string.Equals(a, answer, comparison));
            pointsAwarded = isCorrect ? maxPoints : 0m;
            autoGradeState = isCorrect ? "correct" : "incorrect";
        }

        var upsertCmd = conn.CreateCommand();
        upsertCmd.CommandText = @"
MERGE test_student_answer AS target
USING (SELECT @submissionId AS submission_id, @questionId AS question_id) AS src
ON target.submission_id = src.submission_id AND target.question_id = src.question_id
WHEN MATCHED THEN
    UPDATE SET response_text = @responseText, points_awarded = @pointsAwarded, answered_at = SYSUTCDATETIME()
WHEN NOT MATCHED THEN
    INSERT (submission_id, question_id, response_text, points_awarded, auto_grade_state, is_fully_auto_graded, answered_at)
    VALUES (@submissionId, @questionId, @responseText, @pointsAwarded, @autoGradeState, @isFullyAutoGraded, SYSUTCDATETIME());";
        upsertCmd.Parameters.Add(new SqlParameter("@submissionId", submissionId));
        upsertCmd.Parameters.Add(new SqlParameter("@questionId", questionId));
        upsertCmd.Parameters.Add(new SqlParameter("@responseText", answer));
        upsertCmd.Parameters.Add(new SqlParameter("@pointsAwarded", (object?)pointsAwarded ?? DBNull.Value));
        upsertCmd.Parameters.Add(new SqlParameter("@autoGradeState", autoGradeState));
        upsertCmd.Parameters.Add(new SqlParameter("@isFullyAutoGraded", pointsAwarded.HasValue));
        await upsertCmd.ExecuteNonQueryAsync();

        var totalCmd = conn.CreateCommand();
        totalCmd.CommandText = @"
SELECT ISNULL(SUM(ISNULL(points_awarded, 0)), 0)
FROM test_student_answer
WHERE submission_id = @submissionId;";
        totalCmd.Parameters.Add(new SqlParameter("@submissionId", submissionId));
        var totalObj = await totalCmd.ExecuteScalarAsync();
        var totalScore = totalObj == null || totalObj == DBNull.Value ? 0m : Convert.ToDecimal(totalObj);

        var submittedStatus = await _context.SubmissionStatuses.AsNoTracking()
            .Where(s => s.StatusName != null && s.StatusName.ToLower().Contains("отправ"))
            .OrderBy(s => s.StatusId)
            .Select(s => (int?)s.StatusId)
            .FirstOrDefaultAsync();

        var submission = await _context.Submissions.FirstAsync(s => s.SubmissionId == submissionId);
        submission.StudentAnswerText = answer;
        submission.Score = (int)Math.Round(totalScore, MidpointRounding.AwayFromZero);
        submission.SubmissionStatusId = submittedStatus;
        submission.SubmittedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new StudentCabinetQuestionDto
        {
            QuestionId = questionId,
            QuestionOrder = questionOrder,
            QuestionText = questionText,
            QuestionType = questionType,
            MaxPoints = maxPoints,
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

        await using var conn = _context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync();

        int progressId;
        try
        {
            progressId = await EnsureProgressAsync(enrollmentId, lessonId);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        var submissionId = await EnsureSubmissionAsync(progressId, assignmentId);

        var totalCmd = conn.CreateCommand();
        totalCmd.CommandText = @"
SELECT ISNULL(SUM(ISNULL(points_awarded, 0)), 0)
FROM test_student_answer
WHERE submission_id = @submissionId;";
        totalCmd.Parameters.Add(new SqlParameter("@submissionId", submissionId));
        var totalObj = await totalCmd.ExecuteScalarAsync();
        var total = totalObj == null || totalObj == DBNull.Value ? 0m : Convert.ToDecimal(totalObj);

        var maxCmd = conn.CreateCommand();
        maxCmd.CommandText = @"
SELECT ISNULL(SUM(ISNULL(max_points, 0)), 0)
FROM test_question
WHERE assignment_id = @assignmentId;";
        maxCmd.Parameters.Add(new SqlParameter("@assignmentId", assignmentId));
        var maxObj = await maxCmd.ExecuteScalarAsync();
        var max = maxObj == null || maxObj == DBNull.Value ? 0m : Convert.ToDecimal(maxObj);

        return Ok(new StudentCabinetAssignmentResultDto
        {
            AssignmentId = assignmentId,
            TotalScore = total,
            MaxScore = max
        });
    }

    private async Task<int> EnsureProgressAsync(int enrollmentId, int lessonId)
    {
        var access = await _context.StudentLessonAccesses
            .FirstOrDefaultAsync(a => a.EnrollmentId == enrollmentId && a.LessonId == lessonId);
        if (access == null)
            throw new InvalidOperationException("Для урока не настроен доступ.");

        var progress = await _context.StudentProgresses
            .FirstOrDefaultAsync(p => p.EnrollmentId == enrollmentId && p.LessonId == lessonId);
        if (progress == null)
        {
            progress = new StudentProgress
            {
                EnrollmentId = enrollmentId,
                LessonId = lessonId,
                AccessId = access.AccessId,
                IsCompleted = false,
                LastAccessed = DateTime.UtcNow
            };
            _context.StudentProgresses.Add(progress);
            await _context.SaveChangesAsync();
        }

        return progress.ProgressId;
    }

    private async Task<int> EnsureSubmissionAsync(int progressId, int assignmentId)
    {
        var submission = await _context.Submissions
            .Where(s => s.ProgressId == progressId && s.AssignmentId == assignmentId)
            .OrderByDescending(s => s.SubmittedAt ?? s.CreatedAt ?? DateTime.MinValue)
            .FirstOrDefaultAsync();
        if (submission != null)
            return submission.SubmissionId;

        var created = new Submission
        {
            ProgressId = progressId,
            AssignmentId = assignmentId
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
}
