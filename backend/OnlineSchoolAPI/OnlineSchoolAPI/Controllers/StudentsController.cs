using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;
using OnlineSchoolAPI.Services;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;
    private readonly IWebHostEnvironment _env;

    public StudentsController(OnlineSchoolDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
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
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
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

    /// <summary>Загрузка аватара multipart (надёжнее, чем огромный base64 в JSON PUT).</summary>
    [HttpPost("{id:int}/avatar")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(589824)] // 512 KiB + запас заголовков multipart
    public async Task<ActionResult<AvatarUploadResponseDto>> UploadStudentAvatar(
        int id,
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Выберите файл изображения.");
        if (file.Length > StudentAvatarStorage.MaxAvatarBytes)
            return BadRequest($"Размер файла не больше {StudentAvatarStorage.MaxAvatarBytes / 1024} КБ.");

        var student = await _context.Students.FindAsync(id);
        if (student == null) return NotFound();

        byte[] bytes;
        await using (var ms = new MemoryStream((int)file.Length))
        {
            await file.CopyToAsync(ms, cancellationToken);
            bytes = ms.ToArray();
        }

        string url;
        try
        {
            url = await StudentAvatarStorage.SaveValidatedImageBytesAsync(
                _env, bytes, student.UserId, cancellationToken);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, ex.Message);
        }

        student.AvatarUrl = url;
        await _context.SaveChangesAsync(cancellationToken);
        return Ok(new AvatarUploadResponseDto { AvatarUrl = url });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateStudent(int id, UpdateStudentDto dto, CancellationToken cancellationToken)
    {
        var student = await _context.Students.FindAsync(id);
        if (student == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(dto.AvatarBase64))
        {
            var url = await StudentAvatarStorage.TrySaveFromBase64Async(
                _env, dto.AvatarBase64, student.UserId, cancellationToken);
            if (url != null)
                student.AvatarUrl = url;
        }
        else if (dto.AvatarUrl != null)
        {
            student.AvatarUrl = dto.AvatarUrl;
        }

        if (dto.FirstName != null) student.FirstName = dto.FirstName;
        if (dto.LastName != null) student.LastName = dto.LastName;
        if (dto.Phone != null) student.Phone = dto.Phone;
        if (dto.DateOfBirth.HasValue) student.DateOfBirth = dto.DateOfBirth;
        if (dto.ClassNumber.HasValue) student.ClassNumber = dto.ClassNumber.Value;
        if (dto.ParentPhone != null) student.ParentPhone = dto.ParentPhone;
        if (dto.ParentEmail != null) student.ParentEmail = dto.ParentEmail;

        await _context.SaveChangesAsync(cancellationToken);
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
