using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;
using OnlineSchoolAPI.Services;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Authorize]
public sealed class AdminApplicationsController : ControllerBase
{
    private const int StudentRoleId = 7;
    private readonly OnlineSchoolDbContext _context;

    public AdminApplicationsController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet("api/admin/applications/statuses")]
    public async Task<ActionResult<IReadOnlyList<object>>> GetStatuses(CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin", "manager") is { } forbidRole)
            return forbidRole;

        var list = await _context.ApplicationStatuses
            .AsNoTracking()
            .OrderBy(s => s.StatusId)
            .Select(s => new { s.StatusId, s.StatusName })
            .ToListAsync(cancellationToken);
        return Ok(list);
    }

    [HttpGet("api/admin/applications")]
    public async Task<ActionResult<IReadOnlyList<AdminApplicationListRowDto>>> GetList(
        [FromQuery] string? search = null,
        [FromQuery] bool searchLastNameOnly = false,
        [FromQuery] int? statusId = null,
        [FromQuery] int? managerId = null,
        [FromQuery] int? subjectId = null,
        [FromQuery] string? createdFrom = null,
        [FromQuery] string? createdTo = null,
        [FromQuery] string? scope = null,
        CancellationToken cancellationToken = default)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin", "manager") is { } forbidRole)
            return forbidRole;

        IQueryable<TrialApplication> q = _context.TrialApplications
            .AsNoTracking()
            .Include(a => a.ApplicationStatus)
            .Include(a => a.AssignedManager);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            if (searchLastNameOnly)
            {
                q = q.Where(a =>
                    a.LastName != null && a.LastName.ToLower().Contains(s));
            }
            else
            {
                q = q.Where(a =>
                    (a.FirstName + " " + (a.LastName ?? "")).ToLower().Contains(s) ||
                    (a.Phone ?? "").ToLower().Contains(s) ||
                    (a.Email ?? "").ToLower().Contains(s));
            }
        }

        if (statusId.HasValue)
            q = q.Where(a => a.ApplicationStatusId == statusId.Value);

        if (managerId.HasValue)
            q = q.Where(a => a.AssignedManagerId == managerId.Value);

        if (subjectId.HasValue)
        {
            var needle = await _context.Subjects.AsNoTracking()
                .Where(x => x.SubjectId == subjectId.Value)
                .Select(x => x.SubjectName)
                .FirstOrDefaultAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(needle))
            {
                var n = needle.Trim();
                q = q.Where(a => a.SelectedSubjects != null && a.SelectedSubjects.Contains(n));
            }
        }

        if (DateTime.TryParse(createdFrom, out var fromDt))
            q = q.Where(a => a.CreatedAt >= fromDt);
        if (DateTime.TryParse(createdTo, out var toDt))
            q = q.Where(a => a.CreatedAt <= toDt.Date.AddDays(1).AddTicks(-1));

        var myEmpId = AuthClaims.GetEmployeeId(User);
        if (!string.IsNullOrWhiteSpace(scope))
        {
            var sc = scope.Trim().ToLowerInvariant();
            switch (sc)
            {
                case "new":
                    q = q.Where(a =>
                        a.AssignedManagerId == null &&
                        a.ApplicationStatus != null &&
                        (a.ApplicationStatus.StatusName.Trim().ToLower() == "новая" || a.ApplicationStatusId == 1));
                    break;
                case "mine":
                    if (myEmpId is > 0)
                        q = q.Where(a => a.AssignedManagerId == myEmpId.Value);
                    else
                        q = q.Where(a => false);
                    break;
                case "in_progress":
                    q = q.Where(a =>
                        a.ApplicationStatus != null &&
                        (a.ApplicationStatus.StatusName.ToLower().Contains("работ") ||
                         a.ApplicationStatus.StatusName.ToLower().Contains("work")));
                    break;
                case "completed":
                    q = q.Where(a =>
                        a.ApplicationStatus != null &&
                        (a.ApplicationStatus.StatusName.ToLower().Contains("онверт") ||
                         a.ApplicationStatus.StatusName.ToLower().Contains("convert") ||
                         a.ApplicationStatus.StatusName.ToLower().Contains("отказ") ||
                         a.ApplicationStatus.StatusName.ToLower().Contains("рхив")));
                    break;
            }
        }

        var list = await q
            .OrderByDescending(a => a.CreatedAt)
            .Take(500)
            .Select(a => new AdminApplicationListRowDto
            {
                ApplicationId = a.ApplicationId,
                FirstName = a.FirstName,
                LastName = a.LastName,
                Phone = a.Phone,
                Email = a.Email,
                ClassNumber = a.ClassNumber,
                StatusId = a.ApplicationStatusId,
                StatusName = a.ApplicationStatus != null ? a.ApplicationStatus.StatusName : null,
                ManagerId = a.AssignedManagerId,
                ManagerName = a.AssignedManager == null ? null : (a.AssignedManager.FirstName + " " + a.AssignedManager.LastName).Trim(),
                CreatedAt = a.CreatedAt,
                ContactedAt = a.ContactedAt,
                Subjects = a.SelectedSubjects ?? ""
            })
            .ToListAsync(cancellationToken);

        return Ok(list);
    }

    [HttpGet("api/admin/applications/{id:int}")]
    public async Task<ActionResult<AdminApplicationDetailsDto>> Get(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin", "manager") is { } forbidRole)
            return forbidRole;

        var app = await _context.TrialApplications
            .AsNoTracking()
            .Include(a => a.ApplicationStatus)
            .Include(a => a.AssignedManager)
            .FirstOrDefaultAsync(a => a.ApplicationId == id, cancellationToken);
        if (app == null) return NotFound();

        var subjects = await SubjectsFromCsvAsync(app.SelectedSubjects, cancellationToken);
        var comments = ApplicationAuditLog.ParseComments(app.ManagerComment);
        var history = ApplicationAuditLog.ParseStatusHistory(app.ManagerComment);

        return Ok(new AdminApplicationDetailsDto
        {
            ApplicationId = app.ApplicationId,
            FirstName = app.FirstName,
            LastName = app.LastName,
            Phone = app.Phone,
            Email = app.Email,
            ClassNumber = app.ClassNumber,
            ManagerComment = ApplicationAuditLog.SplitAudit(app.ManagerComment).UserNotes,
            StudentId = ApplicationAuditLog.TryParseConvertedStudentId(app.ManagerComment),
            CreatedAt = app.CreatedAt,
            ContactedAt = app.ContactedAt,
            StatusId = app.ApplicationStatusId,
            StatusName = app.ApplicationStatus?.StatusName,
            ManagerId = app.AssignedManagerId,
            ManagerName = app.AssignedManager == null ? null : (app.AssignedManager.FirstName + " " + app.AssignedManager.LastName).Trim(),
            Subjects = subjects,
            Comments = comments,
            StatusHistory = history
        });
    }

    [HttpPost("api/admin/applications")]
    public async Task<ActionResult<AdminApplicationDetailsDto>> Create([FromBody] AdminApplicationUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin", "manager") is { } forbidRole)
            return forbidRole;

        if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName))
            return BadRequest("Укажите имя и фамилию.");
        if (string.IsNullOrWhiteSpace(dto.Phone))
            return BadRequest("Укажите телефон.");
        if (!string.IsNullOrWhiteSpace(dto.Email) && !EmailValidator.IsValid(dto.Email))
            return BadRequest("Введите корректный email.");

        if (dto.StatusId.HasValue)
        {
            var ok = await _context.ApplicationStatuses.AnyAsync(s => s.StatusId == dto.StatusId.Value, cancellationToken);
            if (!ok) return BadRequest("Статус не найден.");
        }

        if (dto.AssignedManagerId.HasValue)
        {
            var ok = await _context.Employees.AnyAsync(e => e.EmployeeId == dto.AssignedManagerId.Value && e.DeletedAt == null, cancellationToken);
            if (!ok) return BadRequest("Менеджер не найден.");
        }

        var selectedCsv = await BuildSelectedSubjectsCsvAsync(dto.SubjectIds, cancellationToken);
        if (dto.SubjectIds.Count > 0 && selectedCsv == null)
            return BadRequest("Некоторые предметы не найдены.");

        await using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var now = DateTime.UtcNow;
            var mc = ApplicationAuditLog.Merge(dto.ManagerComment?.Trim() ?? "", Array.Empty<string>());
            var app = new TrialApplication
            {
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Phone = dto.Phone.Trim(),
                Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim(),
                ClassNumber = dto.ClassNumber,
                SelectedSubjects = selectedCsv,
                ManagerComment = mc,
                ApplicationStatusId = dto.StatusId ?? 1,
                AssignedManagerId = dto.AssignedManagerId,
                CreatedAt = now
            };
            _context.TrialApplications.Add(app);
            await _context.SaveChangesAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(dto.Comment))
            {
                app.ManagerComment = ApplicationAuditLog.Append(app.ManagerComment, AuthClaims.GetEmployeeId(User), "note", dto.Comment.Trim());
                await _context.SaveChangesAsync(cancellationToken);
            }

            await tx.CommitAsync(cancellationToken);
            return await Get(app.ApplicationId, cancellationToken);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    [HttpPut("api/admin/applications/{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] AdminApplicationUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin", "manager") is { } forbidRole)
            return forbidRole;

        var app = await _context.TrialApplications.FirstOrDefaultAsync(a => a.ApplicationId == id, cancellationToken);
        if (app == null) return NotFound();

        if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName))
            return BadRequest("Укажите имя и фамилию.");
        if (string.IsNullOrWhiteSpace(dto.Phone))
            return BadRequest("Укажите телефон.");
        if (!string.IsNullOrWhiteSpace(dto.Email) && !EmailValidator.IsValid(dto.Email))
            return BadRequest("Введите корректный email.");

        if (dto.AssignedManagerId.HasValue)
        {
            var ok = await _context.Employees.AnyAsync(e => e.EmployeeId == dto.AssignedManagerId.Value && e.DeletedAt == null, cancellationToken);
            if (!ok) return BadRequest("Менеджер не найден.");
        }

        var selectedCsv = await BuildSelectedSubjectsCsvAsync(dto.SubjectIds, cancellationToken);
        if (dto.SubjectIds.Count > 0 && selectedCsv == null)
            return BadRequest("Некоторые предметы не найдены.");

        await using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var (_, audit) = ApplicationAuditLog.SplitAudit(app.ManagerComment);
            app.FirstName = dto.FirstName.Trim();
            app.LastName = dto.LastName.Trim();
            app.Phone = dto.Phone.Trim();
            app.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
            app.ClassNumber = dto.ClassNumber;
            app.SelectedSubjects = selectedCsv ?? app.SelectedSubjects;
            app.ManagerComment = ApplicationAuditLog.Merge(dto.ManagerComment?.Trim() ?? "", audit);
            if (dto.StatusId.HasValue) app.ApplicationStatusId = dto.StatusId.Value;
            app.AssignedManagerId = dto.AssignedManagerId;

            if (!string.IsNullOrWhiteSpace(dto.Comment))
                app.ManagerComment = ApplicationAuditLog.Append(app.ManagerComment, AuthClaims.GetEmployeeId(User), "note", dto.Comment.Trim());

            if (dto.StatusId.HasValue && SetsContactedAtOnStatusChange(dto.StatusId.Value))
                app.ContactedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return NoContent();
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    [HttpPatch("api/admin/applications/{id:int}/status")]
    public async Task<IActionResult> PatchStatus(int id, [FromBody] AdminApplicationStatusPatchDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin", "manager") is { } forbidRole)
            return forbidRole;

        var app = await _context.TrialApplications
            .Include(a => a.ApplicationStatus)
            .FirstOrDefaultAsync(a => a.ApplicationId == id, cancellationToken);
        if (app == null) return NotFound();

        var status = await _context.ApplicationStatuses.AsNoTracking().FirstOrDefaultAsync(s => s.StatusId == dto.StatusId, cancellationToken);
        if (status == null) return BadRequest("Статус не найден.");

        var inProgressId = await ResolveInProgressStatusIdAsync(cancellationToken);
        if (inProgressId.HasValue && status.StatusId == inProgressId.Value && app.AssignedManagerId == null)
        {
            var emp = AuthClaims.GetEmployeeId(User);
            if (!emp.HasValue)
                return BadRequest("Для статуса «В работе» назначьте менеджера или укажите employeeId в профиле.");
            app.AssignedManagerId = emp.Value;
        }

        app.ApplicationStatusId = status.StatusId;
        var empId = AuthClaims.GetEmployeeId(User);
        app.ManagerComment = ApplicationAuditLog.Append(app.ManagerComment, empId, "status", status.StatusName);

        if (!string.IsNullOrWhiteSpace(dto.ReasonComment))
            app.ManagerComment = ApplicationAuditLog.Append(app.ManagerComment, empId, "note", dto.ReasonComment.Trim());

        if (SetsContactedAtOnStatusChange(status.StatusId))
            app.ContactedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    /// <summary>Статусы «Проконсультирован», «Отказ», «Клиент» (script6: status_id 3, 4, 5).</summary>
    private static bool SetsContactedAtOnStatusChange(int statusId)
        => statusId is 3 or 4 or 5;

    [HttpPatch("api/admin/applications/{id:int}/manager")]
    public async Task<IActionResult> PatchManager(int id, [FromBody] AdminApplicationManagerPatchDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin", "manager") is { } forbidRole)
            return forbidRole;

        var app = await _context.TrialApplications
            .Include(a => a.ApplicationStatus)
            .FirstOrDefaultAsync(a => a.ApplicationId == id, cancellationToken);
        if (app == null) return NotFound();

        if (dto.ManagerId.HasValue)
        {
            var ok = await _context.Employees.AnyAsync(e => e.EmployeeId == dto.ManagerId.Value && e.DeletedAt == null, cancellationToken);
            if (!ok) return BadRequest("Менеджер не найден.");
        }

        app.AssignedManagerId = dto.ManagerId;

        if (IsNewLikeStatus(app.ApplicationStatus, app.ApplicationStatusId) && dto.ManagerId is > 0)
        {
            var inProg = await ResolveInProgressStatusIdAsync(cancellationToken);
            if (inProg.HasValue)
                app.ApplicationStatusId = inProg.Value;
        }

        var empId = AuthClaims.GetEmployeeId(User);
        if (!string.IsNullOrWhiteSpace(dto.ReasonComment))
            app.ManagerComment = ApplicationAuditLog.Append(app.ManagerComment, empId, "note", dto.ReasonComment.Trim());

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("api/admin/applications/{id:int}/claim")]
    public async Task<IActionResult> Claim(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin", "manager") is { } forbidRole)
            return forbidRole;

        var currentEmp = AuthClaims.GetEmployeeId(User);
        if (!currentEmp.HasValue)
            return BadRequest("В токене нет employeeId. Обновите профиль сотрудника.");

        var app = await _context.TrialApplications
            .Include(a => a.ApplicationStatus)
            .FirstOrDefaultAsync(a => a.ApplicationId == id, cancellationToken);
        if (app == null) return NotFound();

        if (app.AssignedManagerId != null)
            return BadRequest("Заявка уже назначена другому менеджеру.");

        if (!IsNewLikeStatus(app.ApplicationStatus, app.ApplicationStatusId))
            return BadRequest("Взять в работу можно только заявку в статусе «Новая».");

        app.AssignedManagerId = currentEmp.Value;
        var inProg = await ResolveInProgressStatusIdAsync(cancellationToken);
        if (inProg.HasValue)
            app.ApplicationStatusId = inProg.Value;

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPatch("api/admin/applications/{id:int}/contact")]
    public async Task<IActionResult> PatchContact(int id, [FromBody] AdminApplicationContactPatchDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin", "manager") is { } forbidRole)
            return forbidRole;

        var app = await _context.TrialApplications.FirstOrDefaultAsync(a => a.ApplicationId == id, cancellationToken);
        if (app == null) return NotFound();

        var now = DateTime.UtcNow;
        app.ContactedAt = now;
        var text = string.IsNullOrWhiteSpace(dto.Comment) ? "Связались с клиентом" : dto.Comment.Trim();
        app.ManagerComment = ApplicationAuditLog.Append(app.ManagerComment, AuthClaims.GetEmployeeId(User), "contact", text);

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("api/admin/applications/{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin", "manager") is { } forbidRole)
            return forbidRole;

        var app = await _context.TrialApplications.FirstOrDefaultAsync(a => a.ApplicationId == id, cancellationToken);
        if (app == null) return NotFound();

        _context.TrialApplications.Remove(app);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static bool IsNewLikeStatus(ApplicationStatus? st, int? statusId)
    {
        if (st != null && st.StatusName.Trim().Equals("Новая", StringComparison.OrdinalIgnoreCase))
            return true;
        return statusId == 1;
    }

    private async Task<int?> ResolveInProgressStatusIdAsync(CancellationToken cancellationToken)
    {
        var id = await _context.ApplicationStatuses.AsNoTracking()
            .OrderBy(s => s.StatusId)
            .Where(s => s.StatusName.ToLower().Contains("работ"))
            .Select(s => (int?)s.StatusId)
            .FirstOrDefaultAsync(cancellationToken);
        return id;
    }

    [HttpPost("api/admin/applications/{id:int}/comments")]
    public async Task<ActionResult<AdminApplicationCommentDto>> AddComment(int id, [FromBody] AdminApplicationCommentCreateDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin", "manager") is { } forbidRole)
            return forbidRole;

        if (string.IsNullOrWhiteSpace(dto.CommentText))
            return BadRequest("Введите комментарий.");

        var app = await _context.TrialApplications.FirstOrDefaultAsync(a => a.ApplicationId == id, cancellationToken);
        if (app == null) return NotFound();

        var employeeId = AuthClaims.GetEmployeeId(User);
        string? employeeName = null;
        if (employeeId.HasValue)
        {
            employeeName = await _context.Employees.AsNoTracking()
                .Where(e => e.EmployeeId == employeeId.Value)
                .Select(e => (e.FirstName + " " + e.LastName).Trim())
                .FirstOrDefaultAsync(cancellationToken);
        }

        app.ManagerComment = ApplicationAuditLog.Append(app.ManagerComment, employeeId, "note", dto.CommentText.Trim());
        await _context.SaveChangesAsync(cancellationToken);

        var parsed = ApplicationAuditLog.ParseComments(app.ManagerComment);
        var last = parsed.LastOrDefault();
        return Ok(last ?? new AdminApplicationCommentDto
        {
            CommentId = 0,
            CommentText = dto.CommentText.Trim(),
            EmployeeId = employeeId,
            EmployeeName = string.IsNullOrWhiteSpace(employeeName) ? null : employeeName,
            CreatedAt = DateTime.UtcNow
        });
    }

    [HttpPost("api/admin/applications/{id:int}/convert-to-student")]
    public async Task<ActionResult<AdminConvertApplicationToStudentResultDto>> ConvertToStudent(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin", "manager") is { } forbidRole)
            return forbidRole;

        var app = await _context.TrialApplications.FirstOrDefaultAsync(a => a.ApplicationId == id, cancellationToken);
        if (app == null) return NotFound();

        if (ApplicationAuditLog.TryParseConvertedStudentId(app.ManagerComment).HasValue)
            return BadRequest("По этой заявке уже создан студент.");

        if (string.IsNullOrWhiteSpace(app.Email))
            return BadRequest("Нельзя создать студента без email.");

        var emailNorm = app.Email.Trim();
        if (!EmailValidator.IsValid(emailNorm))
            return BadRequest("Некорректный email в заявке.");

        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == emailNorm.ToLower(), cancellationToken))
            return BadRequest("Пользователь с таким email уже существует.");

        var tempPassword = "Temp" + Guid.NewGuid().ToString("N")[..8] + "!";

        await using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var now = DateTime.UtcNow;
            var user = new User
            {
                Email = emailNorm,
                PasswordHash = PasswordHasher.Hash(tempPassword),
                RoleId = StudentRoleId,
                IsEmailConfirmed = true,
                IsActive = true,
                CreatedAt = now
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            var student = new Student
            {
                UserId = user.UserId,
                FirstName = app.FirstName.Trim(),
                LastName = (app.LastName ?? "").Trim(),
                Phone = app.Phone,
                ClassNumber = app.ClassNumber ?? 0,
                CreatedAt = now
            };
            _context.Students.Add(student);
            await _context.SaveChangesAsync(cancellationToken);

            app.ManagerComment = ApplicationAuditLog.Append(app.ManagerComment, AuthClaims.GetEmployeeId(User), "convert", student.StudentId.ToString());

            var convertedStatus = await _context.ApplicationStatuses.AsNoTracking()
                .FirstOrDefaultAsync(s => s.StatusName != null && s.StatusName.ToLower() == "converted", cancellationToken);
            if (convertedStatus != null)
            {
                app.ApplicationStatusId = convertedStatus.StatusId;
                app.ManagerComment = ApplicationAuditLog.Append(app.ManagerComment, AuthClaims.GetEmployeeId(User), "status", convertedStatus.StatusName);
            }

            await _context.SaveChangesAsync(cancellationToken);

            await tx.CommitAsync(cancellationToken);
            return Ok(new AdminConvertApplicationToStudentResultDto { StudentId = student.StudentId, TemporaryPassword = tempPassword });
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task<string?> BuildSelectedSubjectsCsvAsync(IReadOnlyCollection<int> subjectIds, CancellationToken cancellationToken)
    {
        if (subjectIds == null || subjectIds.Count == 0)
            return null;
        var distinct = subjectIds.Distinct().ToList();
        var names = await _context.Subjects.AsNoTracking()
            .Where(s => distinct.Contains(s.SubjectId))
            .OrderBy(s => s.SubjectName)
            .Select(s => s.SubjectName)
            .ToListAsync(cancellationToken);
        if (names.Count != distinct.Count)
            return null;
        return string.Join(",", names);
    }

    private async Task<List<AdminApplicationSubjectDto>> SubjectsFromCsvAsync(string? csv, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(csv))
            return [];
        var parts = csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        var all = await _context.Subjects.AsNoTracking().ToListAsync(cancellationToken);
        var list = new List<AdminApplicationSubjectDto>();
        foreach (var name in parts)
        {
            var s = all.FirstOrDefault(x => string.Equals(x.SubjectName.Trim(), name, StringComparison.OrdinalIgnoreCase));
            list.Add(new AdminApplicationSubjectDto { SubjectId = s?.SubjectId ?? 0, SubjectName = name });
        }

        return list;
    }
}
