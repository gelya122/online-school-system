using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExamsController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public ExamsController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExamDto>>> GetExams()
    {
        var exams = await _context.Exams
            .Select(e => new ExamDto
            {
                ExamId = e.ExamId,
                ExamName = e.ExamName,
                Description = e.Description,
                IsActive = e.IsActive,
                CreatedAt = e.CreatedAt
            })
            .ToListAsync();

        return Ok(exams);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ExamDto>> GetExam(int id)
    {
        var exam = await _context.Exams.FindAsync(id);
        if (exam == null) return NotFound();

        return Ok(new ExamDto
        {
            ExamId = exam.ExamId,
            ExamName = exam.ExamName,
            Description = exam.Description,
            IsActive = exam.IsActive,
            CreatedAt = exam.CreatedAt
        });
    }

    [HttpPost]
    public async Task<ActionResult<ExamDto>> CreateExam(CreateExamDto dto)
    {
        var exam = new Exam
        {
            ExamName = dto.ExamName,
            Description = dto.Description,
            IsActive = dto.IsActive
        };

        _context.Exams.Add(exam);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetExam), new { id = exam.ExamId }, new ExamDto
        {
            ExamId = exam.ExamId,
            ExamName = exam.ExamName,
            Description = exam.Description,
            IsActive = exam.IsActive,
            CreatedAt = exam.CreatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateExam(int id, UpdateExamDto dto)
    {
        var exam = await _context.Exams.FindAsync(id);
        if (exam == null) return NotFound();

        if (dto.ExamName != null) exam.ExamName = dto.ExamName;
        if (dto.Description != null) exam.Description = dto.Description;
        if (dto.IsActive.HasValue) exam.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExam(int id)
    {
        var exam = await _context.Exams.FindAsync(id);
        if (exam == null) return NotFound();

        _context.Exams.Remove(exam);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

