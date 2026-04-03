using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LessonMaterialsController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public LessonMaterialsController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LessonMaterialDto>>> GetLessonMaterials()
    {
        var materials = await _context.LessonMaterials
            .Select(m => new LessonMaterialDto
            {
                MaterialId = m.MaterialId,
                LessonId = m.LessonId,
                FileName = m.FileName,
                FileUrl = m.FileUrl,
                FileType = m.FileType,
                FileSizeKb = m.FileSizeKb,
                DownloadCount = m.DownloadCount,
                UploadedAt = m.UploadedAt
            })
            .ToListAsync();
        return Ok(materials);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LessonMaterialDto>> GetLessonMaterial(int id)
    {
        var material = await _context.LessonMaterials.FindAsync(id);
        if (material == null) return NotFound();

        return Ok(new LessonMaterialDto
        {
            MaterialId = material.MaterialId,
            LessonId = material.LessonId,
            FileName = material.FileName,
            FileUrl = material.FileUrl,
            FileType = material.FileType,
            FileSizeKb = material.FileSizeKb,
            DownloadCount = material.DownloadCount,
            UploadedAt = material.UploadedAt
        });
    }

    [HttpPost]
    public async Task<ActionResult<LessonMaterialDto>> CreateLessonMaterial(CreateLessonMaterialDto dto)
    {
        var material = new LessonMaterial
        {
            LessonId = dto.LessonId,
            FileName = dto.FileName,
            FileUrl = dto.FileUrl,
            FileType = dto.FileType,
            FileSizeKb = dto.FileSizeKb
        };

        _context.LessonMaterials.Add(material);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetLessonMaterial), new { id = material.MaterialId }, new LessonMaterialDto
        {
            MaterialId = material.MaterialId,
            LessonId = material.LessonId,
            FileName = material.FileName,
            FileUrl = material.FileUrl,
            FileType = material.FileType,
            FileSizeKb = material.FileSizeKb,
            DownloadCount = material.DownloadCount,
            UploadedAt = material.UploadedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLessonMaterial(int id, UpdateLessonMaterialDto dto)
    {
        var material = await _context.LessonMaterials.FindAsync(id);
        if (material == null) return NotFound();

        if (dto.FileName != null) material.FileName = dto.FileName;
        if (dto.FileUrl != null) material.FileUrl = dto.FileUrl;
        if (dto.FileType != null) material.FileType = dto.FileType;
        if (dto.FileSizeKb.HasValue) material.FileSizeKb = dto.FileSizeKb;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLessonMaterial(int id)
    {
        var material = await _context.LessonMaterials.FindAsync(id);
        if (material == null) return NotFound();

        _context.LessonMaterials.Remove(material);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

