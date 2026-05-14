using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;
using OnlineSchoolAPI.Services;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/admin/students")]
[Authorize]
public sealed class AdminStudentsController : ControllerBase
{
    private const int StudentRoleId = 7;

    private readonly OnlineSchoolDbContext _context;
    private readonly IWebHostEnvironment _env;

    public AdminStudentsController(OnlineSchoolDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminStudentListRowDto>>> GetList(
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        [FromQuery] int? classNumber,
        CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin", "manager") is { } forbidRole)
            return forbidRole;

        var q = _context.Students
            .AsNoTracking()
            .Include(s => s.User)
            .Where(s => s.DeletedAt == null && s.User.DeletedAt == null);

        if (isActive.HasValue)
        {
            if (isActive.Value)
                q = q.Where(s => (s.User.IsActive == null || s.User.IsActive == true));
            else
                q = q.Where(s => s.User.IsActive == false);
        }

        if (classNumber.HasValue)
            q = q.Where(s => s.ClassNumber == classNumber.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            q = q.Where(st =>
                (st.LastName + " " + st.FirstName).ToLower().Contains(s) ||
                (st.FirstName + " " + st.LastName).ToLower().Contains(s) ||
                (st.User.Email ?? "").ToLower().Contains(s) ||
                (st.Phone ?? "").ToLower().Contains(s) ||
                (st.ParentPhone ?? "").ToLower().Contains(s) ||
                (st.ParentEmail ?? "").ToLower().Contains(s));
        }

        var rows = await q
            .OrderByDescending(s => s.CreatedAt ?? s.User.CreatedAt)
            .Take(500)
            .Select(s => new AdminStudentListRowDto
            {
                StudentId = s.StudentId,
                FullName = (s.LastName + " " + s.FirstName).Trim(),
                Email = s.User.Email,
                Phone = s.Phone,
                ClassNumber = s.ClassNumber,
                ParentPhone = s.ParentPhone,
                ParentEmail = s.ParentEmail,
                IsActive = s.User.IsActive != false,
                RegisteredAt = s.CreatedAt ?? s.User.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(rows);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AdminStudentDetailsDto>> GetById(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin", "manager") is { } forbidRole)
            return forbidRole;

        var student = await _context.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.StudentId == id && s.DeletedAt == null, cancellationToken);

        if (student == null)
            return NotFound();

        var enrollments = await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.Instance).ThenInclude(i => i.Course)
            .Where(e => e.StudentId == student.StudentId)
            .OrderByDescending(e => e.EnrolledAt)
            .Select(e => new AdminStudentEnrollmentDto
            {
                EnrollmentId = e.EnrollmentId,
                InstanceId = e.InstanceId,
                InstanceName = e.Instance.InstanceName,
                CourseId = e.Instance.CourseId,
                CourseTitle = e.Instance.Course.Title,
                StartDate = e.Instance.StartDate,
                EnrolledAt = e.EnrolledAt,
                CompletedAt = e.CompletedAt
            })
            .ToListAsync(cancellationToken);

        var payments = await _context.Payments
            .AsNoTracking()
            .Include(p => p.Order)
            .Include(p => p.PaymentStatus)
            .Where(p => p.Order.StudentId == student.StudentId)
            .OrderByDescending(p => p.PaidAt ?? p.CreatedAt)
            .Take(200)
            .Select(p => new AdminStudentPaymentDto
            {
                PaymentId = p.PaymentId,
                OrderId = p.OrderId,
                Amount = p.Amount,
                Status = p.PaymentStatus != null ? p.PaymentStatus.StatusName : "—",
                PaidAt = p.PaidAt,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var progressAgg = await _context.StudentProgresses
            .AsNoTracking()
            .Where(p => p.Enrollment.StudentId == student.StudentId)
            .GroupBy(_ => 1)
            .Select(g => new AdminStudentProgressDto
            {
                TotalLessons = g.Count(),
                CompletedLessons = g.Count(x => x.IsCompleted == true),
                WatchTimeSeconds = g.Sum(x => x.WatchTimeSeconds ?? 0)
            })
            .FirstOrDefaultAsync(cancellationToken) ?? new AdminStudentProgressDto();

        var notes = await _context.StudentNotes
            .AsNoTracking()
            .Include(n => n.Employee)
            .Where(n => n.StudentId == student.StudentId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(200)
            .Select(n => new AdminStudentNoteDto
            {
                NoteId = n.NoteId,
                NoteType = n.NoteType,
                NoteText = n.NoteText,
                EmployeeId = n.EmployeeId,
                EmployeeName = n.Employee == null ? null : (n.Employee.FirstName + " " + n.Employee.LastName).Trim(),
                CreatedAt = n.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var homework = await (
                from sub in _context.Submissions.AsNoTracking()
                where sub.EnrollmentId != null
                join e in _context.Enrollments on sub.EnrollmentId equals e.EnrollmentId
                where e.StudentId == student.StudentId
                join a in _context.Assignments on sub.AssignmentId equals a.AssignmentId
                join inst in _context.CourseInstances on e.InstanceId equals inst.InstanceId
                join c in _context.Courses on inst.CourseId equals c.CourseId
                join st in _context.SubmissionStatuses on sub.SubmissionStatusId equals st.StatusId
                orderby sub.SubmittedAt descending, sub.CreatedAt descending
                select new AdminStudentHomeworkRowDto
                {
                    SubmissionId = sub.SubmissionId,
                    AssignmentTitle = a.Title,
                    CourseTitle = c.Title,
                    InstanceName = inst.InstanceName,
                    Score = sub.Score,
                    SubmittedAt = sub.SubmittedAt,
                    GradedAt = sub.GradedAt,
                    StatusName = st.StatusName
                })
            .Take(100)
            .ToListAsync(cancellationToken);

        return Ok(new AdminStudentDetailsDto
        {
            StudentId = student.StudentId,
            UserId = student.UserId,
            Email = student.User.Email,
            IsActive = student.User.IsActive != false,
            FirstName = student.FirstName,
            LastName = student.LastName,
            Patronymic = null, // у Student нет patronymic в схеме
            Phone = student.Phone,
            DateOfBirth = student.DateOfBirth,
            ClassNumber = student.ClassNumber,
            ParentPhone = student.ParentPhone,
            ParentEmail = student.ParentEmail,
            AvatarUrl = student.AvatarUrl,
            RegisteredAt = student.CreatedAt ?? student.User.CreatedAt,
            Enrollments = enrollments,
            Payments = payments,
            Progress = progressAgg,
            Homework = homework,
            Notes = notes
        });
    }

    [HttpPost]
    public async Task<ActionResult<AdminStudentDetailsDto>> Create([FromBody] AdminStudentCreateDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin", "manager") is { } forbidRole)
            return forbidRole;

        if (string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest("Укажите email.");
        var emailNorm = dto.Email.Trim();
        if (!EmailValidator.IsValid(emailNorm))
            return BadRequest("Введите корректный email.");
        if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName))
            return BadRequest("Укажите имя и фамилию.");
        if (dto.ClassNumber is < 0 or > 11)
            return BadRequest("Класс должен быть от 0 до 11.");

        DateOnly? birth = null;
        if (!string.IsNullOrWhiteSpace(dto.BirthDate))
        {
            if (!DateOnly.TryParse(dto.BirthDate.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                return BadRequest("Дата рождения: формат ГГГГ-ММ-ДД.");
            birth = parsed;
        }

        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == emailNorm.ToLower(), cancellationToken))
            return BadRequest("Пользователь с таким email уже существует.");

        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
            return BadRequest("Пароль: не короче 6 символов.");

        await using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var now = DateTime.UtcNow;
            var user = new User
            {
                Email = emailNorm,
                PasswordHash = PasswordHasher.Hash(dto.Password),
                RoleId = StudentRoleId,
                IsEmailConfirmed = true,
                IsActive = dto.IsActive,
                CreatedAt = now
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            string? avatarUrl = null;
            try
            {
                avatarUrl = await StudentAvatarStorage.TrySaveFromBase64Async(_env, dto.AvatarBase64, user.UserId, cancellationToken);
            }
            catch (ArgumentException ex)
            {
                await tx.RollbackAsync(cancellationToken);
                return BadRequest(ex.Message);
            }
            if (avatarUrl == null && !string.IsNullOrWhiteSpace(dto.AvatarUrl))
                avatarUrl = dto.AvatarUrl.Trim();

            var student = new Student
            {
                UserId = user.UserId,
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim(),
                DateOfBirth = birth,
                AvatarUrl = avatarUrl,
                ClassNumber = dto.ClassNumber,
                ParentPhone = string.IsNullOrWhiteSpace(dto.ParentPhone) ? null : dto.ParentPhone.Trim(),
                ParentEmail = string.IsNullOrWhiteSpace(dto.ParentEmail) ? null : dto.ParentEmail.Trim(),
                CreatedAt = now
            };
            _context.Students.Add(student);
            await _context.SaveChangesAsync(cancellationToken);

            await tx.CommitAsync(cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = student.StudentId }, await GetById(student.StudentId, cancellationToken));
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] AdminStudentUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin", "manager") is { } forbidRole)
            return forbidRole;

        var student = await _context.Students.Include(s => s.User)
            .FirstOrDefaultAsync(s => s.StudentId == id && s.DeletedAt == null, cancellationToken);
        if (student == null)
            return NotFound();

        var emailNorm = (dto.Email ?? "").Trim();
        if (!EmailValidator.IsValid(emailNorm))
            return BadRequest("Введите корректный email.");
        if (dto.ClassNumber is < 0 or > 11)
            return BadRequest("Класс должен быть от 0 до 11.");

        DateOnly? birth = null;
        if (!string.IsNullOrWhiteSpace(dto.BirthDate))
        {
            if (!DateOnly.TryParse(dto.BirthDate.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                return BadRequest("Дата рождения: формат ГГГГ-ММ-ДД.");
            birth = parsed;
        }

        if (await _context.Users.AnyAsync(u => u.UserId != student.UserId && u.Email.ToLower() == emailNorm.ToLower(), cancellationToken))
            return BadRequest("Email уже занят другим пользователем.");

        string? avatarUrl = null;
        try
        {
            avatarUrl = await StudentAvatarStorage.TrySaveFromBase64Async(_env, dto.AvatarBase64, student.UserId, cancellationToken);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }

        if (avatarUrl == null && dto.AvatarUrl != null)
            avatarUrl = string.IsNullOrWhiteSpace(dto.AvatarUrl) ? null : dto.AvatarUrl.Trim();

        student.FirstName = (dto.FirstName ?? "").Trim();
        student.LastName = (dto.LastName ?? "").Trim();
        student.Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();
        student.DateOfBirth = birth;
        student.ClassNumber = dto.ClassNumber;
        student.ParentPhone = string.IsNullOrWhiteSpace(dto.ParentPhone) ? null : dto.ParentPhone.Trim();
        student.ParentEmail = string.IsNullOrWhiteSpace(dto.ParentEmail) ? null : dto.ParentEmail.Trim();
        if (dto.AvatarBase64 != null || dto.AvatarUrl != null)
            student.AvatarUrl = avatarUrl;

        student.User.Email = emailNorm;
        student.User.IsActive = dto.IsActive;
        student.User.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:int}/activate")]
    public async Task<IActionResult> Activate(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin", "manager") is { } forbidRole)
            return forbidRole;

        var student = await _context.Students.Include(s => s.User)
            .FirstOrDefaultAsync(s => s.StudentId == id && s.DeletedAt == null, cancellationToken);
        if (student == null) return NotFound();
        student.User.IsActive = true;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin", "manager") is { } forbidRole)
            return forbidRole;

        var student = await _context.Students.Include(s => s.User)
            .FirstOrDefaultAsync(s => s.StudentId == id && s.DeletedAt == null, cancellationToken);
        if (student == null) return NotFound();
        student.User.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    /// <summary>Мягкое удаление: <c>student.deleted_at</c> и <c>users.deleted_at</c>.</summary>
    [HttpPatch("{id:int}/soft-delete")]
    public async Task<IActionResult> SoftDelete(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin", "manager") is { } forbidRole)
            return forbidRole;

        var student = await _context.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.StudentId == id && s.DeletedAt == null, cancellationToken);
        if (student == null) return NotFound();

        var deletedAt = DateTime.UtcNow;
        student.DeletedAt = deletedAt;
        student.User.DeletedAt = deletedAt;
        student.User.IsActive = false;
        student.User.UpdatedAt = deletedAt;

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:int}/notes")]
    public async Task<ActionResult<IReadOnlyList<AdminStudentNoteDto>>> GetNotes(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin", "manager") is { } forbidRole)
            return forbidRole;

        var exists = await _context.Students.AnyAsync(s => s.StudentId == id && s.DeletedAt == null, cancellationToken);
        if (!exists) return NotFound();

        var notes = await _context.StudentNotes
            .AsNoTracking()
            .Include(n => n.Employee)
            .Where(n => n.StudentId == id)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new AdminStudentNoteDto
            {
                NoteId = n.NoteId,
                NoteType = n.NoteType,
                NoteText = n.NoteText,
                EmployeeId = n.EmployeeId,
                EmployeeName = n.Employee == null ? null : (n.Employee.FirstName + " " + n.Employee.LastName).Trim(),
                CreatedAt = n.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(notes);
    }

    [HttpPost("{id:int}/notes")]
    public async Task<ActionResult<AdminStudentNoteDto>> AddNote(int id, [FromBody] AdminStudentNoteCreateDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid)
            return forbid;
        if (AdminAccessGuard.ForbidIfRoleNotIn(this, "admin", "manager") is { } forbidRole)
            return forbidRole;

        if (string.IsNullOrWhiteSpace(dto.NoteText))
            return BadRequest("Введите текст заметки.");

        var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentId == id && s.DeletedAt == null, cancellationToken);
        if (student == null) return NotFound();

        var employeeId = AuthClaims.GetEmployeeId(User);

        var note = new StudentNote
        {
            StudentId = student.StudentId,
            EmployeeId = employeeId,
            NoteType = string.IsNullOrWhiteSpace(dto.NoteType) ? null : dto.NoteType.Trim(),
            NoteText = dto.NoteText.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        _context.StudentNotes.Add(note);
        await _context.SaveChangesAsync(cancellationToken);

        var saved = await _context.StudentNotes.AsNoTracking().Include(n => n.Employee)
            .FirstAsync(n => n.NoteId == note.NoteId, cancellationToken);

        return Ok(new AdminStudentNoteDto
        {
            NoteId = saved.NoteId,
            NoteType = saved.NoteType,
            NoteText = saved.NoteText,
            EmployeeId = saved.EmployeeId,
            EmployeeName = saved.Employee == null ? null : (saved.Employee.FirstName + " " + saved.Employee.LastName).Trim(),
            CreatedAt = saved.CreatedAt
        });
    }
}

