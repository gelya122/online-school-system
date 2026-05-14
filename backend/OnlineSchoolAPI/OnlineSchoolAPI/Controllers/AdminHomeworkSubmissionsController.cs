using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;
using OnlineSchoolAPI.Services;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Authorize]
public sealed class AdminHomeworkSubmissionsController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public AdminHomeworkSubmissionsController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    private static async Task RecalcSubmissionScoreAsync(OnlineSchoolDbContext ctx, int submissionId, CancellationToken cancellationToken)
    {
        var total = await ctx.TestStudentAnswers
            .Where(a => a.SubmissionId == submissionId)
            .SumAsync(a => a.PointsAwarded ?? 0m, cancellationToken);
        var sub = await ctx.Submissions.FirstAsync(s => s.SubmissionId == submissionId, cancellationToken);
        sub.Score = (int)Math.Round(total, MidpointRounding.AwayFromZero);
    }

    private static async Task UpdateSubmissionCheckStatusAsync(OnlineSchoolDbContext ctx, int submissionId, CancellationToken cancellationToken)
    {
        var totalAnswers = await ctx.TestStudentAnswers.CountAsync(a => a.SubmissionId == submissionId, cancellationToken);
        if (totalAnswers == 0) return;

        var graded = await ctx.TestStudentAnswers.CountAsync(a => a.SubmissionId == submissionId && a.PointsAwarded != null, cancellationToken);
        var sub = await ctx.Submissions.FirstAsync(s => s.SubmissionId == submissionId, cancellationToken);
        var key = graded == totalAnswers
            ? AdminCabinetSubmissionStatuses.Checked
            : AdminCabinetSubmissionStatuses.ManualCheckRequired;
        sub.SubmissionStatusId = await AdminCabinetSubmissionStatuses.GetOrCreateStatusIdAsync(ctx, key, cancellationToken);
    }

    [HttpGet("api/admin/homework-submissions/review-queue")]
    public async Task<ActionResult<IReadOnlyList<AdminHomeworkAnswerReviewQueueRowDto>>> ReviewQueue(
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] string? reviewState = null,
        [FromQuery] int? courseId = null,
        [FromQuery] int? instanceId = null,
        [FromQuery] int? reviewerId = null,
        [FromQuery] int? mentorId = null,
        [FromQuery] int? studentId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin", "manager", "teacher") is { } forbidRole)
            return forbidRole;

        var q =
            from ts in _context.TestStudentAnswers.AsNoTracking()
            join qn in _context.TestQuestions on ts.QuestionId equals qn.QuestionId
            join qt in _context.QuestionTypes on qn.QuestionTypeId equals qt.QuestionTypeId
            where qt.Title == "Развернутый ответ"
            join s in _context.Submissions on ts.SubmissionId equals s.SubmissionId
            where s.SubmittedAt != null && s.EnrollmentId != null
            join e in _context.Enrollments on s.EnrollmentId equals e.EnrollmentId
            join st in _context.Students on e.StudentId equals st.StudentId
            join u in _context.Users on st.UserId equals u.UserId into uj
            from u in uj.DefaultIfEmpty()
            join a in _context.Assignments on s.AssignmentId equals a.AssignmentId
            join l in _context.Lessons on a.LessonId equals l.LessonId
            join inst in _context.CourseInstances on e.InstanceId equals inst.InstanceId
            join c in _context.Courses on inst.CourseId equals c.CourseId
            join ss in _context.SubmissionStatuses on s.SubmissionStatusId equals ss.StatusId into ssj
            from ss in ssj.DefaultIfEmpty()
            join rev in _context.Employees on ts.ReviewedByEmployeeId equals rev.EmployeeId into revj
            from rev in revj.DefaultIfEmpty()
            join mentor in _context.Employees on e.AssignedTeacherId equals mentor.EmployeeId into mentj
            from mentor in mentj.DefaultIfEmpty()
            select new { ts, qn, s, e, st, u, a, l, inst, c, SubmissionStatusName = ss != null ? ss.StatusName : "", rev, mentor };

        var role = AuthClaims.GetRole(User);
        if (string.Equals(role, "teacher", StringComparison.OrdinalIgnoreCase))
        {
            var employeeId = AuthClaims.GetEmployeeId(User);
            if (employeeId == null)
                return StatusCode(403, "Недостаточно прав.");

            // Наставник видит только своих студентов: enrollment.assigned_mentor_id == current employee_id
            q = q.Where(x => x.e.AssignedTeacherId == employeeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            q = q.Where(x =>
                ((x.st.FirstName ?? "") + " " + (x.st.LastName ?? "")).ToLower().Contains(s) ||
                (x.u != null && x.u.Email != null && x.u.Email.ToLower().Contains(s)));
        }

        // reviewState: pending | reviewed | all (по умолчанию pending)
        var rs = (reviewState ?? "pending").Trim().ToLowerInvariant();
        if (rs == "pending")
            q = q.Where(x => x.ts.PointsAwarded == null && x.ts.ReviewedAt == null);
        else if (rs == "reviewed")
            q = q.Where(x => x.ts.PointsAwarded != null || x.ts.ReviewedAt != null);
        else if (rs != "all")
            return BadRequest("Некорректный reviewState. Используйте pending|reviewed|all.");

        if (!string.IsNullOrWhiteSpace(status))
        {
            var stKey = status.Trim().ToLowerInvariant();
            q = q.Where(x => x.SubmissionStatusName != null && x.SubmissionStatusName.ToLower() == stKey);
        }

        if (courseId.HasValue)
            q = q.Where(x => x.inst.CourseId == courseId.Value);

        if (instanceId.HasValue)
            q = q.Where(x => x.e.InstanceId == instanceId.Value);

        if (reviewerId.HasValue)
            q = q.Where(x => x.ts.ReviewedByEmployeeId == reviewerId.Value);

        if (mentorId.HasValue)
            q = q.Where(x => x.e.AssignedTeacherId == mentorId.Value);

        if (studentId.HasValue)
            q = q.Where(x => x.st.StudentId == studentId.Value);

        if (from.HasValue)
            q = q.Where(x => x.ts.AnsweredAt >= from.Value);

        if (to.HasValue)
            q = q.Where(x => x.ts.AnsweredAt <= to.Value);

        var raw = await q
            .OrderByDescending(x => x.ts.AnsweredAt)
            .Take(400)
            .Select(x => new
            {
                x.ts.StudentAnswerId,
                x.ts.SubmissionId,
                x.st.StudentId,
                Student = ((x.st.FirstName ?? "") + " " + (x.st.LastName ?? "")).Trim(),
                StudentEmail = x.u != null ? x.u.Email : null,
                Course = x.c.Title,
                x.inst.CourseId,
                Instance = x.inst.InstanceName,
                x.inst.InstanceId,
                Lesson = x.l.Title,
                Homework = x.a.Title,
                QuestionText = x.qn.QuestionText,
                x.ts.AnsweredAt,
                x.qn.MaxPoints,
                x.ts.PointsAwarded,
                x.ts.ReviewedAt,
                SubmissionStatus = x.SubmissionStatusName ?? "",
                x.s.Score,
                ReviewerId = x.ts.ReviewedByEmployeeId,
                Reviewer = x.rev == null ? null : (x.rev.FirstName + " " + x.rev.LastName).Trim(),
                MentorEmployeeId = x.e.AssignedTeacherId,
                MentorEmployeeName = x.mentor == null ? null : (x.mentor.FirstName + " " + x.mentor.LastName).Trim()
            })
            .ToListAsync(cancellationToken);

        static string Preview(string? text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            var t = text.Trim();
            return t.Length <= 140 ? t : t[..140] + "…";
        }

        var list = raw.Select(x => new AdminHomeworkAnswerReviewQueueRowDto
        {
            StudentAnswerId = x.StudentAnswerId,
            SubmissionId = x.SubmissionId,
            StudentId = x.StudentId,
            Student = x.Student,
            StudentEmail = x.StudentEmail,
            Course = x.Course,
            CourseId = x.CourseId,
            Instance = x.Instance,
            InstanceId = x.InstanceId,
            Lesson = x.Lesson,
            Homework = x.Homework,
            TaskPreview = Preview(x.QuestionText),
            AnsweredAt = x.AnsweredAt,
            MaxPoints = x.MaxPoints,
            MentorEmployeeId = x.MentorEmployeeId,
            MentorEmployeeName = x.MentorEmployeeName,
            ReviewStatus = x.PointsAwarded == null ? "ожидает" : "проверено",
            SubmissionStatus = x.SubmissionStatus,
            TotalScore = x.Score,
            ReviewerId = x.ReviewerId,
            Reviewer = x.Reviewer
        }).ToList();

        return Ok(list);
    }

    [HttpGet("api/admin/homework-submissions/{submissionId:int}")]
    public async Task<ActionResult<AdminHomeworkSubmissionDetailsDto>> Get(int submissionId, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin", "manager", "teacher") is { } forbidRole)
            return forbidRole;

        var row = await (
            from s in _context.Submissions.AsNoTracking()
            where s.SubmissionId == submissionId && s.EnrollmentId != null
            join e in _context.Enrollments on s.EnrollmentId equals e.EnrollmentId
            join st in _context.Students on e.StudentId equals st.StudentId
            join u in _context.Users on st.UserId equals u.UserId into uj
            from u in uj.DefaultIfEmpty()
            join a in _context.Assignments on s.AssignmentId equals a.AssignmentId
            join l in _context.Lessons on a.LessonId equals l.LessonId
            join inst in _context.CourseInstances on e.InstanceId equals inst.InstanceId
            join c in _context.Courses on inst.CourseId equals c.CourseId
            join ss in _context.SubmissionStatuses on s.SubmissionStatusId equals ss.StatusId into ssj
            from ss in ssj.DefaultIfEmpty()
            join emp in _context.Employees on s.GradedByEmployeeId equals emp.EmployeeId into emj
            from emp in emj.DefaultIfEmpty()
            select new { s, st, u, a, l, inst, c, StatusName = ss != null ? ss.StatusName : "", emp }
        ).FirstOrDefaultAsync(cancellationToken);

        if (row == null) return NotFound();

        var role = AuthClaims.GetRole(User);
        if (string.Equals(role, "teacher", StringComparison.OrdinalIgnoreCase))
        {
            var employeeId = AuthClaims.GetEmployeeId(User);
            if (employeeId == null)
                return StatusCode(403, "Недостаточно прав.");

            var assigned = await _context.CourseInstanceStaff.AsNoTracking()
                .AnyAsync(s => s.InstanceId == row.inst.InstanceId && s.EmployeeId == employeeId.Value && s.RoleId == CourseInstanceStaffRoleIds.Teacher && s.DeletedAt == null, cancellationToken);
            if (!assigned)
                return StatusCode(403, "Недостаточно прав.");
        }

        var taskRows = await _context.TestStudentAnswers
            .AsNoTracking()
            .Where(ts => ts.SubmissionId == submissionId)
            .Join(_context.TestQuestions,
                ts => ts.QuestionId,
                q => q.QuestionId,
                (ts, q) => new { ts, q })
            .OrderBy(x => x.q.QuestionOrder)
            .Select(x => new
            {
                x.ts.StudentAnswerId,
                x.q.QuestionId,
                x.q.QuestionTypeId,
                x.q.CorrectAnswer,
                x.q.QuestionText,
                x.q.Explanation,
                x.q.MaxPoints,
                x.ts.ResponseText,
                x.ts.PointsAwarded,
                x.ts.TeacherComment,
                x.ts.ReviewedByEmployeeId,
                x.ts.ReviewedAt
            })
            .ToListAsync(cancellationToken);

        var tasks = taskRows.Select(x =>
        {
            var slug = HomeworkQuestionTypeIds.ToTaskTypeSlug(x.QuestionTypeId, x.CorrectAnswer);
            var auto = x.PointsAwarded == null
                ? null
                : (x.PointsAwarded >= x.MaxPoints ? "correct" : x.PointsAwarded <= 0 ? "incorrect" : "partial");
            return new AdminHomeworkTaskSubmissionDetailsDto
            {
                TaskSubmissionId = x.StudentAnswerId,
                TaskId = x.QuestionId,
                TaskType = slug,
                Title = null,
                TaskText = x.QuestionText,
                Explanation = x.Explanation,
                MaxScore = (int)Math.Round(x.MaxPoints, MidpointRounding.AwayFromZero),
                MaxPointsDecimal = x.MaxPoints,
                StudentAnswerText = x.ResponseText,
                AttachedFileUrl = null,
                AttachedFileName = null,
                AutoCheckResult = auto,
                IsCorrect = x.PointsAwarded != null && x.PointsAwarded >= x.MaxPoints,
                PointsAwarded = x.PointsAwarded,
                Score = x.PointsAwarded == null ? null : (int?)Math.Round(x.PointsAwarded.Value, MidpointRounding.AwayFromZero),
                TeacherComment = x.TeacherComment,
                CheckedByEmployeeId = x.ReviewedByEmployeeId,
                CheckedAt = x.ReviewedAt,
                NeedsManualReview = slug == "detailed_answer" && x.ReviewedAt == null && x.PointsAwarded == null
            };
        }).ToList();

        return Ok(new AdminHomeworkSubmissionDetailsDto
        {
            SubmissionId = row.s.SubmissionId,
            StudentId = row.st.StudentId,
            StudentName = ((row.st.FirstName ?? "") + " " + (row.st.LastName ?? "")).Trim(),
            StudentEmail = row.u != null ? row.u.Email : null,
            StudentPhone = row.st.Phone,
            CourseId = row.inst.CourseId,
            CourseTitle = row.c.Title,
            InstanceId = row.inst.InstanceId,
            InstanceName = row.inst.InstanceName,
            LessonId = row.l.LessonId,
            LessonTitle = row.l.Title,
            HomeworkId = row.a.AssignmentId,
            HomeworkTitle = row.a.Title,
            Status = row.StatusName ?? "",
            TotalScore = row.s.Score,
            SubmittedAt = row.s.SubmittedAt,
            CheckedAt = row.s.GradedAt,
            CheckedByEmployeeId = row.s.GradedByEmployeeId,
            CheckedByEmployeeName = row.emp == null ? null : (row.emp.FirstName + " " + row.emp.LastName).Trim(),
            Tasks = tasks
        });
    }

    [HttpPut("api/admin/homework-task-submissions/{taskSubmissionId:int}/review")]
    public async Task<IActionResult> ReviewTask(int taskSubmissionId, [FromBody] AdminHomeworkTaskSubmissionReviewDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin", "manager", "teacher") is { } forbidRole)
            return forbidRole;

        var ts = await _context.TestStudentAnswers
            .Include(x => x.Question)
            .Include(x => x.Submission)
            .FirstOrDefaultAsync(x => x.StudentAnswerId == taskSubmissionId, cancellationToken);
        if (ts == null) return NotFound();

        var role = AuthClaims.GetRole(User);
        if (string.Equals(role, "teacher", StringComparison.OrdinalIgnoreCase))
        {
            var employeeId = AuthClaims.GetEmployeeId(User);
            if (employeeId == null)
                return StatusCode(403, "Недостаточно прав.");

            if (ts.Submission.EnrollmentId == null)
                return StatusCode(403, "Недостаточно прав.");

            var mentorId = await _context.Enrollments.AsNoTracking()
                .Where(e => e.EnrollmentId == ts.Submission.EnrollmentId)
                .Select(e => e.AssignedTeacherId)
                .FirstOrDefaultAsync(cancellationToken);
            if (!mentorId.HasValue || mentorId.Value != employeeId.Value)
                return StatusCode(403, "Недостаточно прав.");
        }

        if (!string.Equals(HomeworkQuestionTypeIds.ToTaskTypeSlug(ts.Question.QuestionTypeId, ts.Question.CorrectAnswer), "detailed_answer", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Ручная проверка доступна только для заданий типа detailed_answer.");

        if (!dto.Score.HasValue)
            return BadRequest("Укажите балл (points_awarded).");

        var awarded = dto.Score.Value;
        if (awarded < 0m || awarded > ts.Question.MaxPoints)
            return BadRequest("Балл должен быть от 0 до max_points задания.");

        if (ts.PointsAwarded != null || ts.ReviewedAt != null)
            return Conflict("Ответ уже проверен другим сотрудником. Обновите список и откройте ответ заново.");

        ts.PointsAwarded = awarded;
        ts.TeacherComment = string.IsNullOrWhiteSpace(dto.TeacherComment) ? null : dto.TeacherComment.Trim();
        ts.ReviewedAt = DateTime.UtcNow;
        ts.ReviewedByEmployeeId = AuthClaims.GetEmployeeId(User);
        ts.IsFullyAutoGraded = false;

        await _context.SaveChangesAsync(cancellationToken);

        await RecalcSubmissionScoreAsync(_context, ts.SubmissionId, cancellationToken);
        await UpdateSubmissionCheckStatusAsync(_context, ts.SubmissionId, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("api/admin/homework-submissions/{submissionId:int}/approve")]
    public async Task<IActionResult> Approve(int submissionId, CancellationToken cancellationToken)
        => await SetSubmissionStatus(submissionId, AdminCabinetSubmissionStatuses.Approved, cancellationToken, setGradedAt: true);

    [HttpPost("api/admin/homework-submissions/{submissionId:int}/request-revision")]
    public async Task<IActionResult> RequestRevision(int submissionId, CancellationToken cancellationToken)
        => await SetSubmissionStatus(submissionId, AdminCabinetSubmissionStatuses.RevisionRequested, cancellationToken, setGradedAt: false);

    [HttpPost("api/admin/homework-submissions/{submissionId:int}/reject")]
    public async Task<IActionResult> Reject(int submissionId, CancellationToken cancellationToken)
        => await SetSubmissionStatus(submissionId, AdminCabinetSubmissionStatuses.Rejected, cancellationToken, setGradedAt: true);

    private async Task<IActionResult> SetSubmissionStatus(int submissionId, string statusKey, CancellationToken cancellationToken, bool setGradedAt)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin", "manager", "teacher") is { } forbidRole)
            return forbidRole;

        var s = await _context.Submissions.FirstOrDefaultAsync(x => x.SubmissionId == submissionId, cancellationToken);
        if (s == null) return NotFound();

        var role = AuthClaims.GetRole(User);
        if (string.Equals(role, "teacher", StringComparison.OrdinalIgnoreCase))
        {
            var employeeId = AuthClaims.GetEmployeeId(User);
            if (employeeId == null)
                return StatusCode(403, "Недостаточно прав.");

            var instanceId = s.EnrollmentId == null
                ? (int?)null
                : await _context.Enrollments.AsNoTracking()
                    .Where(e => e.EnrollmentId == s.EnrollmentId)
                    .Select(e => (int?)e.InstanceId)
                    .FirstOrDefaultAsync(cancellationToken);
            if (!instanceId.HasValue)
                return StatusCode(403, "Недостаточно прав.");

            var assigned = await _context.CourseInstanceStaff.AsNoTracking()
                .AnyAsync(staff => staff.InstanceId == instanceId && staff.EmployeeId == employeeId.Value && staff.RoleId == CourseInstanceStaffRoleIds.Teacher && staff.DeletedAt == null, cancellationToken);
            if (!assigned)
                return StatusCode(403, "Недостаточно прав.");
        }

        await RecalcSubmissionScoreAsync(_context, submissionId, cancellationToken);
        var statusId = await AdminCabinetSubmissionStatuses.GetOrCreateStatusIdAsync(_context, statusKey, cancellationToken);
        s.SubmissionStatusId = statusId;
        if (setGradedAt)
            s.GradedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPatch("api/admin/homework-submissions/{submissionId:int}/assign-reviewer")]
    public async Task<IActionResult> AssignReviewer(int submissionId, [FromBody] AdminAssignReviewerDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin", "manager") is { } forbidRole)
            return forbidRole;

        var s = await _context.Submissions.FirstOrDefaultAsync(x => x.SubmissionId == submissionId, cancellationToken);
        if (s == null) return NotFound();

        if (dto.ReviewerEmployeeId.HasValue)
        {
            var ok = await _context.Employees.AnyAsync(e => e.EmployeeId == dto.ReviewerEmployeeId.Value && e.DeletedAt == null, cancellationToken);
            if (!ok) return BadRequest("Сотрудник не найден.");
        }

        s.GradedByEmployeeId = dto.ReviewerEmployeeId;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
