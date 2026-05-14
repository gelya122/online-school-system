using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Services;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Authorize]
public sealed class AdminAnalyticsController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public AdminAnalyticsController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet("api/admin/analytics/summary")]
    public async Task<ActionResult<AdminAnalyticsSummaryDto>> GetSummary(
        [FromQuery] string? period = null,
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null,
        CancellationToken cancellationToken = default)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;

        var (f, t) = ResolvePeriod(period, from, to);
        var fromDt = f.ToDateTime(TimeOnly.MinValue);
        var toDtExclusive = t.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var newApps = await _context.TrialApplications.AsNoTracking()
            .Where(a => a.CreatedAt >= fromDt && a.CreatedAt < toDtExclusive)
            .CountAsync(cancellationToken);

        var convertedApps = await _context.TrialApplications.AsNoTracking()
            .Where(a => a.CreatedAt >= fromDt && a.CreatedAt < toDtExclusive
                && a.ApplicationStatus != null &&
                (a.ApplicationStatus.StatusName.ToLower().Contains("онверт") ||
                 a.ApplicationStatus.StatusName.ToLower().Contains("convert")))
            .CountAsync(cancellationToken);

        var appToStudent = newApps == 0 ? 0m : (decimal)convertedApps / newApps;

        var paidByConvertedApp = await (
            from a in _context.TrialApplications.AsNoTracking()
            where a.CreatedAt >= fromDt && a.CreatedAt < toDtExclusive && a.Email != null
            join u in _context.Users.AsNoTracking() on a.Email!.ToLower() equals u.Email.ToLower()
            join st in _context.Students.AsNoTracking() on u.UserId equals st.UserId
            join o in _context.AppOrders.AsNoTracking() on st.StudentId equals o.StudentId
            where o.PaidAt != null && o.PaidAt >= fromDt && o.PaidAt < toDtExclusive
            select a.ApplicationId).Distinct().CountAsync(cancellationToken);

        var appToPay = newApps == 0 ? 0m : (decimal)paidByConvertedApp / newApps;

        var revenue = await _context.AppOrders.AsNoTracking()
            .Where(o => o.PaidAt != null && o.PaidAt >= fromDt && o.PaidAt < toDtExclusive)
            .SumAsync(o => (decimal?)o.FinalAmount, cancellationToken) ?? 0m;

        var paidOrders = await _context.AppOrders.AsNoTracking()
            .Where(o => o.PaidAt != null && o.PaidAt >= fromDt && o.PaidAt < toDtExclusive)
            .CountAsync(cancellationToken);

        var unpaidOrders = await _context.AppOrders.AsNoTracking()
            .Where(o => o.CreatedAt >= fromDt && o.CreatedAt < toDtExclusive && o.PaidAt == null)
            .CountAsync(cancellationToken);

        var activeStreams = await _context.CourseInstances.AsNoTracking()
            .Where(i => i.DeletedAt == null && i.IsActive == true)
            .CountAsync(cancellationToken);

        var activeStudents = await _context.Enrollments.AsNoTracking()
            .Where(e => e.Student.DeletedAt == null && e.Instance.DeletedAt == null && e.Instance.IsActive == true)
            .Select(e => e.StudentId)
            .Distinct()
            .CountAsync(cancellationToken);

        // Average progress: snapshot for active enrollments
        var totalLessonsByCourse = await _context.Lessons.AsNoTracking()
            .Where(l => l.DeletedAt == null && l.Module.DeletedAt == null)
            .GroupBy(l => l.Module.CourseId)
            .Select(g => new { CourseId = g.Key, Total = g.Count() })
            .ToDictionaryAsync(x => x.CourseId, x => x.Total, cancellationToken);

        var activeEnrollments = await _context.Enrollments.AsNoTracking()
            .Where(e => e.Student.DeletedAt == null && e.Instance.DeletedAt == null && e.Instance.IsActive == true)
            .Select(e => new { e.EnrollmentId, CourseId = e.Instance.CourseId })
            .ToListAsync(cancellationToken);

        var completedByEnrollment = await _context.StudentProgresses.AsNoTracking()
            .Where(p => p.IsCompleted == true)
            .GroupBy(p => p.EnrollmentId)
            .Select(g => new { EnrollmentId = g.Key, Completed = g.Count() })
            .ToDictionaryAsync(x => x.EnrollmentId, x => x.Completed, cancellationToken);

        var progressPercents = new List<int>(activeEnrollments.Count);
        foreach (var e in activeEnrollments)
        {
            var total = totalLessonsByCourse.TryGetValue(e.CourseId, out var tl) ? tl : 0;
            if (total <= 0) continue;
            var completed = completedByEnrollment.TryGetValue(e.EnrollmentId, out var c) ? c : 0;
            progressPercents.Add((int)Math.Round(completed * 100.0 / total, 0));
        }
        var avgProgress = progressPercents.Count == 0 ? 0 : (int)Math.Round(progressPercents.Average(), 0);

        var avgHwScore = await _context.Submissions.AsNoTracking()
            .Where(s => s.SubmittedAt != null && s.SubmittedAt >= fromDt && s.SubmittedAt < toDtExclusive && s.Score != null)
            .AverageAsync(s => (double?)s.Score, cancellationToken);

        var hwOnReview = await _context.Submissions.AsNoTracking()
            .Where(s => s.SubmittedAt != null && s.SubmittedAt >= fromDt && s.SubmittedAt < toDtExclusive && s.GradedAt == null)
            .CountAsync(cancellationToken);

        var promoUsages = await _context.AppOrders.AsNoTracking()
            .Where(o => o.PromoCodeId != null && o.CreatedAt >= fromDt && o.CreatedAt < toDtExclusive)
            .CountAsync(cancellationToken);

        var promoDiscountTotal = await _context.AppOrders.AsNoTracking()
            .Where(o => o.PromoCodeId != null && o.CreatedAt >= fromDt && o.CreatedAt < toDtExclusive)
            .SumAsync(o => (decimal?)(o.DiscountAmount ?? 0m), cancellationToken) ?? 0m;

        return Ok(new AdminAnalyticsSummaryDto
        {
            From = f,
            To = t,
            NewApplications = newApps,
            ApplicationToStudentConversion = appToStudent,
            ApplicationToPaymentConversion = appToPay,
            Revenue = revenue,
            PaidOrders = paidOrders,
            UnpaidOrders = unpaidOrders,
            ActiveStudents = activeStudents,
            ActiveStreams = activeStreams,
            AverageProgressPercent = avgProgress,
            AverageHomeworkScore = avgHwScore == null ? null : (decimal?)avgHwScore.Value,
            HomeworkOnReview = hwOnReview,
            PromoCodeUsages = promoUsages,
            PromoCodeDiscountTotal = promoDiscountTotal
        });
    }

    [HttpGet("api/admin/analytics/applications")]
    public async Task<ActionResult<IReadOnlyList<AdminDateCountPointDto>>> ApplicationsByDay(
        [FromQuery] string? period = null, [FromQuery] DateOnly? from = null, [FromQuery] DateOnly? to = null,
        CancellationToken cancellationToken = default)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        var (f, t) = ResolvePeriod(period, from, to);
        var fromDt = f.ToDateTime(TimeOnly.MinValue);
        var toEx = t.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var rows = await _context.TrialApplications.AsNoTracking()
            .Where(a => a.CreatedAt >= fromDt && a.CreatedAt < toEx)
            .GroupBy(a => DateOnly.FromDateTime(a.CreatedAt!.Value))
            .OrderBy(g => g.Key)
            .Select(g => new AdminDateCountPointDto { Date = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return Ok(FillDateRange(f, t, rows, x => x.Count));
    }

    [HttpGet("api/admin/analytics/revenue")]
    public async Task<ActionResult<IReadOnlyList<AdminDateAmountPointDto>>> RevenueByDay(
        [FromQuery] string? period = null, [FromQuery] DateOnly? from = null, [FromQuery] DateOnly? to = null,
        CancellationToken cancellationToken = default)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        var (f, t) = ResolvePeriod(period, from, to);
        var fromDt = f.ToDateTime(TimeOnly.MinValue);
        var toEx = t.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var rows = await _context.AppOrders.AsNoTracking()
            .Where(o => o.PaidAt != null && o.PaidAt >= fromDt && o.PaidAt < toEx)
            .GroupBy(o => DateOnly.FromDateTime(o.PaidAt!.Value))
            .OrderBy(g => g.Key)
            .Select(g => new AdminDateAmountPointDto { Date = g.Key, Amount = g.Sum(x => x.FinalAmount) })
            .ToListAsync(cancellationToken);

        return Ok(FillDateRangeAmount(f, t, rows));
    }

    [HttpGet("api/admin/analytics/orders")]
    public async Task<ActionResult<IReadOnlyList<AdminNameCountPointDto>>> OrdersByStatus(
        [FromQuery] string? period = null, [FromQuery] DateOnly? from = null, [FromQuery] DateOnly? to = null,
        CancellationToken cancellationToken = default)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        var (f, t) = ResolvePeriod(period, from, to);
        var fromDt = f.ToDateTime(TimeOnly.MinValue);
        var toEx = t.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var rows = await _context.Payments.AsNoTracking()
            .Include(p => p.PaymentStatus)
            .Where(p => p.PaidAt != null && p.PaidAt >= fromDt && p.PaidAt < toEx)
            .GroupBy(p => p.PaymentStatus != null ? p.PaymentStatus.StatusName : "unknown")
            .Select(g => new AdminNameCountPointDto { Name = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        return Ok(rows);
    }

    [HttpGet("api/admin/analytics/student-progress")]
    public async Task<ActionResult<IReadOnlyList<AdminNamePercentPointDto>>> ProgressByCourse(CancellationToken cancellationToken = default)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;

        var totalLessonsByCourse = await _context.Lessons.AsNoTracking()
            .Where(l => l.DeletedAt == null && l.Module.DeletedAt == null)
            .GroupBy(l => l.Module.CourseId)
            .Select(g => new { CourseId = g.Key, Total = g.Count() })
            .ToDictionaryAsync(x => x.CourseId, x => x.Total, cancellationToken);

        var enrollments = await _context.Enrollments.AsNoTracking()
            .Where(e => e.Student.DeletedAt == null && e.Instance.DeletedAt == null && e.Instance.IsActive == true)
            .Select(e => new { e.EnrollmentId, CourseId = e.Instance.CourseId, CourseTitle = e.Instance.Course.Title })
            .ToListAsync(cancellationToken);

        var completedByEnrollment = await _context.StudentProgresses.AsNoTracking()
            .Where(p => p.IsCompleted == true)
            .GroupBy(p => p.EnrollmentId)
            .Select(g => new { EnrollmentId = g.Key, Completed = g.Count() })
            .ToDictionaryAsync(x => x.EnrollmentId, x => x.Completed, cancellationToken);

        var agg = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);
        foreach (var e in enrollments)
        {
            var total = totalLessonsByCourse.TryGetValue(e.CourseId, out var tl) ? tl : 0;
            if (total <= 0) continue;
            var completed = completedByEnrollment.TryGetValue(e.EnrollmentId, out var c) ? c : 0;
            var pct = (int)Math.Round(completed * 100.0 / total, 0);
            if (!agg.TryGetValue(e.CourseTitle, out var list)) agg[e.CourseTitle] = list = [];
            list.Add(pct);
        }

        var rows = agg
            .Select(x => new AdminNamePercentPointDto { Name = x.Key, Percent = (int)Math.Round(x.Value.Average(), 0) })
            .OrderByDescending(x => x.Percent)
            .ToList();

        return Ok(rows);
    }

    [HttpGet("api/admin/analytics/homework")]
    public async Task<ActionResult<IReadOnlyList<AdminNameCountPointDto>>> HomeworkOnReviewByCourse(
        [FromQuery] string? period = null, [FromQuery] DateOnly? from = null, [FromQuery] DateOnly? to = null,
        CancellationToken cancellationToken = default)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        var (f, t) = ResolvePeriod(period, from, to);
        var fromDt = f.ToDateTime(TimeOnly.MinValue);
        var toEx = t.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var rows = await (
            from s in _context.Submissions.AsNoTracking()
            where s.SubmittedAt != null && s.SubmittedAt >= fromDt && s.SubmittedAt < toEx && s.GradedAt == null && s.EnrollmentId != null
            join e in _context.Enrollments on s.EnrollmentId equals e.EnrollmentId
            join inst in _context.CourseInstances on e.InstanceId equals inst.InstanceId
            join c in _context.Courses on inst.CourseId equals c.CourseId
            group c by c.Title into g
            select new AdminNameCountPointDto { Name = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        return Ok(rows);
    }

    [HttpGet("api/admin/analytics/popular-courses")]
    public async Task<ActionResult<IReadOnlyList<AdminNameCountPointDto>>> PopularCourses(
        [FromQuery] string? period = null, [FromQuery] DateOnly? from = null, [FromQuery] DateOnly? to = null,
        CancellationToken cancellationToken = default)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        var (f, t) = ResolvePeriod(period, from, to);
        var fromDt = f.ToDateTime(TimeOnly.MinValue);
        var toEx = t.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var rows = await (
            from oi in _context.OrderItems.AsNoTracking()
            join o in _context.AppOrders.AsNoTracking() on oi.OrderId equals o.OrderId
            where o.PaidAt != null && o.PaidAt >= fromDt && o.PaidAt < toEx
            join c in _context.Courses.AsNoTracking() on oi.CourseId equals c.CourseId
            group oi by c.Title into g
            select new AdminNameCountPointDto { Name = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(25)
            .ToListAsync(cancellationToken);

        return Ok(rows);
    }

    [HttpGet("api/admin/analytics/promo-codes")]
    public async Task<ActionResult<IReadOnlyList<AdminPromoCodeUsageAggDto>>> PromoCodes(
        [FromQuery] string? period = null, [FromQuery] DateOnly? from = null, [FromQuery] DateOnly? to = null,
        CancellationToken cancellationToken = default)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        var (f, t) = ResolvePeriod(period, from, to);
        var fromDt = f.ToDateTime(TimeOnly.MinValue);
        var toEx = t.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var rows = await (
            from o in _context.AppOrders.AsNoTracking()
            where o.PromoCodeId != null && o.CreatedAt >= fromDt && o.CreatedAt < toEx
            join pc in _context.PromoCodes.AsNoTracking() on o.PromoCodeId equals pc.PromoCodeId
            group o by pc.Code into g
            select new AdminPromoCodeUsageAggDto
            {
                Code = g.Key,
                Uses = g.Count(),
                DiscountTotal = g.Sum(x => x.DiscountAmount ?? 0m)
            })
            .OrderByDescending(x => x.Uses)
            .Take(50)
            .ToListAsync(cancellationToken);

        return Ok(rows);
    }

    private static (DateOnly From, DateOnly To) ResolvePeriod(string? period, DateOnly? from, DateOnly? to)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var p = (period ?? "").Trim().ToLowerInvariant();
        return p switch
        {
            "today" or "сегодня" => (today, today),
            "week" or "неделя" => (today.AddDays(-6), today),
            "month" or "месяц" => (today.AddDays(-29), today),
            "quarter" or "квартал" => (today.AddDays(-89), today),
            "year" or "год" => (today.AddDays(-364), today),
            "custom" or "произвольный" when from.HasValue && to.HasValue => (from.Value, to.Value),
            _ => (today.AddDays(-29), today)
        };
    }

    private static List<AdminDateCountPointDto> FillDateRange(DateOnly from, DateOnly to, List<AdminDateCountPointDto> rows, Func<AdminDateCountPointDto, int> _)
    {
        var map = rows.ToDictionary(x => x.Date, x => x.Count);
        var result = new List<AdminDateCountPointDto>();
        for (var d = from; d <= to; d = d.AddDays(1))
        {
            result.Add(new AdminDateCountPointDto { Date = d, Count = map.TryGetValue(d, out var v) ? v : 0 });
        }
        return result;
    }

    private static List<AdminDateAmountPointDto> FillDateRangeAmount(DateOnly from, DateOnly to, List<AdminDateAmountPointDto> rows)
    {
        var map = rows.ToDictionary(x => x.Date, x => x.Amount);
        var result = new List<AdminDateAmountPointDto>();
        for (var d = from; d <= to; d = d.AddDays(1))
        {
            result.Add(new AdminDateAmountPointDto { Date = d, Amount = map.TryGetValue(d, out var v) ? v : 0m });
        }
        return result;
    }
}

