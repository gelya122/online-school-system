using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;

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

        var submission = new Submission
        {
            ProgressId = progress.ProgressId,
            AssignmentId = assignmentId,
            StudentAnswerText = answer,
            SubmissionStatusId = submittedStatus
        };

        _context.Submissions.Add(submission);
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
            SubmittedAt = submission.SubmittedAt,
            Score = submission.Score,
            SubmissionStatusName = statusName,
            TeacherComment = submission.TeacherComment
        });
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
