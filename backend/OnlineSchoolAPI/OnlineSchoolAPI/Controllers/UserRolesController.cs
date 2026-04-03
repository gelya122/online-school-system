using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserRolesController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public UserRolesController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserRoleDto>>> GetUserRoles()
    {
        var roles = await _context.UserRoles
            .Select(r => new UserRoleDto
            {
                RoleId = r.RoleId,
                RoleName = r.RoleName,
                Description = r.Description
            })
            .ToListAsync();
        return Ok(roles);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserRoleDto>> GetUserRole(int id)
    {
        var role = await _context.UserRoles.FindAsync(id);
        if (role == null) return NotFound();

        return Ok(new UserRoleDto
        {
            RoleId = role.RoleId,
            RoleName = role.RoleName,
            Description = role.Description
        });
    }

    [HttpPost]
    public async Task<ActionResult<UserRoleDto>> CreateUserRole(CreateUserRoleDto dto)
    {
        var role = new UserRole
        {
            RoleName = dto.RoleName,
            Description = dto.Description
        };

        _context.UserRoles.Add(role);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUserRole), new { id = role.RoleId }, new UserRoleDto
        {
            RoleId = role.RoleId,
            RoleName = role.RoleName,
            Description = role.Description
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUserRole(int id, UpdateUserRoleDto dto)
    {
        var role = await _context.UserRoles.FindAsync(id);
        if (role == null) return NotFound();

        if (dto.RoleName != null) role.RoleName = dto.RoleName;
        if (dto.Description != null) role.Description = dto.Description;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUserRole(int id)
    {
        var role = await _context.UserRoles.FindAsync(id);
        if (role == null) return NotFound();

        _context.UserRoles.Remove(role);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

