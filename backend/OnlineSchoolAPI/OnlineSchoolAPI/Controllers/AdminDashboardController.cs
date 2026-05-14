using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI.Dto;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize]
public sealed class AdminDashboardController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public AdminDashboardController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<AdminDashboardSummaryDto>> Summary(CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;
        var monthStart = new DateTime(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var weekAgo = nowUtc.AddDays(-7);

        // Важно: нельзя выполнять параллельные операции на одном DbContext.
        var activeStudents = await _context.Students.CountAsync(s => s.DeletedAt == null, cancellationToken);
        var newApplications = await _context.TrialApplications.CountAsync(
            a => a.CreatedAt != null && a.CreatedAt >= weekAgo,
            cancellationToken);
        var homework = await _context.Submissions.CountAsync(s =>
                s.SubmittedAt != null && s.GradedAt == null,
            cancellationToken);
        var activeInstances = await _context.CourseInstances.CountAsync(i =>
                i.DeletedAt == null &&
                (i.IsActive == null || i.IsActive == true),
            cancellationToken);

        var paymentsThisMonth = await _context.Payments
            .Where(p => p.PaidAt != null && p.PaidAt >= monthStart)
            .SumAsync(p => (decimal?)p.Amount, cancellationToken);

        var overdueInstallments = await _context.InstallmentPayments.CountAsync(p =>
                p.PaidAt == null && p.DueDate < DateOnly.FromDateTime(DateTime.UtcNow),
            cancellationToken);

        return Ok(new AdminDashboardSummaryDto
        {
            ActiveStudents = activeStudents,
            NewApplications = newApplications,
            HomeworkPendingReview = homework,
            ActiveInstances = activeInstances,
            PaymentsThisMonth = paymentsThisMonth ?? 0m,
            OverdueInstallmentPayments = overdueInstallments
        });
    }

    [HttpGet("recent-applications")]
    public async Task<ActionResult<IReadOnlyList<AdminRecentApplicationDto>>> RecentApplications(CancellationToken cancellationToken)
    {
        var rows = await _context.TrialApplications
            .AsNoTracking()
            .Include(a => a.ApplicationStatus)
            .OrderByDescending(a => a.CreatedAt)
            .Take(20)
            .Select(a => new AdminRecentApplicationDto
            {
                ApplicationId = a.ApplicationId,
                FullName = (a.FirstName + " " + (a.LastName ?? "")).Trim(),
                Phone = a.Phone,
                Email = a.Email,
                Status = a.ApplicationStatus != null ? a.ApplicationStatus.StatusName : "—",
                CreatedAt = a.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(rows);
    }

    [HttpGet("homework-review-queue")]
    public async Task<ActionResult<IReadOnlyList<AdminHomeworkReviewQueueRowDto>>> HomeworkReviewQueue(CancellationToken cancellationToken)
    {
        var rows = await (
            from s in _context.Submissions.AsNoTracking()
            where s.EnrollmentId != null
            join e in _context.Enrollments on s.EnrollmentId equals e.EnrollmentId
            join st in _context.Students on e.StudentId equals st.StudentId
            join a in _context.Assignments on s.AssignmentId equals a.AssignmentId
            join l in _context.Lessons on a.LessonId equals l.LessonId
            join inst in _context.CourseInstances on e.InstanceId equals inst.InstanceId
            join c in _context.Courses on inst.CourseId equals c.CourseId
            join ss in _context.SubmissionStatuses on s.SubmissionStatusId equals ss.StatusId into ssj
            from ss in ssj.DefaultIfEmpty()
            where s.SubmittedAt != null && s.GradedAt == null
            orderby (s.SubmittedAt ?? s.CreatedAt) descending
            select new AdminHomeworkReviewQueueRowDto
            {
                SubmissionId = s.SubmissionId,
                Student = ((st.FirstName ?? "") + " " + (st.LastName ?? "")).Trim(),
                Course = c.Title,
                Lesson = l.Title,
                SubmittedAt = s.SubmittedAt,
                Status = ss != null ? ss.StatusName : ""
            })
            .Take(30)
            .ToListAsync(cancellationToken);

        return Ok(rows);
    }

    [HttpGet("upcoming-instances")]
    public async Task<ActionResult<IReadOnlyList<AdminUpcomingInstanceDto>>> UpcomingInstances(CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var list = await _context.CourseInstances
            .AsNoTracking()
            .Include(i => i.Course)
            .Include(i => i.Enrollments)
            .Where(i =>
                i.DeletedAt == null &&
                i.StartDate >= today &&
                (i.IsActive == null || i.IsActive == true))
            .OrderBy(i => i.StartDate)
            .Take(20)
            .Select(i => new AdminUpcomingInstanceDto
            {
                InstanceId = i.InstanceId,
                Instance = i.InstanceName,
                Course = i.Course.Title,
                StartDate = i.StartDate,
                StudentsCount = i.Enrollments.Count(e => e.CompletedAt == null)
            })
            .ToListAsync(cancellationToken);

        return Ok(list);
    }
}

