using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;
using OnlineSchoolAPI.Services;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Authorize]
public sealed class AdminInstanceStudentsController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public AdminInstanceStudentsController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    private async Task SyncStudentActiveByStreamsAsync(int studentId, CancellationToken cancellationToken)
    {
        var hasActiveStreams = await _context.Enrollments
            .AsNoTracking()
            .Where(e => e.StudentId == studentId && e.CompletedAt == null)
            .AnyAsync(e =>
                    e.Instance.DeletedAt == null &&
                    (e.Instance.IsActive == null || e.Instance.IsActive == true),
                cancellationToken);

        var userId = await _context.Students
            .AsNoTracking()
            .Where(s => s.StudentId == studentId && s.DeletedAt == null)
            .Select(s => (int?)s.UserId)
            .FirstOrDefaultAsync(cancellationToken);
        if (userId == null) return;

        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId.Value && u.DeletedAt == null, cancellationToken);
        if (user == null) return;

        var want = hasActiveStreams;
        if (user.IsActive != want)
        {
            user.IsActive = want;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    [HttpGet("api/admin/course-instances/{instanceId:int}/students")]
    public async Task<ActionResult<IReadOnlyList<AdminInstanceStudentRowDto>>> GetStudents(int instanceId, CancellationToken cancellationToken)
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

            var assigned = await _context.CourseInstanceStaff.AsNoTracking()
                .AnyAsync(s => s.InstanceId == instanceId && s.EmployeeId == employeeId.Value && s.RoleId == CourseInstanceStaffRoleIds.Teacher && s.DeletedAt == null, cancellationToken);
            if (!assigned)
                return StatusCode(403, "Недостаточно прав.");
        }

        var instanceExists = await _context.CourseInstances.AnyAsync(i => i.InstanceId == instanceId && i.DeletedAt == null, cancellationToken);
        if (!instanceExists) return NotFound();

        // paid: any order item for this instance and order has PaidAt (or any payment has PaidAt)
        var paidEnrollments = await _context.Enrollments
            .AsNoTracking()
            .Where(e => e.InstanceId == instanceId)
            .Select(e => new
            {
                e.EnrollmentId,
                IsPaid = _context.OrderItems.Any(oi => oi.InstanceId == instanceId && oi.Order.StudentId == e.StudentId &&
                                                     (oi.Order.PaidAt != null || oi.Order.Payments.Any(p => p.PaidAt != null)))
            })
            .ToDictionaryAsync(x => x.EnrollmentId, x => x.IsPaid, cancellationToken);

        // progress %: completed lessons / total lessons of course (rough)
        var courseId = await _context.CourseInstances.AsNoTracking().Where(i => i.InstanceId == instanceId)
            .Select(i => i.CourseId).FirstAsync(cancellationToken);

        var totalLessons = await _context.Lessons
            .AsNoTracking()
            .Where(l => l.Module.CourseId == courseId && l.DeletedAt == null && l.Module.DeletedAt == null)
            .CountAsync(cancellationToken);
        if (totalLessons <= 0) totalLessons = 1;

        var completedByEnrollment = await _context.StudentProgresses
            .AsNoTracking()
            .Where(p => p.IsCompleted == true)
            .GroupBy(p => p.EnrollmentId)
            .Select(g => new { EnrollmentId = g.Key, Completed = g.Count() })
            .ToDictionaryAsync(x => x.EnrollmentId, x => x.Completed, cancellationToken);

        var rows = await _context.Enrollments
            .AsNoTracking()
            .Where(e => e.InstanceId == instanceId)
            .OrderByDescending(e => e.EnrolledAt)
            .Select(e => new AdminInstanceStudentRowDto
            {
                EnrollmentId = e.EnrollmentId,
                StudentId = e.StudentId,
                StudentName = (e.Student.LastName ?? "") + " " + (e.Student.FirstName ?? ""),
                Email = e.Student.User.Email,
                Phone = e.Student.Phone,
                EnrolledAt = e.EnrolledAt,
                Status = e.EnrollmentStatus != null ? e.EnrollmentStatus.StatusName : "",
                AssignedTeacherId = e.AssignedTeacherId,
                AssignedTeacherName = e.AssignedTeacher != null
                    ? ((e.AssignedTeacher.LastName ?? "") + " " + (e.AssignedTeacher.FirstName ?? "")).Trim()
                    : null,
                IsPaid = false, // filled below
                ProgressPercent = 0, // filled below
                FinalScore = e.FinalScore
            })
            .ToListAsync(cancellationToken);

        foreach (var r in rows)
        {
            if (paidEnrollments.TryGetValue(r.EnrollmentId, out var isPaid))
                r.IsPaid = isPaid;

            if (completedByEnrollment.TryGetValue(r.EnrollmentId, out var completed))
                r.ProgressPercent = (int)Math.Round(completed * 100.0 / totalLessons, 0);
        }

        return Ok(rows);
    }

    [HttpPost("api/admin/course-instances/{instanceId:int}/students")]
    public async Task<IActionResult> EnrollStudent(int instanceId, [FromBody] AdminEnrollStudentDto dto, CancellationToken cancellationToken)
        => await EnrollInternalAsync(instanceId, new List<int> { dto.StudentId }, cancellationToken);

    [HttpPost("api/admin/course-instances/{instanceId:int}/students/bulk")]
    public async Task<IActionResult> EnrollStudentsBulk(int instanceId, [FromBody] AdminEnrollStudentsBulkDto dto, CancellationToken cancellationToken)
        => await EnrollInternalAsync(instanceId, dto.StudentIds.Distinct().ToList(), cancellationToken);

    private async Task<IActionResult> EnrollInternalAsync(int instanceId, List<int> studentIds, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin", "manager") is { } forbidRole)
            return forbidRole;

        if (studentIds.Count == 0) return BadRequest("Пустой список студентов.");
        if (studentIds.Any(x => x <= 0)) return BadRequest("Некорректные studentId.");

        var instance = await _context.CourseInstances.FirstOrDefaultAsync(i => i.InstanceId == instanceId && i.DeletedAt == null, cancellationToken);
        if (instance == null) return NotFound();

        // capacity
        if (instance.MaxStudents.HasValue)
        {
            var current = await _context.Enrollments.CountAsync(e => e.InstanceId == instanceId, cancellationToken);
            if (current + studentIds.Count > instance.MaxStudents.Value)
                return BadRequest("Поток переполнен.");
        }

        var existingStudentIds = await _context.Enrollments
            .AsNoTracking()
            .Where(e => e.InstanceId == instanceId && studentIds.Contains(e.StudentId))
            .Select(e => e.StudentId)
            .ToListAsync(cancellationToken);
        if (existingStudentIds.Count > 0)
            return BadRequest($"Студент уже записан: {string.Join(", ", existingStudentIds)}");

        var studentsExist = await _context.Students
            .AsNoTracking()
            .Where(s => studentIds.Contains(s.StudentId) && s.DeletedAt == null)
            .Select(s => s.StudentId)
            .ToListAsync(cancellationToken);
        if (studentsExist.Count != studentIds.Count)
            return BadRequest("Некоторые студенты не найдены.");

        var defaultStatusId = await _context.EnrollmentStatuses
            .AsNoTracking()
            .Where(s => s.StatusName == "active")
            .Select(s => (int?)s.StatusId)
            .FirstOrDefaultAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var enrollments = studentIds.Select(studentId => new Enrollment
        {
            StudentId = studentId,
            InstanceId = instanceId,
            EnrolledAt = now,
            EnrollmentStatusId = defaultStatusId,
            CompletedAt = null,
            FinalScore = null
        }).ToList();

        await AssignMentorsForNewEnrollmentsAsync(instanceId, enrollments, cancellationToken);

        _context.Enrollments.AddRange(enrollments);
        await _context.SaveChangesAsync(cancellationToken);

        // если появились активные потоки — активируем учётку студента
        foreach (var studentId in studentIds)
            await SyncStudentActiveByStreamsAsync(studentId, cancellationToken);

        // Начальный student_progress: как раньше для доступа — планы с release_day_offset <= 0 или первый урок курса.
        var instanceCourseId = instance.CourseId;
        var totalLessonsForCourse = await _context.Lessons
            .AsNoTracking()
            .Where(l => l.Module.CourseId == instanceCourseId && l.DeletedAt == null && l.Module.DeletedAt == null)
            .Select(l => new { l.LessonId, l.LessonOrder, l.Module.ModuleOrder })
            .ToListAsync(cancellationToken);

        var planLessons = await _context.CourseSchedulePlans
            .AsNoTracking()
            .Where(p => p.InstanceId == instanceId && p.ReleaseDayOffset <= 0)
            .Select(p => p.LessonId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var progressRows = new List<StudentProgress>();
        foreach (var e in enrollments)
        {
            var lessonIds = new HashSet<int>();
            if (planLessons.Count > 0)
            {
                foreach (var lid in planLessons)
                    lessonIds.Add(lid);
            }
            else
            {
                var first = totalLessonsForCourse
                    .OrderBy(x => x.ModuleOrder)
                    .ThenBy(x => x.LessonOrder)
                    .Select(x => x.LessonId)
                    .FirstOrDefault();
                if (first != 0)
                    lessonIds.Add(first);
            }

            foreach (var lessonId in lessonIds)
            {
                progressRows.Add(new StudentProgress
                {
                    EnrollmentId = e.EnrollmentId,
                    LessonId = lessonId,
                    IsCompleted = false,
                    CreatedAt = now
                });
            }
        }

        if (progressRows.Count > 0)
        {
            var existingKeys = await _context.StudentProgresses
                .AsNoTracking()
                .Where(p => enrollments.Select(x => x.EnrollmentId).Contains(p.EnrollmentId))
                .Select(p => new { p.EnrollmentId, p.LessonId })
                .ToListAsync(cancellationToken);
            var existsSet = existingKeys.ToHashSet();
            progressRows = progressRows.Where(p => !existsSet.Contains(new { p.EnrollmentId, p.LessonId })).ToList();
            if (progressRows.Count > 0)
            {
                _context.StudentProgresses.AddRange(progressRows);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        return NoContent();
    }

    /// <summary>Назначение assigned_mentor_id (AssignedTeacherId): при нескольких наставниках — минимальная нагрузка по active-записям.</summary>
    private async Task AssignMentorsForNewEnrollmentsAsync(int instanceId, List<Enrollment> enrollments, CancellationToken cancellationToken)
    {
        var mentorIds = await _context.CourseInstanceStaff.AsNoTracking()
            .Where(s => s.InstanceId == instanceId && s.RoleId == CourseInstanceStaffRoleIds.Mentor && s.DeletedAt == null)
            .Select(s => s.EmployeeId)
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);
        if (mentorIds.Count == 0)
            return;

        var activeStatusIds = await _context.EnrollmentStatuses.AsNoTracking()
            .Where(s => s.StatusName == "active")
            .Select(s => s.StatusId)
            .ToListAsync(cancellationToken);

        var loads = new Dictionary<int, int>();
        foreach (var mid in mentorIds)
        {
            var c = await _context.Enrollments.CountAsync(e =>
                    e.InstanceId == instanceId &&
                    e.AssignedTeacherId == mid &&
                    e.EnrollmentStatusId != null &&
                    activeStatusIds.Contains(e.EnrollmentStatusId.Value),
                cancellationToken);
            loads[mid] = c;
        }

        foreach (var e in enrollments)
        {
            var best = loads.OrderBy(kv => kv.Value).ThenBy(kv => kv.Key).First().Key;
            e.AssignedTeacherId = best;
            loads[best]++;
        }
    }

    [HttpPatch("api/admin/enrollments/{enrollmentId:int}/status")]
    public async Task<IActionResult> PatchEnrollmentStatus(int enrollmentId, [FromBody] AdminEnrollmentStatusPatchDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin") is { } forbidRole)
            return forbidRole;

        var enrollment = await _context.Enrollments
            .Include(e => e.EnrollmentStatus)
            .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId, cancellationToken);
        if (enrollment == null) return NotFound();

        if (string.IsNullOrWhiteSpace(dto.Status))
            return BadRequest("Укажите статус.");

        if (string.IsNullOrWhiteSpace(dto.Reason) || dto.Reason.Trim().Length < 3)
            return BadRequest("Укажите причину смены статуса (не короче 3 символов).");

        var statusName = dto.Status.Trim();
        if (string.Equals(statusName, "completed", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Статус completed выставляется автоматически; через админку его выбрать нельзя.");

        var currentName = enrollment.EnrollmentStatus?.StatusName?.Trim() ?? "";
        if (string.Equals(currentName, statusName, StringComparison.OrdinalIgnoreCase))
            return BadRequest("Новый статус совпадает с текущим.");

        var statusId = await _context.EnrollmentStatuses.AsNoTracking()
            .Where(s => s.StatusName == statusName)
            .Select(s => (int?)s.StatusId)
            .FirstOrDefaultAsync(cancellationToken);
        if (statusId == null)
            return BadRequest("Неизвестный статус. Проверьте enrollment_status.");

        enrollment.EnrollmentStatusId = statusId.Value;
        // TODO: enrollment_status_history — записать смену статуса и reason.
        // TODO: реализовать отправку email о смене статуса
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPatch("api/admin/enrollments/{enrollmentId:int}/assigned-teacher")]
    public async Task<IActionResult> PatchEnrollmentAssignedTeacher(int enrollmentId, [FromBody] AdminEnrollmentAssignedTeacherPatchDto dto,
        CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin", "manager") is { } forbidRole)
            return forbidRole;

        var enrollment = await _context.Enrollments.FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId, cancellationToken);
        if (enrollment == null) return NotFound();

        if (dto.AssignedTeacherId.HasValue)
        {
            var empId = dto.AssignedTeacherId.Value;
            if (empId <= 0)
                return BadRequest("Некорректный идентификатор наставника (assignedTeacherId / assigned_mentor_id).");

            var onInstance = await _context.CourseInstanceStaff.AsNoTracking()
                .AnyAsync(s => s.InstanceId == enrollment.InstanceId && s.EmployeeId == empId && s.RoleId == CourseInstanceStaffRoleIds.Mentor && s.DeletedAt == null, cancellationToken);
            if (!onInstance)
                return BadRequest("Наставник должен быть в staff потока с ролью «Наставник» (course_instance_staff, role_id наставника).");

            var exists = await _context.Employees.AsNoTracking().AnyAsync(e => e.EmployeeId == empId, cancellationToken);
            if (!exists)
                return BadRequest("Сотрудник не найден.");
        }

        enrollment.AssignedTeacherId = dto.AssignedTeacherId;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("api/admin/enrollments/{enrollmentId:int}")]
    public async Task<IActionResult> DeleteEnrollment(int enrollmentId, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin") is { } forbidRole)
            return forbidRole;

        var enrollment = await _context.Enrollments.FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId, cancellationToken);
        if (enrollment == null) return NotFound();
        var studentId = enrollment.StudentId;

        var submissions = await _context.Submissions.Where(s => s.EnrollmentId == enrollmentId).ToListAsync(cancellationToken);
        if (submissions.Count > 0)
            _context.Submissions.RemoveRange(submissions);

        var progresses = await _context.StudentProgresses.Where(p => p.EnrollmentId == enrollmentId).ToListAsync(cancellationToken);
        if (progresses.Count > 0)
            _context.StudentProgresses.RemoveRange(progresses);

        _context.Enrollments.Remove(enrollment);
        await _context.SaveChangesAsync(cancellationToken);

        // если активных потоков больше нет — деактивируем учётку
        await SyncStudentActiveByStreamsAsync(studentId, cancellationToken);
        return NoContent();
    }
}

