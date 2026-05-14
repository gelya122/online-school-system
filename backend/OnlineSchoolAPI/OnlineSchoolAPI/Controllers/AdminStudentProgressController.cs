using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;
using OnlineSchoolAPI.Services;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Authorize]
public sealed class AdminStudentProgressController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public AdminStudentProgressController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    private static string PreviewText(string? text, int maxLen = 100)
    {
        if (string.IsNullOrEmpty(text)) return "";
        var t = text.Trim();
        return t.Length <= maxLen ? t : t[..maxLen] + "…";
    }

    [HttpGet("api/admin/student-progress")]
    public async Task<ActionResult<IReadOnlyList<AdminStudentProgressListRowDto>>> GetList(
        [FromQuery] string? search = null,
        [FromQuery] int? courseId = null,
        [FromQuery] int? instanceId = null,
        [FromQuery] int? studentId = null,
        CancellationToken cancellationToken = default)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin", "manager", "teacher") is { } forbidRole)
            return forbidRole;

        var q = _context.Enrollments
            .AsNoTracking()
            .Include(en => en.Student).ThenInclude(s => s.User)
            .Include(en => en.Instance).ThenInclude(i => i.Course)
            .Where(en => en.Student.DeletedAt == null && en.Instance.DeletedAt == null);

        var role = AuthClaims.GetRole(User);
        if (string.Equals(role, "teacher", StringComparison.OrdinalIgnoreCase))
        {
            var employeeId = AuthClaims.GetEmployeeId(User);
            if (employeeId == null)
                return StatusCode(403, "Недостаточно прав.");

            q = q.Where(en => en.Instance.CourseInstanceStaff.Any(s =>
                s.EmployeeId == employeeId.Value &&
                s.RoleId == CourseInstanceStaffRoleIds.Teacher &&
                s.DeletedAt == null));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            q = q.Where(en =>
                ((en.Student.FirstName ?? "") + " " + (en.Student.LastName ?? "")).ToLower().Contains(s) ||
                (en.Student.User.Email ?? "").ToLower().Contains(s) ||
                (en.Student.Phone ?? "").ToLower().Contains(s));
        }

        if (courseId.HasValue)
            q = q.Where(en => en.Instance.CourseId == courseId.Value);

        if (instanceId.HasValue)
            q = q.Where(en => en.InstanceId == instanceId.Value);

        if (studentId.HasValue)
            q = q.Where(en => en.StudentId == studentId.Value);

        var totalLessonsByCourse = await _context.Lessons
            .AsNoTracking()
            .Where(l => l.DeletedAt == null && l.Module.DeletedAt == null)
            .GroupBy(l => l.Module.CourseId)
            .Select(g => new { CourseId = g.Key, Total = g.Count() })
            .ToDictionaryAsync(x => x.CourseId, x => x.Total, cancellationToken);

        var totalHwByCourse = await _context.Assignments
            .AsNoTracking()
            .Where(a => a.Lesson.DeletedAt == null && a.Lesson.Module.DeletedAt == null)
            .GroupBy(a => a.Lesson.Module.CourseId)
            .Select(g => new { CourseId = g.Key, Cnt = g.Count() })
            .ToDictionaryAsync(x => x.CourseId, x => x.Cnt, cancellationToken);

        var maxHwByCourse = await _context.Assignments
            .AsNoTracking()
            .Where(a => a.Lesson.DeletedAt == null && a.Lesson.Module.DeletedAt == null)
            .GroupBy(a => a.Lesson.Module.CourseId)
            .Select(g => new { CourseId = g.Key, SumMax = g.Sum(x => x.MaxScore) })
            .ToDictionaryAsync(x => x.CourseId, x => x.SumMax, cancellationToken);

        var completedByEnrollment = await _context.StudentProgresses
            .AsNoTracking()
            .Where(p => p.IsCompleted == true)
            .GroupBy(p => p.EnrollmentId)
            .Select(g => new { EnrollmentId = g.Key, Completed = g.Count(), LastActivity = g.Max(x => x.LastAccessed) })
            .ToDictionaryAsync(x => x.EnrollmentId, x => new { x.Completed, x.LastActivity }, cancellationToken);

        var hwAgg = await (
                from s in _context.Submissions.AsNoTracking()
                where s.SubmittedAt != null && s.EnrollmentId != null
                group s by s.EnrollmentId!.Value
                into g
                select new
                {
                    EnrollmentId = g.Key,
                    SubmittedCount = g.Count(),
                    AvgScore = g.Average(x => (double?)(x.Score)),
                    SumScore = g.Sum(x => x.Score ?? 0)
                })
            .ToDictionaryAsync(x => x.EnrollmentId, x => new { x.SubmittedCount, x.AvgScore, x.SumScore }, cancellationToken);

        var list = await q
            .OrderByDescending(en => en.EnrolledAt)
            .Take(500)
            .Select(en => new AdminStudentProgressListRowDto
            {
                EnrollmentId = en.EnrollmentId,
                StudentId = en.StudentId,
                StudentName = ((en.Student.FirstName ?? "") + " " + (en.Student.LastName ?? "")).Trim(),
                Email = en.Student.User.Email,
                CourseId = en.Instance.CourseId,
                CourseTitle = en.Instance.Course.Title,
                InstanceId = en.InstanceId,
                InstanceName = en.Instance.InstanceName,
                CompletedLessons = 0,
                TotalLessons = 0,
                Percent = 0,
                TotalHomeworkCount = 0,
                SubmittedHomeworkCount = 0,
                AverageScore = null,
                TotalHomeworkScore = null,
                TotalHomeworkMaxPoints = 0,
                LastActivityAt = null
            })
            .ToListAsync(cancellationToken);

        foreach (var r in list)
        {
            var total = totalLessonsByCourse.TryGetValue(r.CourseId, out var t) ? t : 0;
            if (total <= 0) total = 1;
            r.TotalLessons = total;

            if (completedByEnrollment.TryGetValue(r.EnrollmentId, out var c))
            {
                r.CompletedLessons = c.Completed;
                r.LastActivityAt = c.LastActivity;
            }

            r.Percent = (int)Math.Min(100, Math.Round(r.CompletedLessons * 100.0 / total, MidpointRounding.AwayFromZero));

            r.TotalHomeworkCount = totalHwByCourse.TryGetValue(r.CourseId, out var th) ? th : 0;
            r.TotalHomeworkMaxPoints = maxHwByCourse.TryGetValue(r.CourseId, out var mh) ? mh : 0;

            if (hwAgg.TryGetValue(r.EnrollmentId, out var h))
            {
                r.SubmittedHomeworkCount = h.SubmittedCount;
                r.AverageScore = h.AvgScore == null ? null : (decimal?)Math.Round((decimal)h.AvgScore.Value, 2, MidpointRounding.AwayFromZero);
                r.TotalHomeworkScore = h.SumScore;
            }
        }

        return Ok(list);
    }

    [HttpGet("api/admin/students/{studentId:int}/progress")]
    public async Task<ActionResult<IReadOnlyList<AdminStudentProgressListRowDto>>> GetStudentProgress(int studentId, CancellationToken cancellationToken)
    {
        return await GetList(search: null, courseId: null, instanceId: null, studentId: studentId, cancellationToken: cancellationToken);
    }

    [HttpGet("api/admin/students/{studentId:int}/instances/{instanceId:int}/progress")]
    public async Task<ActionResult<AdminEnrollmentProgressDto>> GetStudentInstanceProgress(int studentId, int instanceId, CancellationToken cancellationToken)
    {
        var enrollmentId = await _context.Enrollments
            .AsNoTracking()
            .Where(en => en.StudentId == studentId && en.InstanceId == instanceId)
            .Select(en => (int?)en.EnrollmentId)
            .FirstOrDefaultAsync(cancellationToken);
        if (enrollmentId == null) return NotFound();
        return await GetEnrollmentProgress(enrollmentId.Value, cancellationToken);
    }

    [HttpGet("api/admin/enrollments/{enrollmentId:int}/progress")]
    public async Task<ActionResult<AdminEnrollmentProgressDto>> GetEnrollmentProgress(int enrollmentId, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin", "manager", "teacher") is { } forbidRole)
            return forbidRole;

        var role = AuthClaims.GetRole(User);
        if (string.Equals(role, "teacher", StringComparison.OrdinalIgnoreCase))
        {
            var employeeId = AuthClaims.GetEmployeeId(User);
            if (employeeId == null)
                return StatusCode(403, "Недостаточно прав.");

            var instanceId = await _context.Enrollments.AsNoTracking()
                .Where(en => en.EnrollmentId == enrollmentId)
                .Select(en => (int?)en.InstanceId)
                .FirstOrDefaultAsync(cancellationToken);
            if (instanceId == null)
                return NotFound();

            var assigned = await _context.CourseInstanceStaff.AsNoTracking()
                .AnyAsync(s => s.InstanceId == instanceId.Value && s.EmployeeId == employeeId.Value && s.RoleId == CourseInstanceStaffRoleIds.Teacher && s.DeletedAt == null, cancellationToken);
            if (!assigned)
                return StatusCode(403, "Недостаточно прав.");
        }

        var e = await _context.Enrollments
            .AsNoTracking()
            .Include(x => x.Student).ThenInclude(s => s.User)
            .Include(x => x.Instance).ThenInclude(i => i.Course)
            .FirstOrDefaultAsync(x => x.EnrollmentId == enrollmentId, cancellationToken);
        if (e == null) return NotFound();

        var courseId = e.Instance.CourseId;

        var totalLessons = await _context.Lessons.AsNoTracking()
            .CountAsync(l => l.Module.CourseId == courseId && l.DeletedAt == null && l.Module.DeletedAt == null, cancellationToken);

        var modulesDb = await _context.CourseModules
            .AsNoTracking()
            .Where(m => m.CourseId == courseId && m.DeletedAt == null)
            .OrderBy(m => m.ModuleOrder)
            .Select(m => new { m.ModuleId, m.Title, m.ModuleOrder })
            .ToListAsync(cancellationToken);

        var lessonsDb = await _context.Lessons
            .AsNoTracking()
            .Where(l => l.Module.CourseId == courseId && l.DeletedAt == null && l.Module.DeletedAt == null)
            .OrderBy(l => l.Module.ModuleOrder).ThenBy(l => l.LessonOrder)
            .Select(l => new { l.LessonId, l.Title, l.LessonOrder, l.ModuleId })
            .ToListAsync(cancellationToken);

        var planList = await _context.CourseSchedulePlans
            .AsNoTracking()
            .Where(p => p.InstanceId == e.InstanceId)
            .ToListAsync(cancellationToken);
        var planByLesson = planList.ToDictionary(p => p.LessonId, p => p);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        bool PlanReleased(CourseSchedulePlan pl)
        {
            if (!pl.IsPublished) return false;
            if (pl.ScheduledAt.HasValue)
                return pl.ScheduledAt.Value <= DateTime.UtcNow;
            return e.Instance.StartDate.AddDays(pl.ReleaseDayOffset) <= today;
        }

        var progList = await _context.StudentProgresses
            .AsNoTracking()
            .Where(p => p.EnrollmentId == enrollmentId)
            .Select(p => new { p.LessonId, p.IsCompleted, p.CompletedAt, p.WatchTimeSeconds, p.LastAccessed })
            .ToListAsync(cancellationToken);
        var progByLesson = progList.GroupBy(x => x.LessonId).ToDictionary(
            g => g.Key,
            g => new
            {
                IsCompleted = g.Any(x => x.IsCompleted == true),
                CompletedAt = g.Max(x => x.CompletedAt),
                Watch = g.Sum(x => x.WatchTimeSeconds ?? 0),
                Last = g.Max(x => x.LastAccessed)
            });

        var assigns = await _context.Assignments
            .AsNoTracking()
            .Where(a => a.Lesson.Module.CourseId == courseId && a.Lesson.DeletedAt == null && a.Lesson.Module.DeletedAt == null)
            .Select(a => new { a.AssignmentId, a.LessonId, a.Title, a.MaxScore })
            .ToListAsync(cancellationToken);
        var assignByLesson = assigns.GroupBy(a => a.LessonId).ToDictionary(g => g.Key, g => g.OrderBy(x => x.AssignmentId).ToList());

        var assignmentIds = assigns.Select(a => a.AssignmentId).Distinct().ToList();
        var qMaxByAssign = assignmentIds.Count == 0
            ? new Dictionary<int, int>()
            : await _context.TestQuestions
                .AsNoTracking()
                .Where(q => assignmentIds.Contains(q.AssignmentId))
                .GroupBy(q => q.AssignmentId)
                .Select(g => new { Id = g.Key, Sum = g.Sum(x => x.MaxPoints) })
                .ToDictionaryAsync(x => x.Id, x => (int)Math.Round(x.Sum, MidpointRounding.AwayFromZero), cancellationToken);

        var subsRaw = await (
                from s in _context.Submissions.AsNoTracking()
                where s.EnrollmentId == enrollmentId && s.SubmittedAt != null
                join a in _context.Assignments.AsNoTracking() on s.AssignmentId equals a.AssignmentId
                join ss in _context.SubmissionStatuses on s.SubmissionStatusId equals ss.StatusId into ssj
                from ss in ssj.DefaultIfEmpty()
                select new
                {
                    s.SubmissionId,
                    s.AssignmentId,
                    LessonId = a.LessonId,
                    s.SubmittedAt,
                    s.CreatedAt,
                    s.Score,
                    s.GradedAt,
                    StatusName = ss != null ? ss.StatusName : ""
                })
            .ToListAsync(cancellationToken);

        var latestSubs = subsRaw
            .GroupBy(x => new { x.LessonId, x.AssignmentId })
            .Select(g => g.OrderByDescending(x => x.SubmittedAt ?? x.CreatedAt).First())
            .ToList();

        var subIds = latestSubs.Select(x => x.SubmissionId).Distinct().ToList();
        var answersBySub = new Dictionary<int, List<AdminProgressAnswerPreviewDto>>();
        if (subIds.Count > 0)
        {
            var answersRaw = await (
                    from a in _context.TestStudentAnswers.AsNoTracking()
                    where subIds.Contains(a.SubmissionId)
                    join q in _context.TestQuestions on a.QuestionId equals q.QuestionId
                    select new
                    {
                        a.SubmissionId,
                        a.StudentAnswerId,
                        q.QuestionId,
                        q.QuestionTypeId,
                        q.CorrectAnswer,
                        q.QuestionText,
                        q.MaxPoints,
                        a.ResponseText,
                        a.PointsAwarded
                    })
                .ToListAsync(cancellationToken);

            foreach (var gr in answersRaw.GroupBy(x => x.SubmissionId))
            {
                answersBySub[gr.Key] = gr
                    .OrderBy(x => x.QuestionId)
                    .Select(x => new AdminProgressAnswerPreviewDto
                    {
                        StudentAnswerId = x.StudentAnswerId,
                        QuestionId = x.QuestionId,
                        QuestionType = HomeworkQuestionTypeIds.ToTaskTypeSlug(x.QuestionTypeId, x.CorrectAnswer),
                        QuestionTextPreview = PreviewText(x.QuestionText),
                        ResponseTextPreview = string.IsNullOrEmpty(x.ResponseText) ? null : PreviewText(x.ResponseText, 200),
                        PointsAwarded = x.PointsAwarded,
                        MaxPoints = x.MaxPoints
                    })
                    .ToList();
            }
        }

        var modules = new List<AdminProgressModuleDto>();
        var completedLessons = 0;

        foreach (var mod in modulesDb)
        {
            var lessonDtos = new List<AdminProgressLessonDto>();
            foreach (var les in lessonsDb.Where(l => l.ModuleId == mod.ModuleId).OrderBy(l => l.LessonOrder))
            {
                if (progByLesson.TryGetValue(les.LessonId, out var pg) && pg.IsCompleted)
                    completedLessons++;

                planByLesson.TryGetValue(les.LessonId, out var pl);
                progByLesson.TryGetValue(les.LessonId, out var pr);
                var released = pl != null && PlanReleased(pl);
                var hasProgress = pr != null;
                var isAvail = hasProgress || released;
                var openedAt = pr?.Last ?? (released ? pl!.ScheduledAt : null);

                var asnDtos = new List<AdminProgressAssignmentDto>();
                if (assignByLesson.TryGetValue(les.LessonId, out var asns))
                {
                    foreach (var asn in asns)
                    {
                        qMaxByAssign.TryGetValue(asn.AssignmentId, out var qMax);
                        AdminProgressSubmissionDto? subDto = null;
                        var sub = latestSubs.FirstOrDefault(s => s.LessonId == les.LessonId && s.AssignmentId == asn.AssignmentId);
                        if (sub != null)
                        {
                            answersBySub.TryGetValue(sub.SubmissionId, out var ansList);
                            subDto = new AdminProgressSubmissionDto
                            {
                                SubmissionId = sub.SubmissionId,
                                StatusName = sub.StatusName,
                                Score = sub.Score,
                                MaxPointsFromQuestions = qMax,
                                SubmittedAt = sub.SubmittedAt,
                                GradedAt = sub.GradedAt,
                                Answers = ansList ?? []
                            };
                        }

                        asnDtos.Add(new AdminProgressAssignmentDto
                        {
                            AssignmentId = asn.AssignmentId,
                            AssignmentTitle = asn.Title,
                            AssignmentMaxScore = asn.MaxScore,
                            QuestionsMaxPoints = qMax,
                            Submission = subDto
                        });
                    }
                }

                lessonDtos.Add(new AdminProgressLessonDto
                {
                    LessonId = les.LessonId,
                    LessonTitle = les.Title,
                    LessonOrder = les.LessonOrder,
                    IsOpened = isAvail,
                    IsAvailable = isAvail,
                    OpenedAt = openedAt,
                    IsCompleted = pr?.IsCompleted == true,
                    CompletedAt = pr?.CompletedAt,
                    WatchTimeSeconds = pr?.Watch ?? 0,
                    LastAccessed = pr?.Last,
                    Assignments = asnDtos
                });
            }

            modules.Add(new AdminProgressModuleDto
            {
                ModuleId = mod.ModuleId,
                ModuleTitle = mod.Title,
                ModuleOrder = mod.ModuleOrder,
                Lessons = lessonDtos
            });
        }

        var submittedCount = latestSubs.Count;
        var sumScore = latestSubs.Sum(x => x.Score ?? 0);
        var scoredSubs = latestSubs.Where(x => x.Score != null).Select(x => x.Score!.Value).ToList();
        var avg = scoredSubs.Count == 0
            ? (decimal?)null
            : Math.Round((decimal)scoredSubs.Average(), 2, MidpointRounding.AwayFromZero);
        var maxPointsCourse = assigns.Sum(x => x.MaxScore);
        var completionPercent = totalLessons <= 0
            ? 0
            : (int)Math.Min(100, Math.Round(completedLessons * 100.0 / totalLessons, MidpointRounding.AwayFromZero));

        return Ok(new AdminEnrollmentProgressDto
        {
            EnrollmentId = e.EnrollmentId,
            StudentId = e.StudentId,
            StudentName = ((e.Student.FirstName ?? "") + " " + (e.Student.LastName ?? "")).Trim(),
            Email = e.Student.User.Email,
            CourseId = e.Instance.CourseId,
            CourseTitle = e.Instance.Course.Title,
            InstanceId = e.InstanceId,
            InstanceName = e.Instance.InstanceName,
            StartDate = e.Instance.StartDate,
            TotalLessons = totalLessons,
            CompletedLessons = completedLessons,
            CompletionPercent = completionPercent,
            TotalHomeworkCount = assigns.Count,
            SubmittedHomeworkCount = submittedCount,
            AverageHomeworkScore = avg,
            TotalHomeworkScore = sumScore,
            TotalHomeworkMaxPoints = maxPointsCourse,
            Modules = modules
        });
    }
}
