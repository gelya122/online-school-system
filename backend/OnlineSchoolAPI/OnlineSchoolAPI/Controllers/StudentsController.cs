using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public StudentsController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StudentDto>>> GetStudents()
    {
        var students = await _context.Students
            .Select(s => new StudentDto
            {
                StudentId = s.StudentId,
                UserId = s.UserId,
                FirstName = s.FirstName,
                LastName = s.LastName,
                Phone = s.Phone,
                DateOfBirth = s.DateOfBirth,
                AvatarUrl = s.AvatarUrl,
                ClassNumber = s.ClassNumber,
                ParentPhone = s.ParentPhone,
                ParentEmail = s.ParentEmail,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();
        return Ok(students);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StudentDto>> GetStudent(int id)
    {
        var student = await _context.Students.FindAsync(id);
        if (student == null) return NotFound();

        return Ok(new StudentDto
        {
            StudentId = student.StudentId,
            UserId = student.UserId,
            FirstName = student.FirstName,
            LastName = student.LastName,
            Phone = student.Phone,
            DateOfBirth = student.DateOfBirth,
            AvatarUrl = student.AvatarUrl,
            ClassNumber = student.ClassNumber,
            ParentPhone = student.ParentPhone,
            ParentEmail = student.ParentEmail,
            CreatedAt = student.CreatedAt
        });
    }

    [HttpPost]
    public async Task<ActionResult<StudentDto>> CreateStudent(CreateStudentDto dto)
    {
        var student = new Student
        {
            UserId = dto.UserId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Phone = dto.Phone,
            DateOfBirth = dto.DateOfBirth,
            AvatarUrl = dto.AvatarUrl,
            ClassNumber = dto.ClassNumber,
            ParentPhone = dto.ParentPhone,
            ParentEmail = dto.ParentEmail
        };

        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetStudent), new { id = student.StudentId }, new StudentDto
        {
            StudentId = student.StudentId,
            UserId = student.UserId,
            FirstName = student.FirstName,
            LastName = student.LastName,
            Phone = student.Phone,
            DateOfBirth = student.DateOfBirth,
            AvatarUrl = student.AvatarUrl,
            ClassNumber = student.ClassNumber,
            ParentPhone = student.ParentPhone,
            ParentEmail = student.ParentEmail,
            CreatedAt = student.CreatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateStudent(int id, UpdateStudentDto dto)
    {
        var student = await _context.Students.FindAsync(id);
        if (student == null) return NotFound();

        if (dto.FirstName != null) student.FirstName = dto.FirstName;
        if (dto.LastName != null) student.LastName = dto.LastName;
        if (dto.Phone != null) student.Phone = dto.Phone;
        if (dto.DateOfBirth.HasValue) student.DateOfBirth = dto.DateOfBirth;
        if (dto.AvatarUrl != null) student.AvatarUrl = dto.AvatarUrl;
        if (dto.ClassNumber.HasValue) student.ClassNumber = dto.ClassNumber.Value;
        if (dto.ParentPhone != null) student.ParentPhone = dto.ParentPhone;
        if (dto.ParentEmail != null) student.ParentEmail = dto.ParentEmail;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStudent(int id)
    {
        var student = await _context.Students.FindAsync(id);
        if (student == null) return NotFound();

        _context.Students.Remove(student);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
