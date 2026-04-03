using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public UsersController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var users = await _context.Users
            .Select(u => new UserDto
            {
                UserId = u.UserId,
                Email = u.Email,
                RoleId = u.RoleId,
                IsEmailConfirmed = u.IsEmailConfirmed,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        return Ok(new UserDto
        {
            UserId = user.UserId,
            Email = user.Email,
            RoleId = user.RoleId,
            IsEmailConfirmed = user.IsEmailConfirmed,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        });
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto dto)
    {
        var user = new User
        {
            Email = dto.Email,
            PasswordHash = dto.PasswordHash,
            RoleId = dto.RoleId,
            IsEmailConfirmed = dto.IsEmailConfirmed,
            IsActive = dto.IsActive
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, new UserDto
        {
            UserId = user.UserId,
            Email = user.Email,
            RoleId = user.RoleId,
            IsEmailConfirmed = user.IsEmailConfirmed,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        if (dto.Email != null) user.Email = dto.Email;
        if (dto.PasswordHash != null) user.PasswordHash = dto.PasswordHash;
        if (dto.RoleId.HasValue) user.RoleId = dto.RoleId.Value;
        if (dto.IsEmailConfirmed.HasValue) user.IsEmailConfirmed = dto.IsEmailConfirmed;
        if (dto.IsActive.HasValue) user.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

