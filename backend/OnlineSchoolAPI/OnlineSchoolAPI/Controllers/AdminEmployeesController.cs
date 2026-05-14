using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;
using OnlineSchoolAPI.Services;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/admin/employees")]
[Authorize]
public sealed class AdminEmployeesController : ControllerBase
{
    private const int StudentRoleId = 7;

    private readonly OnlineSchoolDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly AuditLogWriter _audit;

    public AdminEmployeesController(OnlineSchoolDbContext context, IWebHostEnvironment env, AuditLogWriter audit)
    {
        _context = context;
        _env = env;
        _audit = audit;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminEmployeeListRowDto>>> GetList(
        [FromQuery] string? search,
        [FromQuery] int? roleId,
        CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin") is { } forbidRole)
            return forbidRole;

        var q = _context.Employees
            .AsNoTracking()
            .Include(e => e.User).ThenInclude(u => u.Role)
            .Where(e => e.DeletedAt == null && e.User.DeletedAt == null);

        if (roleId.HasValue)
            q = q.Where(e => e.User.RoleId == roleId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            q = q.Where(e =>
                (e.LastName + " " + e.FirstName + " " + (e.Patronymic ?? "")).ToLower().Contains(s) ||
                (e.FirstName + " " + e.LastName + " " + (e.Patronymic ?? "")).ToLower().Contains(s) ||
                (e.User.Email ?? "").ToLower().Contains(s) ||
                (e.Phone ?? "").ToLower().Contains(s));
        }

        var rows = await q
            .OrderByDescending(e => e.CreatedAt ?? e.User.CreatedAt)
            .Take(500)
            .Select(e => new AdminEmployeeListRowDto
            {
                EmployeeId = e.EmployeeId,
                FullName = (e.LastName + " " + e.FirstName + " " + (e.Patronymic ?? "")).Trim(),
                Email = e.User.Email,
                Phone = e.Phone,
                RoleId = e.User.RoleId,
                RoleName = e.User.Role.RoleName,
                Experience = e.WorkExperience,
                IsActive = (e.User.IsActive != false) && (e.IsActive != false),
                CreatedAt = e.CreatedAt ?? e.User.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(rows);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AdminEmployeeDetailsDto>> GetById(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin") is { } forbidRole)
            return forbidRole;

        var emp = await _context.Employees
            .Include(e => e.User).ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(e => e.EmployeeId == id && e.DeletedAt == null, cancellationToken);

        if (emp == null) return NotFound();

        var staffRows = await _context.CourseInstanceStaff
            .AsNoTracking()
            .Include(s => s.Instance).ThenInclude(i => i.Course)
            .Where(s => s.EmployeeId == emp.EmployeeId && s.DeletedAt == null)
            .ToListAsync(cancellationToken);

        var assignedTeachers = staffRows
            .Where(s => s.RoleId == CourseInstanceStaffRoleIds.Teacher)
            .Select(t => new AdminEmployeeAssignedInstanceDto
            {
                InstanceId = t.InstanceId,
                InstanceName = t.Instance.InstanceName,
                CourseId = t.Instance.CourseId,
                CourseTitle = t.Instance.Course.Title,
                Kind = "teacher",
                IsLeadOrMain = false,
                AssignedAt = t.AssignedAt
            })
            .ToList();

        var assignedCoords = staffRows
            .Where(s => s.RoleId == CourseInstanceStaffRoleIds.Mentor)
            .Select(c => new AdminEmployeeAssignedInstanceDto
            {
                InstanceId = c.InstanceId,
                InstanceName = c.Instance.InstanceName,
                CourseId = c.Instance.CourseId,
                CourseTitle = c.Instance.Course.Title,
                Kind = "coordinator",
                IsLeadOrMain = false,
                AssignedAt = c.AssignedAt
            })
            .ToList();

        var homeworkChecked = await (
            from s in _context.Submissions.AsNoTracking()
            where s.GradedByEmployeeId == emp.EmployeeId && s.EnrollmentId != null
            join e in _context.Enrollments on s.EnrollmentId equals e.EnrollmentId
            join st in _context.Students on e.StudentId equals st.StudentId
            join a in _context.Assignments on s.AssignmentId equals a.AssignmentId
            join inst in _context.CourseInstances on e.InstanceId equals inst.InstanceId
            join c in _context.Courses on inst.CourseId equals c.CourseId
            orderby (s.GradedAt ?? s.SubmittedAt ?? s.CreatedAt) descending
            select new AdminEmployeeHomeworkCheckedDto
            {
                SubmissionId = s.SubmissionId,
                StudentId = st.StudentId,
                StudentName = (st.FirstName + " " + st.LastName).Trim(),
                InstanceId = e.InstanceId,
                InstanceName = inst.InstanceName,
                CourseTitle = c.Title,
                HomeworkTitle = a.Title,
                SubmittedAt = s.SubmittedAt,
                CheckedAt = s.GradedAt
            })
            .Take(50)
            .ToListAsync(cancellationToken);

        var audit = await _context.AuditLogs
            .AsNoTracking()
            .Where(a => a.EmployeeId == emp.EmployeeId || a.UserId == emp.UserId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(200)
            .Select(a => new AdminAuditLogRowDto
            {
                AuditLogId = a.AuditLogId,
                CreatedAt = a.CreatedAt,
                Action = a.Action,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                OldValues = a.OldValues,
                NewValues = a.NewValues,
                IpAddress = a.IpAddress,
                UserAgent = a.UserAgent
            })
            .ToListAsync(cancellationToken);

        var allAssigned = assignedTeachers.Concat(assignedCoords)
            .OrderByDescending(x => x.AssignedAt)
            .ToList();

        return Ok(new AdminEmployeeDetailsDto
        {
            EmployeeId = emp.EmployeeId,
            UserId = emp.UserId,
            Email = emp.User.Email,
            IsActive = (emp.User.IsActive != false) && (emp.IsActive != false),
            FirstName = emp.FirstName,
            LastName = emp.LastName,
            Patronymic = emp.Patronymic,
            Phone = emp.Phone,
            DateOfBirth = emp.DateOfBirth,
            AvatarUrl = emp.AvatarUrl,
            Experience = emp.WorkExperience,
            RoleId = emp.User.RoleId,
            RoleName = emp.User.Role.RoleName,
            AssignedInstances = allAssigned,
            HomeworkChecked = homeworkChecked,
            AuditLog = audit
        });
    }

    [HttpPost]
    public async Task<ActionResult<AdminEmployeeDetailsDto>> Create([FromBody] AdminEmployeeCreateDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin") is { } forbidRole)
            return forbidRole;

        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("Укажите email и пароль.");
        var emailNorm = dto.Email.Trim();
        if (!EmailValidator.IsValid(emailNorm))
            return BadRequest("Введите корректный email.");
        if (dto.Password.Length < 6)
            return BadRequest("Пароль не короче 6 символов.");
        if (dto.RoleId == StudentRoleId)
            return BadRequest("Роль student нельзя назначить сотруднику.");
        if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName))
            return BadRequest("Укажите имя и фамилию.");

        var role = await _context.UserRoles.FirstOrDefaultAsync(r => r.RoleId == dto.RoleId, cancellationToken);
        if (role == null)
            return BadRequest("Неизвестная роль.");
        if (role.RoleId == StudentRoleId)
            return BadRequest("Роль student нельзя назначить сотруднику.");

        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == emailNorm.ToLower(), cancellationToken))
            return BadRequest("Пользователь с таким email уже существует.");

        DateOnly? birth = null;
        if (!string.IsNullOrWhiteSpace(dto.BirthDate))
        {
            if (!DateOnly.TryParse(dto.BirthDate.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                return BadRequest("Дата рождения: формат ГГГГ-ММ-ДД.");
            birth = parsed;
        }
        if (dto.Experience is < 0 or > 80)
            return BadRequest("Опыт: допустимый диапазон 0–80 лет.");

        await using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var now = DateTime.UtcNow;
            var user = new User
            {
                Email = emailNorm,
                Login = emailNorm,
                PasswordHash = PasswordHasher.Hash(dto.Password),
                RoleId = dto.RoleId,
                IsEmailConfirmed = true,
                IsActive = dto.IsActive,
                CreatedAt = now
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            string? avatarUrl = null;
            try
            {
                avatarUrl = await EmployeeAvatarStorage.TrySaveFromBase64Async(_env, dto.AvatarBase64, user.UserId, cancellationToken);
            }
            catch (ArgumentException ex)
            {
                await tx.RollbackAsync(cancellationToken);
                return BadRequest(ex.Message);
            }
            if (avatarUrl == null && !string.IsNullOrWhiteSpace(dto.AvatarUrl))
                avatarUrl = dto.AvatarUrl.Trim();

            var emp = new Employee
            {
                UserId = user.UserId,
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Patronymic = string.IsNullOrWhiteSpace(dto.Patronymic) ? null : dto.Patronymic.Trim(),
                Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim(),
                DateOfBirth = birth,
                WorkExperience = dto.Experience,
                AvatarUrl = avatarUrl,
                IsActive = dto.IsActive,
                CreatedAt = now
            };
            _context.Employees.Add(emp);

            _audit.Add("employee.create", "employee", null, null, new { dto.Email, dto.RoleId });
            await _context.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            return await GetById(emp.EmployeeId, cancellationToken);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] AdminEmployeeUpdateDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin") is { } forbidRole)
            return forbidRole;

        var emp = await _context.Employees
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.EmployeeId == id && e.DeletedAt == null, cancellationToken);
        if (emp == null) return NotFound();

        var emailNorm = (dto.Email ?? "").Trim();
        if (!EmailValidator.IsValid(emailNorm))
            return BadRequest("Введите корректный email.");
        if (dto.RoleId == StudentRoleId)
            return BadRequest("Роль student нельзя назначить сотруднику.");
        if (dto.Experience is < 0 or > 80)
            return BadRequest("Опыт: допустимый диапазон 0–80 лет.");

        var role = await _context.UserRoles.FirstOrDefaultAsync(r => r.RoleId == dto.RoleId, cancellationToken);
        if (role == null)
            return BadRequest("Неизвестная роль.");
        if (role.RoleId == StudentRoleId)
            return BadRequest("Роль student нельзя назначить сотруднику.");

        if (await _context.Users.AnyAsync(u => u.UserId != emp.UserId && u.Email.ToLower() == emailNorm.ToLower(), cancellationToken))
            return BadRequest("Email уже занят.");

        DateOnly? birth = null;
        if (!string.IsNullOrWhiteSpace(dto.BirthDate))
        {
            if (!DateOnly.TryParse(dto.BirthDate.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                return BadRequest("Дата рождения: формат ГГГГ-ММ-ДД.");
            birth = parsed;
        }

        string? avatarUrl = null;
        try
        {
            avatarUrl = await EmployeeAvatarStorage.TrySaveFromBase64Async(_env, dto.AvatarBase64, emp.UserId, cancellationToken);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        if (avatarUrl == null && dto.AvatarUrl != null)
            avatarUrl = string.IsNullOrWhiteSpace(dto.AvatarUrl) ? null : dto.AvatarUrl.Trim();

        var old = new { emp.User.Email, emp.User.RoleId, emp.FirstName, emp.LastName, emp.Patronymic, emp.Phone, emp.DateOfBirth, emp.WorkExperience, emp.AvatarUrl, UserActive = emp.User.IsActive, EmpActive = emp.IsActive };

        emp.User.Email = emailNorm;
        emp.User.RoleId = dto.RoleId;
        emp.User.IsActive = dto.IsActive;
        emp.User.UpdatedAt = DateTime.UtcNow;

        emp.FirstName = (dto.FirstName ?? "").Trim();
        emp.LastName = (dto.LastName ?? "").Trim();
        emp.Patronymic = string.IsNullOrWhiteSpace(dto.Patronymic) ? null : dto.Patronymic.Trim();
        emp.Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();
        emp.DateOfBirth = birth;
        emp.WorkExperience = dto.Experience;
        emp.IsActive = dto.IsActive;
        if (dto.AvatarBase64 != null || dto.AvatarUrl != null)
            emp.AvatarUrl = avatarUrl;

        var @new = new { emp.User.Email, emp.User.RoleId, emp.FirstName, emp.LastName, emp.Patronymic, emp.Phone, emp.DateOfBirth, emp.WorkExperience, emp.AvatarUrl, UserActive = emp.User.IsActive, EmpActive = emp.IsActive };
        _audit.Add("employee.update", "employee", emp.EmployeeId, old, @new);

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:int}/activate")]
    public async Task<IActionResult> Activate(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin") is { } forbidRole)
            return forbidRole;

        var emp = await _context.Employees.Include(e => e.User)
            .FirstOrDefaultAsync(e => e.EmployeeId == id && e.DeletedAt == null, cancellationToken);
        if (emp == null) return NotFound();

        var old = new { UserActive = emp.User.IsActive, EmpActive = emp.IsActive };
        emp.User.IsActive = true;
        emp.IsActive = true;
        _audit.Add("employee.activate", "employee", emp.EmployeeId, old, new { UserActive = emp.User.IsActive, EmpActive = emp.IsActive });

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin") is { } forbidRole)
            return forbidRole;

        var emp = await _context.Employees.Include(e => e.User)
            .FirstOrDefaultAsync(e => e.EmployeeId == id && e.DeletedAt == null, cancellationToken);
        if (emp == null) return NotFound();

        var old = new { UserActive = emp.User.IsActive, EmpActive = emp.IsActive };
        emp.User.IsActive = false;
        emp.IsActive = false;
        _audit.Add("employee.deactivate", "employee", emp.EmployeeId, old, new { UserActive = emp.User.IsActive, EmpActive = emp.IsActive });

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:int}/role")]
    public async Task<IActionResult> ChangeRole(int id, [FromBody] AdminEmployeeChangeRoleDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin") is { } forbidRole)
            return forbidRole;

        if (dto.RoleId == StudentRoleId)
            return BadRequest("Роль student нельзя назначить сотруднику.");

        var role = await _context.UserRoles.FirstOrDefaultAsync(r => r.RoleId == dto.RoleId, cancellationToken);
        if (role == null) return BadRequest("Неизвестная роль.");
        if (role.RoleId == StudentRoleId) return BadRequest("Роль student нельзя назначить сотруднику.");

        var emp = await _context.Employees.Include(e => e.User)
            .FirstOrDefaultAsync(e => e.EmployeeId == id && e.DeletedAt == null, cancellationToken);
        if (emp == null) return NotFound();

        var old = new { emp.User.RoleId };
        emp.User.RoleId = dto.RoleId;
        emp.User.UpdatedAt = DateTime.UtcNow;
        _audit.Add("employee.role.change", "employee", emp.EmployeeId, old, new { emp.User.RoleId });

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    /// <summary>Мягкое удаление: <c>employee.deleted_at</c> и <c>users.deleted_at</c>, без физического удаления.</summary>
    [HttpPatch("{id:int}/soft-delete")]
    public async Task<IActionResult> SoftDelete(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin") is { } forbidRole)
            return forbidRole;

        var emp = await _context.Employees
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.EmployeeId == id && e.DeletedAt == null, cancellationToken);
        if (emp == null) return NotFound();

        var now = DateTime.UtcNow;
        var old = new { emp.DeletedAt, UserDeletedAt = emp.User.DeletedAt, UserActive = emp.User.IsActive, EmpActive = emp.IsActive };
        emp.DeletedAt = now;
        emp.User.DeletedAt = now;
        emp.User.IsActive = false;
        emp.IsActive = false;
        emp.User.UpdatedAt = now;
        _audit.Add("employee.soft_delete", "employee", emp.EmployeeId, old, new { emp.DeletedAt, UserDeletedAt = emp.User.DeletedAt });

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

