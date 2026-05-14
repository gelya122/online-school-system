using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;
using OnlineSchoolAPI.Services;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Authorize]
public sealed class AdminCourseInstancesController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public AdminCourseInstancesController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet("api/admin/course-instances")]
    public async Task<ActionResult<IReadOnlyList<AdminCourseInstanceListRowDto>>> GetList(
        [FromQuery] string? search = null,
        [FromQuery] int? courseId = null,
        [FromQuery] string? status = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin", "manager", "teacher") is { } forbidRole)
            return forbidRole;

        var q = _context.CourseInstances
            .AsNoTracking()
            .Where(i => i.DeletedAt == null);

        var role = AuthClaims.GetRole(User);
        if (string.Equals(role, "teacher", StringComparison.OrdinalIgnoreCase))
        {
            var employeeId = AuthClaims.GetEmployeeId(User);
            if (employeeId == null)
                return StatusCode(403, "Недостаточно прав.");

            q = q.Where(i => i.CourseInstanceStaff.Any(s =>
                s.EmployeeId == employeeId.Value &&
                s.RoleId == CourseInstanceStaffRoleIds.Teacher &&
                s.DeletedAt == null));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            q = q.Where(i => i.InstanceName.ToLower().Contains(s));
        }

        if (courseId.HasValue)
            q = q.Where(i => i.CourseId == courseId.Value);

        if (!string.IsNullOrWhiteSpace(status))
        {
            var st = CourseInstanceStatusMap.NormalizeFilterCode(status);
            q = q.Where(i => i.InstanceStatus.Code.ToLower() == st);
        }

        if (isActive.HasValue)
            q = q.Where(i => (i.IsActive ?? true) == isActive.Value);

        var list = await q
            .OrderByDescending(i => i.StartDate)
            .Select(i => new AdminCourseInstanceListRowDto
            {
                InstanceId = i.InstanceId,
                Title = i.InstanceName,
                CourseId = i.CourseId,
                CourseTitle = i.Course.Title,
                StartDate = i.StartDate,
                EndDate = i.EndDate,
                Status = i.InstanceStatus.Code,
                MaxStudents = i.MaxStudents,
                StudentsCount = i.Enrollments.Count(),
                IsActive = i.IsActive ?? true,
                TeacherFullName = i.CourseInstanceStaff
                    .Where(s => s.RoleId == CourseInstanceStaffRoleIds.Teacher && s.DeletedAt == null)
                    .OrderBy(s => s.StaffAssignmentId)
                    .Select(s => ((s.Employee.LastName ?? "") + " " + (s.Employee.FirstName ?? "")).Trim())
                    .FirstOrDefault(),
                MentorsCount = i.CourseInstanceStaff.Count(s =>
                    s.RoleId == CourseInstanceStaffRoleIds.Mentor && s.DeletedAt == null)
            })
            .ToListAsync(cancellationToken);

        return Ok(list);
    }

    [HttpGet("api/admin/course-instances/{id:int}")]
    public async Task<ActionResult<AdminCourseInstanceDetailsDto>> Get(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;

        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin", "manager", "teacher") is { } forbidRole)
            return forbidRole;

        var instance = await _context.CourseInstances
            .AsNoTracking()
            .Include(i => i.Course)
            .Include(i => i.InstanceStatus)
            .FirstOrDefaultAsync(i => i.InstanceId == id && i.DeletedAt == null, cancellationToken);
        if (instance == null) return NotFound();

        var role = AuthClaims.GetRole(User);
        if (string.Equals(role, "teacher", StringComparison.OrdinalIgnoreCase))
        {
            var employeeId = AuthClaims.GetEmployeeId(User);
            if (employeeId == null)
                return StatusCode(403, "Недостаточно прав.");

            var assigned = await _context.CourseInstanceStaff.AsNoTracking()
                .AnyAsync(s => s.InstanceId == id && s.EmployeeId == employeeId.Value && s.RoleId == CourseInstanceStaffRoleIds.Teacher && s.DeletedAt == null, cancellationToken);
            if (!assigned)
                return StatusCode(403, "Недостаточно прав.");
        }

        var teacherRows = await _context.CourseInstanceStaff
            .AsNoTracking()
            .Where(s => s.InstanceId == id && s.RoleId == CourseInstanceStaffRoleIds.Teacher && s.DeletedAt == null)
            .OrderBy(s => s.StaffAssignmentId)
            .Select(s => new { s.StaffAssignmentId, s.EmployeeId, Last = s.Employee.LastName, First = s.Employee.FirstName, Pat = s.Employee.Patronymic })
            .ToListAsync(cancellationToken);
        var firstTeacherId = teacherRows.Count > 0 ? teacherRows.Min(x => x.StaffAssignmentId) : (int?)null;
        var teachers = teacherRows
            .Select(t => new AdminInstanceTeacherRowDto
            {
                EmployeeId = t.EmployeeId,
                FullName = (t.Last ?? "") + " " + (t.First ?? "") + " " + (t.Pat ?? ""),
                IsMainTeacher = firstTeacherId.HasValue && t.StaffAssignmentId == firstTeacherId.Value
            })
            .ToList();

        var coordinators = await _context.CourseInstanceStaff
            .AsNoTracking()
            .Where(s => s.InstanceId == id && s.RoleId == CourseInstanceStaffRoleIds.Mentor && s.DeletedAt == null)
            .OrderBy(s => s.Employee.LastName)
            .ThenBy(s => s.Employee.FirstName)
            .Select(s => new AdminInstanceCoordinatorRowDto
            {
                EmployeeId = s.EmployeeId,
                FullName = (s.Employee.LastName ?? "") + " " + (s.Employee.FirstName ?? "") + " " + (s.Employee.Patronymic ?? ""),
                IsLead = false
            })
            .ToListAsync(cancellationToken);

        return Ok(new AdminCourseInstanceDetailsDto
        {
            InstanceId = instance.InstanceId,
            CourseId = instance.CourseId,
            CourseTitle = instance.Course?.Title ?? "",
            Title = instance.InstanceName,
            Description = instance.ScheduleDescription,
            StartDate = instance.StartDate,
            EndDate = instance.EndDate,
            EnrollmentStartDate = instance.EnrollmentStartDate,
            EnrollmentEndDate = instance.EnrollmentEndDate,
            MaxStudents = instance.MaxStudents,
            LessonsPerWeek = instance.LessonsPerWeek,
            TotalWeeks = instance.TotalWeeks,
            Timezone = instance.Timezone,
            Status = instance.InstanceStatus.Code,
            IsActive = instance.IsActive ?? true,
            ScheduleRulesJson = instance.ScheduleRulesJson,
            Teachers = teachers,
            Coordinators = coordinators
        });
    }

    [HttpPost("api/admin/course-instances")]
    public async Task<ActionResult<AdminCourseInstanceDetailsDto>> Create([FromBody] AdminCourseInstanceUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin") is { } forbidRole)
            return forbidRole;

        if (dto.CourseId <= 0) return BadRequest("Укажите course_id.");
        if (string.IsNullOrWhiteSpace(dto.Title)) return BadRequest("Укажите название потока.");

        // Новый поток всегда со статусом planned (status_id = 1); клиентское поле Status не учитывается.
        const int statusId = CourseInstanceStatusMap.DefaultStatusId;

        var courseExists = await _context.Courses.AnyAsync(c => c.CourseId == dto.CourseId && c.DeletedAt == null, cancellationToken);
        if (!courseExists) return BadRequest("Курс не найден.");

        if (!dto.EndDate.HasValue)
            return BadRequest("Укажите дату окончания потока.");
        if (dto.EndDate.Value < dto.StartDate)
            return BadRequest("Дата окончания должна быть позже даты начала.");
        if (dto.EnrollmentStartDate.HasValue && dto.EnrollmentEndDate.HasValue &&
            dto.EnrollmentStartDate.Value > dto.EnrollmentEndDate.Value)
            return BadRequest("Дата открытия набора не должна быть позже даты закрытия набора.");
        if (dto.EnrollmentEndDate.HasValue && dto.EnrollmentEndDate.Value > dto.StartDate.AddDays(7))
            return BadRequest(
                "Дата закрытия набора не может быть позже даты начала потока + 7 календарных дней (набор ведётся не дольше недели после старта).");

        var now = DateTime.UtcNow;
        var instance = new CourseInstance
        {
            CourseId = dto.CourseId,
            InstanceName = dto.Title.Trim(),
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            EnrollmentStartDate = dto.EnrollmentStartDate,
            EnrollmentEndDate = dto.EnrollmentEndDate,
            MaxStudents = dto.MaxStudents,
            LessonsPerWeek = dto.LessonsPerWeek,
            TotalWeeks = dto.TotalWeeks,
            Timezone = string.IsNullOrWhiteSpace(dto.Timezone) ? null : dto.Timezone.Trim(),
            StatusId = statusId,
            IsActive = dto.IsActive,
            ScheduleDescription = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description,
            ScheduleRulesJson = string.IsNullOrWhiteSpace(dto.ScheduleRulesJson) ? null : dto.ScheduleRulesJson.Trim(),
            CreatedAt = now,
            UpdatedAt = null,
            DeletedAt = null
        };

        _context.CourseInstances.Add(instance);
        await _context.SaveChangesAsync(cancellationToken);

        return await Get(instance.InstanceId, cancellationToken);
    }

    /// <summary>Создание потока одной транзакцией: экземпляр, преподаватель, наставники, расписание (course_schedule_plan).</summary>
    [HttpPost("api/admin/course-instances/bootstrap")]
    public async Task<ActionResult<AdminCourseInstanceDetailsDto>> CreateBootstrap(
        [FromBody] AdminCourseInstanceBootstrapDto dto,
        CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin") is { } forbidRole)
            return forbidRole;

        var inv = dto.Instance;
        if (string.IsNullOrWhiteSpace(inv.Title))
            return BadRequest("Укажите название потока.");
        if (inv.CourseId <= 0)
            return BadRequest("Укажите курс.");
        if (!inv.EndDate.HasValue)
            return BadRequest("Укажите дату окончания потока.");
        if (inv.EndDate.Value < inv.StartDate)
            return BadRequest("Дата окончания должна быть позже даты начала.");
        if (!inv.MaxStudents.HasValue || inv.MaxStudents.Value <= 0)
            return BadRequest("Максимум студентов должен быть больше 0.");
        if (dto.TeacherEmployeeId <= 0)
            return BadRequest("Выберите преподавателя.");
        var mentors = (dto.MentorEmployeeIds ?? []).Where(x => x > 0).Distinct().ToList();
        if (mentors.Count == 0)
            return BadRequest("Нужен хотя бы один наставник.");
        if (string.IsNullOrWhiteSpace(inv.ScheduleRulesJson) && (inv.LessonsPerWeek ?? 0) <= 0)
            return BadRequest("Добавьте правила расписания (JSON) или укажите lessons_per_week.");

        if (inv.EnrollmentStartDate.HasValue && inv.EnrollmentEndDate.HasValue &&
            inv.EnrollmentStartDate.Value > inv.EnrollmentEndDate.Value)
            return BadRequest("Дата открытия набора не должна быть позже даты закрытия набора.");
        if (inv.EnrollmentEndDate.HasValue && inv.EnrollmentEndDate.Value > inv.StartDate.AddDays(7))
            return BadRequest(
                "Дата закрытия набора не может быть позже даты начала потока + 7 календарных дней (набор ведётся не дольше недели после старта).");

        var courseExists = await _context.Courses.AnyAsync(c => c.CourseId == inv.CourseId && c.DeletedAt == null, cancellationToken);
        if (!courseExists)
            return BadRequest("Курс не найден.");

        foreach (var eid in mentors.Prepend(dto.TeacherEmployeeId).Distinct())
        {
            if (!await _context.Employees.AnyAsync(e => e.EmployeeId == eid, cancellationToken))
                return BadRequest($"Сотрудник не найден (id={eid}).");
        }

        const int statusId = CourseInstanceStatusMap.DefaultStatusId;
        var now = DateTime.UtcNow;

        await using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);
        var newInstanceId = 0;
        try
        {
            var instance = new CourseInstance
            {
                CourseId = inv.CourseId,
                InstanceName = inv.Title.Trim(),
                StartDate = inv.StartDate,
                EndDate = inv.EndDate,
                EnrollmentStartDate = inv.EnrollmentStartDate,
                EnrollmentEndDate = inv.EnrollmentEndDate,
                MaxStudents = inv.MaxStudents,
                LessonsPerWeek = inv.LessonsPerWeek,
                TotalWeeks = inv.TotalWeeks,
                Timezone = string.IsNullOrWhiteSpace(inv.Timezone) ? null : inv.Timezone.Trim(),
                StatusId = statusId,
                IsActive = inv.IsActive,
                ScheduleDescription = string.IsNullOrWhiteSpace(inv.Description) ? null : inv.Description.Trim(),
                ScheduleRulesJson = string.IsNullOrWhiteSpace(inv.ScheduleRulesJson) ? null : inv.ScheduleRulesJson.Trim(),
                CreatedAt = now,
                UpdatedAt = null,
                DeletedAt = null
            };
            _context.CourseInstances.Add(instance);
            await _context.SaveChangesAsync(cancellationToken);

            newInstanceId = instance.InstanceId;

            var teacherErr = await UpsertTeacherAssignmentCoreAsync(newInstanceId, dto.TeacherEmployeeId, cancellationToken);
            if (teacherErr != null)
            {
                await tx.RollbackAsync(cancellationToken);
                return BadRequest(teacherErr);
            }

            foreach (var mentorId in mentors)
            {
                var mentorErr = await UpsertMentorAssignmentCoreAsync(newInstanceId, mentorId, cancellationToken);
                if (mentorErr != null)
                {
                    await tx.RollbackAsync(cancellationToken);
                    return BadRequest(mentorErr);
                }
            }

            var (plans, err) = await CourseInstanceSchedulePlanGenerator.TryBuildPlansAsync(
                _context,
                newInstanceId,
                inv.CourseId,
                inv.StartDate,
                inv.EndDate,
                instance.ScheduleRulesJson,
                inv.LessonsPerWeek,
                new TimeOnly(9, 0),
                cancellationToken);
            if (plans == null)
            {
                await tx.RollbackAsync(cancellationToken);
                return BadRequest(err ?? "Не удалось сгенерировать расписание.");
            }

            _context.CourseSchedulePlans.AddRange(plans);
            await _context.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }

        return await Get(newInstanceId, cancellationToken);
    }

    [HttpPut("api/admin/course-instances/{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] AdminCourseInstanceUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin") is { } forbidRole)
            return forbidRole;

        var instance = await _context.CourseInstances
            .Include(i => i.InstanceStatus)
            .FirstOrDefaultAsync(i => i.InstanceId == id && i.DeletedAt == null, cancellationToken);
        if (instance == null) return NotFound();

        if (dto.CourseId <= 0) return BadRequest("Укажите course_id.");
        if (string.IsNullOrWhiteSpace(dto.Title)) return BadRequest("Укажите название потока.");
        if (string.IsNullOrWhiteSpace(dto.Status)) dto.Status = instance.InstanceStatus.Code;

        var courseExists = await _context.Courses.AnyAsync(c => c.CourseId == dto.CourseId && c.DeletedAt == null, cancellationToken);
        if (!courseExists) return BadRequest("Курс не найден.");

        if (dto.EndDate.HasValue && dto.EndDate.Value < dto.StartDate)
            return BadRequest("Дата окончания должна быть позже даты начала.");
        if (dto.EnrollmentStartDate.HasValue && dto.EnrollmentEndDate.HasValue &&
            dto.EnrollmentStartDate.Value > dto.EnrollmentEndDate.Value)
            return BadRequest("Дата открытия набора не должна быть позже даты закрытия набора.");
        if (dto.EnrollmentEndDate.HasValue && dto.EnrollmentEndDate.Value > dto.StartDate.AddDays(7))
            return BadRequest(
                "Дата закрытия набора не может быть позже даты начала потока + 7 календарных дней (набор ведётся не дольше недели после старта).");

        instance.CourseId = dto.CourseId;
        instance.InstanceName = dto.Title.Trim();
        instance.ScheduleDescription = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description;
        instance.StartDate = dto.StartDate;
        instance.EndDate = dto.EndDate;
        instance.EnrollmentStartDate = dto.EnrollmentStartDate;
        instance.EnrollmentEndDate = dto.EnrollmentEndDate;
        instance.MaxStudents = dto.MaxStudents;
        instance.LessonsPerWeek = dto.LessonsPerWeek;
        instance.TotalWeeks = dto.TotalWeeks;
        instance.Timezone = string.IsNullOrWhiteSpace(dto.Timezone) ? null : dto.Timezone.Trim();
        try
        {
            instance.StatusId = CourseInstanceStatusMap.ResolveStatusId(dto.Status);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }

        instance.IsActive = dto.IsActive;
        instance.ScheduleRulesJson = string.IsNullOrWhiteSpace(dto.ScheduleRulesJson) ? null : dto.ScheduleRulesJson.Trim();
        instance.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPatch("api/admin/course-instances/{id:int}/status")]
    public async Task<IActionResult> PatchStatus(int id, [FromBody] AdminInstanceStatusPatchDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin") is { } forbidRole)
            return forbidRole;

        var instance = await _context.CourseInstances.FirstOrDefaultAsync(i => i.InstanceId == id && i.DeletedAt == null, cancellationToken);
        if (instance == null) return NotFound();

        if (string.IsNullOrWhiteSpace(dto.Status)) return BadRequest("Укажите статус.");

        try
        {
            instance.StatusId = CourseInstanceStatusMap.ResolveStatusId(dto.Status);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        instance.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPatch("api/admin/course-instances/{id:int}/active")]
    public async Task<IActionResult> PatchActive(int id, [FromBody] AdminInstanceIsActivePatchDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin") is { } forbidRole)
            return forbidRole;

        var instance = await _context.CourseInstances.FirstOrDefaultAsync(i => i.InstanceId == id && i.DeletedAt == null, cancellationToken);
        if (instance == null) return NotFound();

        instance.IsActive = dto.IsActive;
        instance.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPatch("api/admin/course-instances/{id:int}/archive")]
    public async Task<IActionResult> Archive(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin") is { } forbidRole)
            return forbidRole;

        var instance = await _context.CourseInstances.FirstOrDefaultAsync(i => i.InstanceId == id && i.DeletedAt == null, cancellationToken);
        if (instance == null) return NotFound();

        instance.IsActive = false;
        instance.StatusId = CourseInstanceStatusMap.ResolveStatusId("cancelled");
        instance.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("api/admin/course-instances/{id:int}/teachers")]
    public async Task<IActionResult> AddTeacher(int id, [FromBody] AdminAssignTeacherDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin") is { } forbidRole)
            return forbidRole;

        var instanceExists = await _context.CourseInstances.AnyAsync(i => i.InstanceId == id && i.DeletedAt == null, cancellationToken);
        if (!instanceExists) return NotFound();

        var employeeExists = await _context.Employees.AnyAsync(e => e.EmployeeId == dto.EmployeeId, cancellationToken);
        if (!employeeExists) return BadRequest("Сотрудник не найден.");

        var err = await UpsertTeacherAssignmentCoreAsync(id, dto.EmployeeId, cancellationToken);
        if (err != null) return BadRequest(err);

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("api/admin/course-instances/{id:int}/teachers/{employeeId:int}")]
    public async Task<IActionResult> RemoveTeacher(int id, int employeeId, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin") is { } forbidRole)
            return forbidRole;

        var row = await _context.CourseInstanceStaff.FirstOrDefaultAsync(
            s => s.InstanceId == id && s.EmployeeId == employeeId && s.RoleId == CourseInstanceStaffRoleIds.Teacher && s.DeletedAt == null,
            cancellationToken);
        if (row == null) return NotFound();

        row.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("api/admin/course-instances/{id:int}/coordinators")]
    public async Task<IActionResult> AddCoordinator(int id, [FromBody] AdminAssignCoordinatorDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin") is { } forbidRole)
            return forbidRole;

        var instanceExists = await _context.CourseInstances.AnyAsync(i => i.InstanceId == id && i.DeletedAt == null, cancellationToken);
        if (!instanceExists) return NotFound();

        var employeeExists = await _context.Employees.AnyAsync(e => e.EmployeeId == dto.EmployeeId, cancellationToken);
        if (!employeeExists) return BadRequest("Сотрудник не найден.");

        var err = await UpsertMentorAssignmentCoreAsync(id, dto.EmployeeId, cancellationToken);
        if (err != null) return BadRequest(err);

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("api/admin/course-instances/{id:int}/coordinators/{employeeId:int}")]
    public async Task<IActionResult> RemoveCoordinator(int id, int employeeId, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin") is { } forbidRole)
            return forbidRole;

        var row = await _context.CourseInstanceStaff.FirstOrDefaultAsync(
            s => s.InstanceId == id && s.EmployeeId == employeeId && s.RoleId == CourseInstanceStaffRoleIds.Mentor && s.DeletedAt == null,
            cancellationToken);
        if (row == null) return NotFound();

        var assignedCount = await _context.Enrollments.CountAsync(
            e => e.InstanceId == id && e.AssignedTeacherId == employeeId,
            cancellationToken);
        if (assignedCount > 0)
        {
            return Conflict(
                $"Нельзя снять наставника: на нём закреплено записей: {assignedCount}. Сначала переназначьте наставника студентам (assigned_mentor_id).");
        }

        row.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    /// <summary>Назначить преподавателя (снять остальных). Без SaveChanges. Ошибка или null при успехе.</summary>
    private async Task<string?> UpsertTeacherAssignmentCoreAsync(int instanceId, int employeeId, CancellationToken cancellationToken)
    {
        var otherTeachers = await _context.CourseInstanceStaff
            .Where(s => s.InstanceId == instanceId && s.RoleId == CourseInstanceStaffRoleIds.Teacher && s.DeletedAt == null && s.EmployeeId != employeeId)
            .ToListAsync(cancellationToken);
        var nowUtc = DateTime.UtcNow;
        foreach (var o in otherTeachers)
            o.DeletedAt = nowUtc;

        var roleId = CourseInstanceStaffRoleIds.Teacher;
        var active = await _context.CourseInstanceStaff.FirstOrDefaultAsync(
            s => s.InstanceId == instanceId && s.EmployeeId == employeeId && s.RoleId == roleId && s.DeletedAt == null,
            cancellationToken);
        if (active != null)
            return "Преподаватель уже назначен.";

        var soft = await _context.CourseInstanceStaff.FirstOrDefaultAsync(
            s => s.InstanceId == instanceId && s.EmployeeId == employeeId && s.RoleId == roleId && s.DeletedAt != null,
            cancellationToken);
        if (soft != null)
        {
            soft.DeletedAt = null;
            soft.AssignedAt = DateTime.UtcNow;
        }
        else
        {
            _context.CourseInstanceStaff.Add(new CourseInstanceStaff
            {
                InstanceId = instanceId,
                EmployeeId = employeeId,
                RoleId = roleId,
                AssignedAt = DateTime.UtcNow,
                DeletedAt = null
            });
        }

        return null;
    }

    /// <summary>Добавить наставника. Без SaveChanges.</summary>
    private async Task<string?> UpsertMentorAssignmentCoreAsync(int instanceId, int employeeId, CancellationToken cancellationToken)
    {
        var roleId = CourseInstanceStaffRoleIds.Mentor;
        var active = await _context.CourseInstanceStaff.FirstOrDefaultAsync(
            s => s.InstanceId == instanceId && s.EmployeeId == employeeId && s.RoleId == roleId && s.DeletedAt == null,
            cancellationToken);
        if (active != null)
            return "Наставник уже назначен.";

        var soft = await _context.CourseInstanceStaff.FirstOrDefaultAsync(
            s => s.InstanceId == instanceId && s.EmployeeId == employeeId && s.RoleId == roleId && s.DeletedAt != null,
            cancellationToken);
        if (soft != null)
        {
            soft.DeletedAt = null;
            soft.AssignedAt = DateTime.UtcNow;
        }
        else
        {
            _context.CourseInstanceStaff.Add(new CourseInstanceStaff
            {
                InstanceId = instanceId,
                EmployeeId = employeeId,
                RoleId = roleId,
                AssignedAt = DateTime.UtcNow,
                DeletedAt = null
            });
        }

        return null;
    }
}

