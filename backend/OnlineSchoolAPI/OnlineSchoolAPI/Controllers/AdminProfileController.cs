using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;
using OnlineSchoolAPI.Services;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Authorize]
public sealed class AdminProfileController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;
    private readonly IWebHostEnvironment _env;

    public AdminProfileController(OnlineSchoolDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [HttpGet("api/admin/profile")]
    public async Task<ActionResult<AdminProfileDto>> Get(CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;

        var uid = User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(uid, out var userId) || userId <= 0) return Unauthorized("Некорректный токен.");

        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
        if (user?.Employee == null) return Unauthorized("Профиль сотрудника не найден.");

        var roleName = user.Role?.RoleName ?? "";
        var employeeId = user.Employee.EmployeeId;

        var dto = new AdminProfileDto
        {
            UserId = user.UserId,
            EmployeeId = employeeId,
            FullName = ((user.Employee.LastName ?? "") + " " + (user.Employee.FirstName ?? "") + " " + (user.Employee.Patronymic ?? "")).Trim(),
            Email = user.Email ?? "",
            Phone = user.Employee.Phone,
            Role = roleName,
            BirthDate = user.Employee.DateOfBirth,
            Experience = user.Employee.WorkExperience,
            AvatarUrl = user.Employee.AvatarUrl,
            AssignedInstances = [],
            HomeworkOnReview = 0,
            RecentChecked = []
        };

        var roleLower = roleName.Trim().ToLowerInvariant();
        var isTeacher = roleLower.Contains("teacher") || roleLower.Contains("препод");

        var staffRows = await _context.CourseInstanceStaff.AsNoTracking()
            .Include(s => s.Instance).ThenInclude(i => i.Course)
            .Include(s => s.Instance).ThenInclude(i => i.InstanceStatus)
            .Where(s => s.EmployeeId == employeeId && s.Instance.DeletedAt == null && s.DeletedAt == null)
            .ToListAsync(cancellationToken);

        var merged = new Dictionary<int, (CourseInstance Inst, DateTime? Sort)>();
        foreach (var s in staffRows)
        {
            if (!merged.TryGetValue(s.InstanceId, out var cur) || (s.AssignedAt > (cur.Sort ?? DateTime.MinValue)))
                merged[s.InstanceId] = (s.Instance, s.AssignedAt);
        }

        dto.AssignedInstances = merged.Values
            .OrderByDescending(x => x.Sort ?? DateTime.MinValue)
            .Select(x => new AdminProfileInstanceRowDto
            {
                InstanceId = x.Inst.InstanceId,
                Title = x.Inst.InstanceName,
                CourseTitle = x.Inst.Course.Title,
                StartDate = x.Inst.StartDate,
                Status = x.Inst.InstanceStatus.Code
            })
            .ToList();

        var assignedInstanceIds = merged.Keys.ToList();
        if (assignedInstanceIds.Count > 0)
        {
            dto.HomeworkOnReview = await (
                from s in _context.Submissions.AsNoTracking()
                where s.SubmittedAt != null && s.GradedAt == null && s.EnrollmentId != null
                      && (s.GradedByEmployeeId == null || s.GradedByEmployeeId == employeeId)
                join e in _context.Enrollments on s.EnrollmentId equals e.EnrollmentId
                where assignedInstanceIds.Contains(e.InstanceId)
                select s.SubmissionId).Distinct().CountAsync(cancellationToken);
        }

        if (isTeacher)
        {
            dto.RecentChecked = await (
                from s in _context.Submissions.AsNoTracking()
                where s.GradedByEmployeeId == employeeId && s.GradedAt != null && s.EnrollmentId != null
                join e in _context.Enrollments on s.EnrollmentId equals e.EnrollmentId
                join st in _context.Students on e.StudentId equals st.StudentId
                join a in _context.Assignments on s.AssignmentId equals a.AssignmentId
                join l in _context.Lessons on a.LessonId equals l.LessonId
                join inst in _context.CourseInstances on e.InstanceId equals inst.InstanceId
                join c in _context.Courses on inst.CourseId equals c.CourseId
                orderby s.GradedAt descending
                select new AdminProfileCheckedHomeworkRowDto
                {
                    SubmissionId = s.SubmissionId,
                    StudentName = ((st.LastName ?? "") + " " + (st.FirstName ?? "")).Trim(),
                    CourseTitle = c.Title,
                    InstanceTitle = inst.InstanceName,
                    LessonTitle = l.Title,
                    TotalScore = s.Score,
                    CheckedAt = s.GradedAt
                })
                .Take(10)
                .ToListAsync(cancellationToken);
        }
        else
        {
            dto.RecentChecked = [];
        }

        return Ok(dto);
    }

    [HttpPut("api/admin/profile")]
    public async Task<IActionResult> Update([FromBody] AdminProfileUpdateDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;

        var uid = User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(uid, out var userId) || userId <= 0) return Unauthorized("Некорректный токен.");

        var user = await _context.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
        if (user?.Employee == null) return Unauthorized("Профиль сотрудника не найден.");

        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            var email = dto.Email.Trim();
            var exists = await _context.Users.AsNoTracking()
                .AnyAsync(u => u.UserId != userId && u.DeletedAt == null && u.Email.ToLower() == email.ToLower(), cancellationToken);
            if (exists) return BadRequest("Email уже используется другим пользователем.");
            user.Email = email;
        }

        if (!string.IsNullOrWhiteSpace(dto.FirstName)) user.Employee.FirstName = dto.FirstName.Trim();
        if (!string.IsNullOrWhiteSpace(dto.LastName)) user.Employee.LastName = dto.LastName.Trim();
        user.Employee.Patronymic = string.IsNullOrWhiteSpace(dto.Patronymic) ? null : dto.Patronymic.Trim();
        user.Employee.Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();
        user.Employee.DateOfBirth = dto.BirthDate;
        user.Employee.WorkExperience = dto.Experience;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("api/admin/profile/change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] AdminChangePasswordDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;

        if (string.IsNullOrWhiteSpace(dto.CurrentPassword)) return BadRequest("Укажите currentPassword");
        if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 6) return BadRequest("Новый пароль слишком короткий (мин. 6).");

        var uid = User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(uid, out var userId) || userId <= 0) return Unauthorized("Некорректный токен.");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
        if (user == null) return Unauthorized("Пользователь не найден.");

        if (!PasswordHasher.Verify(dto.CurrentPassword, user.PasswordHash))
            return BadRequest("Текущий пароль неверный.");

        user.PasswordHash = PasswordHasher.Hash(dto.NewPassword);
        user.PasswordChangedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("api/admin/profile/avatar")]
    public async Task<ActionResult<AdminAvatarUploadResultDto>> UploadAvatar([FromBody] AdminUploadAvatarDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;

        var uid = User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(uid, out var userId) || userId <= 0) return Unauthorized("Некорректный токен.");

        var user = await _context.Users.Include(u => u.Employee).FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
        if (user?.Employee == null) return Unauthorized("Профиль сотрудника не найден.");

        var url = await EmployeeAvatarStorage.TrySaveFromBase64Async(_env, dto.Base64, userId, cancellationToken);
        if (string.IsNullOrWhiteSpace(url)) return BadRequest("Пустой файл.");

        user.Employee.AvatarUrl = url;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new AdminAvatarUploadResultDto { AvatarUrl = url });
    }
}

