using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;
using OnlineSchoolAPI.Services;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegistrationController : ControllerBase
{
    private const int StudentRoleId = 7;

    private readonly OnlineSchoolDbContext _context;
    private readonly IWebHostEnvironment _env;

    public RegistrationController(OnlineSchoolDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    /// <summary>Проверка формата email. Всегда 200 OK, чтобы клиент не путал ответ с сетевой ошибкой.</summary>
    [HttpGet("validate-email")]
    public IActionResult ValidateEmail([FromQuery] string? email)
    {
        return Ok(new { valid = EmailValidator.IsValid(email) });
    }

    /// <summary>Создание пользователя (роль 7) и профиля ученика.</summary>
    [HttpPost("student")]
    public async Task<ActionResult<RegisterStudentResponseDto>> RegisterStudent(
        [FromBody] RegisterStudentDto dto,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("Укажите email и пароль.");

        var emailNorm = dto.Email.Trim();
        if (!EmailValidator.IsValid(emailNorm))
            return BadRequest("Введите корректный адрес электронной почты.");

        if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName))
            return BadRequest("Укажите имя и фамилию.");

        if (string.IsNullOrWhiteSpace(dto.Phone))
            return BadRequest("Укажите телефон ученика.");

        if (string.IsNullOrWhiteSpace(dto.ParentPhone))
            return BadRequest("Укажите телефон родителя.");

        if (string.IsNullOrWhiteSpace(dto.ParentEmail))
            return BadRequest("Укажите почту родителя.");

        var parentEmailNorm = dto.ParentEmail.Trim();
        if (!EmailValidator.IsValid(parentEmailNorm))
            return BadRequest("Введите корректную почту родителя.");

        if (dto.ClassNumber is < 1 or > 11)
            return BadRequest("Класс должен быть от 1 до 11.");

        if (string.IsNullOrWhiteSpace(dto.DateOfBirth) ||
            !DateOnly.TryParse(dto.DateOfBirth.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOfBirth))
            return BadRequest("Укажите корректную дату рождения (формат ГГГГ-ММ-ДД).");

        if (dto.Password.Length < 6)
            return BadRequest("Пароль не короче 6 символов.");

        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == emailNorm.ToLower(), cancellationToken))
            return BadRequest("Пользователь с таким email уже зарегистрирован.");

        if (!await _context.UserRoles.AnyAsync(r => r.RoleId == StudentRoleId, cancellationToken))
            return StatusCode(500, "В справочнике ролей не найдена роль с идентификатором 7 (ученик).");

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
                IsActive = true,
                CreatedAt = now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            string? avatarUrl = null;
            try
            {
                avatarUrl = await StudentAvatarStorage.TrySaveFromBase64Async(
                    _env,
                    dto.AvatarBase64,
                    user.UserId,
                    cancellationToken);
            }
            catch (ArgumentException ex)
            {
                await tx.RollbackAsync(cancellationToken);
                return BadRequest(ex.Message);
            }

            var student = new Student
            {
                UserId = user.UserId,
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Phone = dto.Phone.Trim(),
                DateOfBirth = dateOfBirth,
                AvatarUrl = avatarUrl,
                ClassNumber = dto.ClassNumber,
                ParentPhone = dto.ParentPhone.Trim(),
                ParentEmail = parentEmailNorm,
                CreatedAt = now
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync(cancellationToken);

            await tx.CommitAsync(cancellationToken);

            var roleRow = await _context.UserRoles.AsNoTracking()
                .FirstOrDefaultAsync(r => r.RoleId == StudentRoleId, cancellationToken);

            return Ok(new RegisterStudentResponseDto
            {
                UserId = user.UserId,
                StudentId = student.StudentId,
                Email = user.Email,
                AvatarUrl = avatarUrl,
                RoleLabel = FormatRoleLabel(roleRow)
            });
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>Вход по email и паролю.</summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginStudentResponseDto>> LoginStudent(
        [FromBody] LoginStudentDto dto,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("Укажите email и пароль.");

        var emailNorm = dto.Email.Trim();
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == emailNorm.ToLower(), cancellationToken);

        if (user == null || !PasswordHasher.Verify(dto.Password, user.PasswordHash))
            return Unauthorized("Неверный email или пароль.");

        var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.UserId, cancellationToken);

        return Ok(new LoginStudentResponseDto
        {
            UserId = user.UserId,
            StudentId = student?.StudentId,
            Email = user.Email,
            FirstName = student?.FirstName,
            LastName = student?.LastName,
            AvatarUrl = student?.AvatarUrl,
            RoleLabel = FormatRoleLabel(user.Role)
        });
    }

    private static string? FormatRoleLabel(UserRole? role)
    {
        if (role == null) return null;
        if (!string.IsNullOrWhiteSpace(role.Description))
            return role.Description.Trim();
        return role.RoleName;
    }

}
