using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public EmployeesController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployees()
    {
        var employees = await _context.Employees
            .Select(e => new EmployeeDto
            {
                EmployeeId = e.EmployeeId,
                UserId = e.UserId,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Patronymic = e.Patronymic,
                Phone = e.Phone,
                DateOfBirth = e.DateOfBirth,
                AvatarUrl = e.AvatarUrl,
                WorkExperience = e.WorkExperience,
                IsActive = e.IsActive,
                CreatedAt = e.CreatedAt
            })
            .ToListAsync();
        return Ok(employees);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeDto>> GetEmployee(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return NotFound();

        return Ok(new EmployeeDto
        {
            EmployeeId = employee.EmployeeId,
            UserId = employee.UserId,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Patronymic = employee.Patronymic,
            Phone = employee.Phone,
            DateOfBirth = employee.DateOfBirth,
            AvatarUrl = employee.AvatarUrl,
            WorkExperience = employee.WorkExperience,
            IsActive = employee.IsActive,
            CreatedAt = employee.CreatedAt
        });
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> CreateEmployee(CreateEmployeeDto dto)
    {
        var employee = new Employee
        {
            UserId = dto.UserId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Patronymic = dto.Patronymic,
            Phone = dto.Phone,
            DateOfBirth = dto.DateOfBirth,
            AvatarUrl = dto.AvatarUrl,
            WorkExperience = dto.WorkExperience,
            IsActive = dto.IsActive
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetEmployee), new { id = employee.EmployeeId }, new EmployeeDto
        {
            EmployeeId = employee.EmployeeId,
            UserId = employee.UserId,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Patronymic = employee.Patronymic,
            Phone = employee.Phone,
            DateOfBirth = employee.DateOfBirth,
            AvatarUrl = employee.AvatarUrl,
            WorkExperience = employee.WorkExperience,
            IsActive = employee.IsActive,
            CreatedAt = employee.CreatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEmployee(int id, UpdateEmployeeDto dto)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return NotFound();

        if (dto.FirstName != null) employee.FirstName = dto.FirstName;
        if (dto.LastName != null) employee.LastName = dto.LastName;
        if (dto.Patronymic != null) employee.Patronymic = dto.Patronymic;
        if (dto.Phone != null) employee.Phone = dto.Phone;
        if (dto.DateOfBirth.HasValue) employee.DateOfBirth = dto.DateOfBirth;
        if (dto.AvatarUrl != null) employee.AvatarUrl = dto.AvatarUrl;
        if (dto.WorkExperience.HasValue) employee.WorkExperience = dto.WorkExperience;
        if (dto.IsActive.HasValue) employee.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return NotFound();

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

