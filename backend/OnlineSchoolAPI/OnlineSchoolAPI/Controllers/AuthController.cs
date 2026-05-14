using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;
using OnlineSchoolAPI.Services;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private const int StudentRoleId = 7;

    private readonly OnlineSchoolDbContext _context;
    private readonly JwtTokenService _jwt;

    public AuthController(OnlineSchoolDbContext context, JwtTokenService jwt)
    {
        _context = context;
        _jwt = jwt;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthLoginResponseDto>> Login([FromBody] AuthLoginRequestDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("Укажите email и пароль.");

        var emailNorm = dto.Email.Trim();
        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == emailNorm.ToLower(), cancellationToken);

        if (user == null || !PasswordHasher.Verify(dto.Password, user.PasswordHash))
            return Unauthorized("Неверный email или пароль.");

        if (user.IsActive == false)
            return Unauthorized("Учётная запись отключена.");

        if (user.DeletedAt != null)
            return Unauthorized("Учётная запись удалена.");

        if (user.Employee == null)
            return Unauthorized("Доступ только для сотрудников.");

        if (user.Employee.DeletedAt != null)
            return Unauthorized("Профиль сотрудника удалён.");

        if (user.Employee.IsActive == false)
            return Unauthorized("Профиль сотрудника отключён.");

        if (user.RoleId == StudentRoleId)
            return Unauthorized("Вход в приложение для сотрудников недоступен для учеников.");

        var token = _jwt.CreateAccessToken(user);
        return Ok(new AuthLoginResponseDto
        {
            AccessToken = token,
            User = MapUser(user)
        });
    }

    [HttpPost("register-employee")]
    public async Task<ActionResult<AuthLoginResponseDto>> RegisterEmployee([FromBody] AuthRegisterEmployeeRequestDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("Укажите email и пароль.");

        var emailNorm = dto.Email.Trim();
        if (!EmailValidator.IsValid(emailNorm))
            return BadRequest("Введите корректный адрес электронной почты.");

        if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName))
            return BadRequest("Укажите имя и фамилию.");

        if (string.IsNullOrWhiteSpace(dto.Phone))
            return BadRequest("Укажите телефон.");

        if (dto.Password.Length < 6)
            return BadRequest("Пароль не короче 6 символов.");

        DateOnly? birthDate = null;
        if (!string.IsNullOrWhiteSpace(dto.BirthDate))
        {
            if (!DateOnly.TryParse(dto.BirthDate.Trim(), out var parsed))
                return BadRequest("Дата рождения: укажите в формате ГГГГ-ММ-ДД.");
            birthDate = parsed;
        }

        if (dto.Experience is < 0 or > 80)
            return BadRequest("Стаж: допустимый диапазон 0–80 лет.");

        if (dto.RoleId == StudentRoleId)
            return BadRequest("Роль «ученик» недоступна для регистрации сотрудника.");

        var role = await _context.UserRoles.FirstOrDefaultAsync(r => r.RoleId == dto.RoleId, cancellationToken);
        if (role == null)
            return BadRequest("Указана неизвестная роль.");

        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == emailNorm.ToLower(), cancellationToken))
            return BadRequest("Пользователь с таким email уже зарегистрирован.");

        await using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var now = DateTime.UtcNow;
            var user = new User
            {
                Email = emailNorm,
                PasswordHash = PasswordHasher.Hash(dto.Password),
                RoleId = role.RoleId,
                IsEmailConfirmed = true,
                IsActive = true,
                CreatedAt = now
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            string? avatarUrl = null;
            try
            {
                avatarUrl = await EmployeeAvatarStorage.TrySaveFromBase64Async(
                    HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>(),
                    dto.AvatarBase64,
                    user.UserId,
                    cancellationToken);
            }
            catch (ArgumentException ex)
            {
                await tx.RollbackAsync(cancellationToken);
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                await tx.RollbackAsync(cancellationToken);
                return StatusCode(500, ex.Message);
            }

            if (avatarUrl == null && !string.IsNullOrWhiteSpace(dto.AvatarUrl))
                avatarUrl = dto.AvatarUrl.Trim();

            var employee = new Employee
            {
                UserId = user.UserId,
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Patronymic = string.IsNullOrWhiteSpace(dto.Patronymic) ? null : dto.Patronymic.Trim(),
                Phone = dto.Phone.Trim(),
                DateOfBirth = birthDate,
                WorkExperience = dto.Experience,
                AvatarUrl = avatarUrl,
                IsActive = true,
                CreatedAt = now
            };
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync(cancellationToken);

            await tx.CommitAsync(cancellationToken);

            // Перечитываем, чтобы гарантировать Role/Employee навигации.
            var created = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Employee)
                .FirstAsync(u => u.UserId == user.UserId, cancellationToken);

            var token = _jwt.CreateAccessToken(created);
            return Ok(new AuthLoginResponseDto
            {
                AccessToken = token,
                User = MapUser(created)
            });
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<AuthCurrentUserDto>> Me(CancellationToken cancellationToken)
    {
        var uid = User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(uid, out var userId) || userId <= 0)
            return Unauthorized("Некорректный токен.");

        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        if (user == null)
            return Unauthorized("Пользователь не найден.");

        if (user.IsActive == false)
            return Unauthorized("Учётная запись отключена.");

        if (user.DeletedAt != null)
            return Unauthorized("Учётная запись удалена.");

        if (user.Employee == null || user.Employee.IsActive == false)
            return Unauthorized("Доступ запрещён.");

        if (user.Employee.DeletedAt != null)
            return Unauthorized("Доступ запрещён.");

        if (user.RoleId == StudentRoleId)
            return Unauthorized("Доступ запрещён.");

        return Ok(MapUser(user));
    }

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // JWT stateless: клиент просто удаляет токен.
        return Ok(new { ok = true });
    }

    private static AuthCurrentUserDto MapUser(User user)
    {
        return new AuthCurrentUserDto
        {
            UserId = user.UserId,
            EmployeeId = user.Employee?.EmployeeId,
            Email = user.Email ?? "",
            FirstName = user.Employee?.FirstName,
            LastName = user.Employee?.LastName,
            AvatarUrl = user.Employee?.AvatarUrl,
            RoleId = user.RoleId,
            RoleName = user.Role?.RoleName ?? ""
        };
    }
}

