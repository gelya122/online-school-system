using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;
using OnlineSchoolAPI.Services;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/admin/dictionaries")]
[Authorize]
public sealed class AdminDictionariesController : ControllerBase
{
    private const string InUseMessage = "Запись используется в других данных и не может быть удалена.";
    private readonly OnlineSchoolDbContext _context;

    public AdminDictionariesController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet("registry")]
    public ActionResult<IReadOnlyList<AdminDictionaryRegistryItemDto>> Registry()
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;

        AdminDictionaryRegistryItemDto I(string code, string title, bool supportsDeactivate) =>
            new() { Code = code, Title = title, SupportsDeactivate = supportsDeactivate };

        return Ok(new[]
        {
            I("user-roles", "Роли пользователей (user_role)", false),
            I("application-statuses", "Статусы заявок (application_status)", false),
            I("assignment-types", "Типы заданий (assignment_type)", false),
            I("submission-statuses", "Статусы сдачи (submission_status)", false),
            I("enrollment-statuses", "Статусы зачисления (enrollment_status)", false),
            I("order-statuses", "Статусы заказа (order_status)", false),
            I("payment-statuses", "Статусы оплаты (payment_status)", false),
            I("payment-methods", "Способы оплаты (payment_method)", true),
            I("discount-types", "Типы скидок (discount_type)", true),
            I("lesson-types", "Типы уроков (lesson_type)", false),
            I("subjects", "Предметы (subject)", true),
            I("exams", "Экзамены (exam)", true),
            I("course-categories", "Категории курсов (course_category)", false)
        });
    }

    #region course-categories

    [HttpGet("course-categories")]
    public async Task<ActionResult<IReadOnlyList<AdminCourseCategoryDictDto>>> CourseCategories(CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;

        var list = await _context.CourseCategories.AsNoTracking()
            .OrderBy(x => x.CategoryName)
            .Select(x => new AdminCourseCategoryDictDto
            {
                CategoryId = x.CategoryId,
                CategoryName = x.CategoryName,
                Description = x.Description,
                SubjectId = x.SubjectId,
                ExamId = x.ExamId
            })
            .ToListAsync(cancellationToken);

        return Ok(list);
    }

    [HttpPost("course-categories")]
    public async Task<ActionResult<AdminCourseCategoryDictDto>> CreateCourseCategory([FromBody] AdminCourseCategoryUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        if (string.IsNullOrWhiteSpace(dto.CategoryName)) return BadRequest("Укажите название");

        if (dto.SubjectId is > 0 && !await _context.Subjects.AsNoTracking().AnyAsync(s => s.SubjectId == dto.SubjectId, cancellationToken))
            return BadRequest("Предмет не найден");
        if (dto.ExamId is > 0 && !await _context.Exams.AsNoTracking().AnyAsync(e => e.ExamId == dto.ExamId, cancellationToken))
            return BadRequest("Экзамен не найден");

        var e = new CourseCategory
        {
            CategoryName = dto.CategoryName.Trim(),
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            SubjectId = dto.SubjectId > 0 ? dto.SubjectId : null,
            ExamId = dto.ExamId > 0 ? dto.ExamId : null
        };
        _context.CourseCategories.Add(e);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new AdminCourseCategoryDictDto
        {
            CategoryId = e.CategoryId,
            CategoryName = e.CategoryName,
            Description = e.Description,
            SubjectId = e.SubjectId,
            ExamId = e.ExamId
        });
    }

    [HttpPut("course-categories/{id:int}")]
    public async Task<IActionResult> UpdateCourseCategory(int id, [FromBody] AdminCourseCategoryUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        if (string.IsNullOrWhiteSpace(dto.CategoryName)) return BadRequest("Укажите название");

        if (dto.SubjectId is > 0 && !await _context.Subjects.AsNoTracking().AnyAsync(s => s.SubjectId == dto.SubjectId, cancellationToken))
            return BadRequest("Предмет не найден");
        if (dto.ExamId is > 0 && !await _context.Exams.AsNoTracking().AnyAsync(e => e.ExamId == dto.ExamId, cancellationToken))
            return BadRequest("Экзамен не найден");

        var e = await _context.CourseCategories.FirstOrDefaultAsync(x => x.CategoryId == id, cancellationToken);
        if (e == null) return NotFound();

        e.CategoryName = dto.CategoryName.Trim();
        e.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        e.SubjectId = dto.SubjectId > 0 ? dto.SubjectId : null;
        e.ExamId = dto.ExamId > 0 ? dto.ExamId : null;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("course-categories/{id:int}")]
    public async Task<IActionResult> DeleteCourseCategory(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;

        if (await _context.Courses.AsNoTracking().AnyAsync(c => c.CategoryId == id, cancellationToken))
            return Conflict(InUseMessage);

        var e = await _context.CourseCategories.FirstOrDefaultAsync(x => x.CategoryId == id, cancellationToken);
        if (e == null) return NotFound();

        _context.CourseCategories.Remove(e);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    #endregion

    #region subjects

    [HttpGet("subjects")]
    public async Task<ActionResult<IReadOnlyList<AdminSubjectDictDto>>> Subjects(CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;

        var list = await _context.Subjects.AsNoTracking()
            .OrderBy(x => x.SubjectName)
            .Select(x => new AdminSubjectDictDto
            {
                SubjectId = x.SubjectId,
                SubjectName = x.SubjectName,
                Description = x.Description,
                IsActive = x.IsActive != false
            })
            .ToListAsync(cancellationToken);

        return Ok(list);
    }

    [HttpPost("subjects")]
    public async Task<ActionResult<AdminSubjectDictDto>> CreateSubject([FromBody] AdminSubjectUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        if (string.IsNullOrWhiteSpace(dto.SubjectName)) return BadRequest("Укажите название");

        var e = new Subject
        {
            SubjectName = dto.SubjectName.Trim(),
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            IsActive = dto.IsActive
        };
        _context.Subjects.Add(e);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new AdminSubjectDictDto
        {
            SubjectId = e.SubjectId,
            SubjectName = e.SubjectName,
            Description = e.Description,
            IsActive = e.IsActive != false
        });
    }

    [HttpPut("subjects/{id:int}")]
    public async Task<IActionResult> UpdateSubject(int id, [FromBody] AdminSubjectUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        if (string.IsNullOrWhiteSpace(dto.SubjectName)) return BadRequest("Укажите название");

        var e = await _context.Subjects.FirstOrDefaultAsync(x => x.SubjectId == id, cancellationToken);
        if (e == null) return NotFound();

        e.SubjectName = dto.SubjectName.Trim();
        e.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        e.IsActive = dto.IsActive;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPatch("subjects/{id:int}/active")]
    public async Task<IActionResult> PatchSubjectActive(int id, [FromBody] AdminDictActiveDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;

        var e = await _context.Subjects.FirstOrDefaultAsync(x => x.SubjectId == id, cancellationToken);
        if (e == null) return NotFound();

        e.IsActive = dto.IsActive;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("subjects/{id:int}")]
    public async Task<IActionResult> DeleteSubject(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;

        var subjName = await _context.Subjects.AsNoTracking().Where(s => s.SubjectId == id).Select(s => s.SubjectName).FirstOrDefaultAsync(cancellationToken);
        var usedInTrials = subjName != null && await _context.TrialApplications.AsNoTracking()
            .AnyAsync(a => a.SelectedSubjects != null && a.SelectedSubjects.Contains(subjName), cancellationToken);
        if (await _context.CourseCategories.AsNoTracking().AnyAsync(c => c.SubjectId == id, cancellationToken) || usedInTrials)
            return Conflict(InUseMessage + " Деактивируйте запись (is_active=false), если она ещё нужна в истории.");

        var e = await _context.Subjects.FirstOrDefaultAsync(x => x.SubjectId == id, cancellationToken);
        if (e == null) return NotFound();

        _context.Subjects.Remove(e);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    #endregion

    #region exams

    [HttpGet("exams")]
    public async Task<ActionResult<IReadOnlyList<AdminExamDictDto>>> Exams(CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;

        var list = await _context.Exams.AsNoTracking()
            .OrderBy(x => x.ExamName)
            .Select(x => new AdminExamDictDto
            {
                ExamId = x.ExamId,
                ExamName = x.ExamName,
                Description = x.Description,
                IsActive = x.IsActive != false
            })
            .ToListAsync(cancellationToken);

        return Ok(list);
    }

    [HttpPost("exams")]
    public async Task<ActionResult<AdminExamDictDto>> CreateExam([FromBody] AdminExamUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        if (string.IsNullOrWhiteSpace(dto.ExamName)) return BadRequest("Укажите название");

        var e = new Exam
        {
            ExamName = dto.ExamName.Trim(),
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            IsActive = dto.IsActive
        };
        _context.Exams.Add(e);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new AdminExamDictDto
        {
            ExamId = e.ExamId,
            ExamName = e.ExamName,
            Description = e.Description,
            IsActive = e.IsActive != false
        });
    }

    [HttpPut("exams/{id:int}")]
    public async Task<IActionResult> UpdateExam(int id, [FromBody] AdminExamUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        if (string.IsNullOrWhiteSpace(dto.ExamName)) return BadRequest("Укажите название");

        var e = await _context.Exams.FirstOrDefaultAsync(x => x.ExamId == id, cancellationToken);
        if (e == null) return NotFound();

        e.ExamName = dto.ExamName.Trim();
        e.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        e.IsActive = dto.IsActive;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPatch("exams/{id:int}/active")]
    public async Task<IActionResult> PatchExamActive(int id, [FromBody] AdminDictActiveDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;

        var e = await _context.Exams.FirstOrDefaultAsync(x => x.ExamId == id, cancellationToken);
        if (e == null) return NotFound();

        e.IsActive = dto.IsActive;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("exams/{id:int}")]
    public async Task<IActionResult> DeleteExam(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;

        if (await _context.CourseCategories.AsNoTracking().AnyAsync(c => c.ExamId == id, cancellationToken))
            return Conflict(InUseMessage + " Деактивируйте запись (is_active=false), если она ещё нужна в истории.");

        var e = await _context.Exams.FirstOrDefaultAsync(x => x.ExamId == id, cancellationToken);
        if (e == null) return NotFound();

        _context.Exams.Remove(e);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    #endregion

    #region user-roles

    [HttpGet("user-roles")]
    public async Task<ActionResult<IReadOnlyList<AdminUserRoleDictDto>>> UserRoles(CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;

        var list = await _context.UserRoles.AsNoTracking()
            .OrderBy(x => x.RoleName)
            .Select(x => new AdminUserRoleDictDto { RoleId = x.RoleId, RoleName = x.RoleName, Description = x.Description })
            .ToListAsync(cancellationToken);

        return Ok(list);
    }

    [HttpPost("user-roles")]
    public async Task<ActionResult<AdminUserRoleDictDto>> CreateUserRole([FromBody] AdminDictNameDescUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Укажите название");

        var name = dto.Name.Trim();
        if (await _context.UserRoles.AsNoTracking().AnyAsync(r => r.RoleName.ToLower() == name.ToLower(), cancellationToken))
            return BadRequest("Роль с таким именем уже есть.");

        var e = new UserRole { RoleName = name, Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim() };
        _context.UserRoles.Add(e);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new AdminUserRoleDictDto { RoleId = e.RoleId, RoleName = e.RoleName, Description = e.Description });
    }

    [HttpPut("user-roles/{id:int}")]
    public async Task<IActionResult> UpdateUserRole(int id, [FromBody] AdminDictNameDescUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Укажите название");

        var name = dto.Name.Trim();
        if (await _context.UserRoles.AsNoTracking().AnyAsync(r => r.RoleId != id && r.RoleName.ToLower() == name.ToLower(), cancellationToken))
            return BadRequest("Роль с таким именем уже есть.");

        var e = await _context.UserRoles.FirstOrDefaultAsync(x => x.RoleId == id, cancellationToken);
        if (e == null) return NotFound();

        e.RoleName = name;
        e.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("user-roles/{id:int}")]
    public async Task<IActionResult> DeleteUserRole(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;

        if (await _context.Users.AsNoTracking().AnyAsync(u => u.RoleId == id, cancellationToken))
            return Conflict(InUseMessage + " У ролей без поля is_active доступна только корректная смена названия/описания.");

        var e = await _context.UserRoles.FirstOrDefaultAsync(x => x.RoleId == id, cancellationToken);
        if (e == null) return NotFound();

        _context.UserRoles.Remove(e);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    #endregion

    #region simple status tables (application, submission, enrollment, order, payment)

    private static string? NormDesc(AdminDictNameDescUpsertDto dto) =>
        string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();

    [HttpGet("application-statuses")]
    public async Task<ActionResult<IReadOnlyList<AdminSimpleStatusDictDto>>> ApplicationStatuses(CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        var list = await _context.ApplicationStatuses.AsNoTracking()
            .OrderBy(x => x.StatusName)
            .Select(x => new AdminSimpleStatusDictDto { Id = x.StatusId, Name = x.StatusName, Description = x.Description })
            .ToListAsync(cancellationToken);
        return Ok(list);
    }

    [HttpPost("application-statuses")]
    public async Task<ActionResult<AdminSimpleStatusDictDto>> CreateApplicationStatus([FromBody] AdminDictNameDescUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Укажите название");
        var e = new ApplicationStatus { StatusName = dto.Name.Trim(), Description = NormDesc(dto) };
        _context.ApplicationStatuses.Add(e);
        await _context.SaveChangesAsync(cancellationToken);
        return Ok(new AdminSimpleStatusDictDto { Id = e.StatusId, Name = e.StatusName, Description = e.Description });
    }

    [HttpPut("application-statuses/{id:int}")]
    public async Task<IActionResult> UpdateApplicationStatus(int id, [FromBody] AdminDictNameDescUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Укажите название");
        var e = await _context.ApplicationStatuses.FirstOrDefaultAsync(x => x.StatusId == id, cancellationToken);
        if (e == null) return NotFound();
        e.StatusName = dto.Name.Trim();
        e.Description = NormDesc(dto);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("application-statuses/{id:int}")]
    public async Task<IActionResult> DeleteApplicationStatus(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        if (await _context.TrialApplications.AsNoTracking().AnyAsync(a => a.ApplicationStatusId == id, cancellationToken))
            return Conflict(InUseMessage + " Деактивация не предусмотрена схемой таблицы application_status.");
        var e = await _context.ApplicationStatuses.FirstOrDefaultAsync(x => x.StatusId == id, cancellationToken);
        if (e == null) return NotFound();
        _context.ApplicationStatuses.Remove(e);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("submission-statuses")]
    public async Task<ActionResult<IReadOnlyList<AdminSimpleStatusDictDto>>> SubmissionStatuses(CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        var list = await _context.SubmissionStatuses.AsNoTracking()
            .OrderBy(x => x.StatusName)
            .Select(x => new AdminSimpleStatusDictDto { Id = x.StatusId, Name = x.StatusName, Description = x.Description })
            .ToListAsync(cancellationToken);
        return Ok(list);
    }

    [HttpPost("submission-statuses")]
    public async Task<ActionResult<AdminSimpleStatusDictDto>> CreateSubmissionStatus([FromBody] AdminDictNameDescUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Укажите название");
        var e = new SubmissionStatus { StatusName = dto.Name.Trim(), Description = NormDesc(dto) };
        _context.SubmissionStatuses.Add(e);
        await _context.SaveChangesAsync(cancellationToken);
        return Ok(new AdminSimpleStatusDictDto { Id = e.StatusId, Name = e.StatusName, Description = e.Description });
    }

    [HttpPut("submission-statuses/{id:int}")]
    public async Task<IActionResult> UpdateSubmissionStatus(int id, [FromBody] AdminDictNameDescUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Укажите название");
        var e = await _context.SubmissionStatuses.FirstOrDefaultAsync(x => x.StatusId == id, cancellationToken);
        if (e == null) return NotFound();
        e.StatusName = dto.Name.Trim();
        e.Description = NormDesc(dto);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("submission-statuses/{id:int}")]
    public async Task<IActionResult> DeleteSubmissionStatus(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        if (await _context.Submissions.AsNoTracking().AnyAsync(x => x.SubmissionStatusId == id, cancellationToken))
            return Conflict(InUseMessage + " Деактивация не предусмотрена схемой таблицы submission_status.");
        var e = await _context.SubmissionStatuses.FirstOrDefaultAsync(x => x.StatusId == id, cancellationToken);
        if (e == null) return NotFound();
        _context.SubmissionStatuses.Remove(e);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("enrollment-statuses")]
    public async Task<ActionResult<IReadOnlyList<AdminSimpleStatusDictDto>>> EnrollmentStatuses(CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        var list = await _context.EnrollmentStatuses.AsNoTracking()
            .OrderBy(x => x.StatusName)
            .Select(x => new AdminSimpleStatusDictDto { Id = x.StatusId, Name = x.StatusName, Description = x.Description })
            .ToListAsync(cancellationToken);
        return Ok(list);
    }

    [HttpPost("enrollment-statuses")]
    public async Task<ActionResult<AdminSimpleStatusDictDto>> CreateEnrollmentStatus([FromBody] AdminDictNameDescUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Укажите название");
        var e = new EnrollmentStatus { StatusName = dto.Name.Trim(), Description = NormDesc(dto) };
        _context.EnrollmentStatuses.Add(e);
        await _context.SaveChangesAsync(cancellationToken);
        return Ok(new AdminSimpleStatusDictDto { Id = e.StatusId, Name = e.StatusName, Description = e.Description });
    }

    [HttpPut("enrollment-statuses/{id:int}")]
    public async Task<IActionResult> UpdateEnrollmentStatus(int id, [FromBody] AdminDictNameDescUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Укажите название");
        var e = await _context.EnrollmentStatuses.FirstOrDefaultAsync(x => x.StatusId == id, cancellationToken);
        if (e == null) return NotFound();
        e.StatusName = dto.Name.Trim();
        e.Description = NormDesc(dto);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("enrollment-statuses/{id:int}")]
    public async Task<IActionResult> DeleteEnrollmentStatus(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        if (await _context.Enrollments.AsNoTracking().AnyAsync(x => x.EnrollmentStatusId == id, cancellationToken))
            return Conflict(InUseMessage + " Деактивация не предусмотрена схемой таблицы enrollment_status.");
        var e = await _context.EnrollmentStatuses.FirstOrDefaultAsync(x => x.StatusId == id, cancellationToken);
        if (e == null) return NotFound();
        _context.EnrollmentStatuses.Remove(e);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("order-statuses")]
    public async Task<ActionResult<IReadOnlyList<AdminSimpleStatusDictDto>>> OrderStatuses(CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        var list = await _context.OrderStatuses.AsNoTracking()
            .OrderBy(x => x.StatusName)
            .Select(x => new AdminSimpleStatusDictDto { Id = x.StatusId, Name = x.StatusName, Description = x.Description })
            .ToListAsync(cancellationToken);
        return Ok(list);
    }

    [HttpPost("order-statuses")]
    public async Task<ActionResult<AdminSimpleStatusDictDto>> CreateOrderStatus([FromBody] AdminDictNameDescUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Укажите название");
        var e = new OrderStatus { StatusName = dto.Name.Trim(), Description = NormDesc(dto) };
        _context.OrderStatuses.Add(e);
        await _context.SaveChangesAsync(cancellationToken);
        return Ok(new AdminSimpleStatusDictDto { Id = e.StatusId, Name = e.StatusName, Description = e.Description });
    }

    [HttpPut("order-statuses/{id:int}")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] AdminDictNameDescUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Укажите название");
        var e = await _context.OrderStatuses.FirstOrDefaultAsync(x => x.StatusId == id, cancellationToken);
        if (e == null) return NotFound();
        e.StatusName = dto.Name.Trim();
        e.Description = NormDesc(dto);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("order-statuses/{id:int}")]
    public async Task<IActionResult> DeleteOrderStatus(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        if (await _context.AppOrders.AsNoTracking().AnyAsync(o => o.OrderStatusId == id, cancellationToken))
            return Conflict(InUseMessage + " Деактивация не предусмотрена схемой таблицы order_status.");
        var e = await _context.OrderStatuses.FirstOrDefaultAsync(x => x.StatusId == id, cancellationToken);
        if (e == null) return NotFound();
        _context.OrderStatuses.Remove(e);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("payment-statuses")]
    public async Task<ActionResult<IReadOnlyList<AdminSimpleStatusDictDto>>> PaymentStatuses(CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        var list = await _context.PaymentStatuses.AsNoTracking()
            .OrderBy(x => x.StatusName)
            .Select(x => new AdminSimpleStatusDictDto { Id = x.StatusId, Name = x.StatusName, Description = x.Description })
            .ToListAsync(cancellationToken);
        return Ok(list);
    }

    [HttpPost("payment-statuses")]
    public async Task<ActionResult<AdminSimpleStatusDictDto>> CreatePaymentStatus([FromBody] AdminDictNameDescUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Укажите название");
        var e = new PaymentStatus { StatusName = dto.Name.Trim(), Description = NormDesc(dto) };
        _context.PaymentStatuses.Add(e);
        await _context.SaveChangesAsync(cancellationToken);
        return Ok(new AdminSimpleStatusDictDto { Id = e.StatusId, Name = e.StatusName, Description = e.Description });
    }

    [HttpPut("payment-statuses/{id:int}")]
    public async Task<IActionResult> UpdatePaymentStatus(int id, [FromBody] AdminDictNameDescUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Укажите название");
        var e = await _context.PaymentStatuses.FirstOrDefaultAsync(x => x.StatusId == id, cancellationToken);
        if (e == null) return NotFound();
        e.StatusName = dto.Name.Trim();
        e.Description = NormDesc(dto);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("payment-statuses/{id:int}")]
    public async Task<IActionResult> DeletePaymentStatus(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        if (await _context.Payments.AsNoTracking().AnyAsync(p => p.PaymentStatusId == id, cancellationToken))
            return Conflict(InUseMessage + " Деактивация не предусмотрена схемой таблицы payment_status.");
        var e = await _context.PaymentStatuses.FirstOrDefaultAsync(x => x.StatusId == id, cancellationToken);
        if (e == null) return NotFound();
        _context.PaymentStatuses.Remove(e);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    #endregion

    #region assignment-types, lesson-types

    [HttpGet("assignment-types")]
    public async Task<ActionResult<IReadOnlyList<AdminAssignmentTypeDictDto>>> AssignmentTypes(CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        var list = await _context.AssignmentTypes.AsNoTracking()
            .OrderBy(x => x.TypeName)
            .Select(x => new AdminAssignmentTypeDictDto { TypeId = x.TypeId, TypeName = x.TypeName, Description = x.Description })
            .ToListAsync(cancellationToken);
        return Ok(list);
    }

    [HttpPost("assignment-types")]
    public async Task<ActionResult<AdminAssignmentTypeDictDto>> CreateAssignmentType([FromBody] AdminDictNameDescUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Укажите название");
        var e = new AssignmentType { TypeName = dto.Name.Trim(), Description = NormDesc(dto) };
        _context.AssignmentTypes.Add(e);
        await _context.SaveChangesAsync(cancellationToken);
        return Ok(new AdminAssignmentTypeDictDto { TypeId = e.TypeId, TypeName = e.TypeName, Description = e.Description });
    }

    [HttpPut("assignment-types/{id:int}")]
    public async Task<IActionResult> UpdateAssignmentType(int id, [FromBody] AdminDictNameDescUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Укажите название");
        var e = await _context.AssignmentTypes.FirstOrDefaultAsync(x => x.TypeId == id, cancellationToken);
        if (e == null) return NotFound();
        e.TypeName = dto.Name.Trim();
        e.Description = NormDesc(dto);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("assignment-types/{id:int}")]
    public async Task<IActionResult> DeleteAssignmentType(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        var e = await _context.AssignmentTypes.FirstOrDefaultAsync(x => x.TypeId == id, cancellationToken);
        if (e == null) return NotFound();
        _context.AssignmentTypes.Remove(e);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("lesson-types")]
    public async Task<ActionResult<IReadOnlyList<AdminLessonTypeDictDto>>> LessonTypes(CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        var list = await _context.LessonTypes.AsNoTracking()
            .OrderBy(x => x.TypeName)
            .Select(x => new AdminLessonTypeDictDto { TypeId = x.TypeId, TypeName = x.TypeName, Description = x.Description })
            .ToListAsync(cancellationToken);
        return Ok(list);
    }

    [HttpPost("lesson-types")]
    public async Task<ActionResult<AdminLessonTypeDictDto>> CreateLessonType([FromBody] AdminDictNameDescUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Укажите название");
        var e = new LessonType { TypeName = dto.Name.Trim(), Description = NormDesc(dto) };
        _context.LessonTypes.Add(e);
        await _context.SaveChangesAsync(cancellationToken);
        return Ok(new AdminLessonTypeDictDto { TypeId = e.TypeId, TypeName = e.TypeName, Description = e.Description });
    }

    [HttpPut("lesson-types/{id:int}")]
    public async Task<IActionResult> UpdateLessonType(int id, [FromBody] AdminDictNameDescUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Укажите название");
        var e = await _context.LessonTypes.FirstOrDefaultAsync(x => x.TypeId == id, cancellationToken);
        if (e == null) return NotFound();
        e.TypeName = dto.Name.Trim();
        e.Description = NormDesc(dto);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("lesson-types/{id:int}")]
    public async Task<IActionResult> DeleteLessonType(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        if (await _context.Lessons.AsNoTracking().AnyAsync(l => l.LessonTypeId == id, cancellationToken))
            return Conflict(InUseMessage + " Деактивация не предусмотрена схемой таблицы lesson_type.");
        var e = await _context.LessonTypes.FirstOrDefaultAsync(x => x.TypeId == id, cancellationToken);
        if (e == null) return NotFound();
        _context.LessonTypes.Remove(e);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    #endregion

    #region payment-methods, discount-types

    [HttpGet("payment-methods")]
    public async Task<ActionResult<IReadOnlyList<AdminPaymentMethodDictDto>>> PaymentMethods(CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        var list = await _context.PaymentMethods.AsNoTracking()
            .OrderBy(x => x.MethodName)
            .Select(x => new AdminPaymentMethodDictDto
            {
                MethodId = x.MethodId,
                MethodName = x.MethodName,
                Description = x.Description,
                IsActive = x.IsActive != false
            })
            .ToListAsync(cancellationToken);
        return Ok(list);
    }

    [HttpPost("payment-methods")]
    public async Task<ActionResult<AdminPaymentMethodDictDto>> CreatePaymentMethod([FromBody] AdminPaymentMethodUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        if (string.IsNullOrWhiteSpace(dto.MethodName)) return BadRequest("Укажите название");
        var e = new PaymentMethod
        {
            MethodName = dto.MethodName.Trim(),
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            IsActive = dto.IsActive
        };
        _context.PaymentMethods.Add(e);
        await _context.SaveChangesAsync(cancellationToken);
        return Ok(new AdminPaymentMethodDictDto
        {
            MethodId = e.MethodId,
            MethodName = e.MethodName,
            Description = e.Description,
            IsActive = e.IsActive != false
        });
    }

    [HttpPut("payment-methods/{id:int}")]
    public async Task<IActionResult> UpdatePaymentMethod(int id, [FromBody] AdminPaymentMethodUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        if (string.IsNullOrWhiteSpace(dto.MethodName)) return BadRequest("Укажите название");
        var e = await _context.PaymentMethods.FirstOrDefaultAsync(x => x.MethodId == id, cancellationToken);
        if (e == null) return NotFound();
        e.MethodName = dto.MethodName.Trim();
        e.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        e.IsActive = dto.IsActive;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPatch("payment-methods/{id:int}/active")]
    public async Task<IActionResult> PatchPaymentMethodActive(int id, [FromBody] AdminDictActiveDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        var e = await _context.PaymentMethods.FirstOrDefaultAsync(x => x.MethodId == id, cancellationToken);
        if (e == null) return NotFound();
        e.IsActive = dto.IsActive;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("payment-methods/{id:int}")]
    public async Task<IActionResult> DeletePaymentMethod(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        var used = await _context.AppOrders.AsNoTracking().AnyAsync(o => o.MethodId == id, cancellationToken)
                   || await _context.Payments.AsNoTracking().AnyAsync(p => p.MethodId == id, cancellationToken);
        if (used)
            return Conflict(InUseMessage + " Используйте деактивацию (is_active=false).");
        var e = await _context.PaymentMethods.FirstOrDefaultAsync(x => x.MethodId == id, cancellationToken);
        if (e == null) return NotFound();
        _context.PaymentMethods.Remove(e);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("discount-types")]
    public async Task<ActionResult<IReadOnlyList<AdminDiscountTypeDictDto>>> DiscountTypes(CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        var list = await _context.DiscountTypes.AsNoTracking()
            .OrderBy(x => x.TypeName)
            .Select(x => new AdminDiscountTypeDictDto
            {
                TypeId = x.TypeId,
                TypeName = x.TypeName,
                Description = x.Description,
                IsActive = x.IsActive != false
            })
            .ToListAsync(cancellationToken);
        return Ok(list);
    }

    [HttpPost("discount-types")]
    public async Task<ActionResult<AdminDiscountTypeDictDto>> CreateDiscountType([FromBody] AdminDiscountTypeUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        if (string.IsNullOrWhiteSpace(dto.TypeName)) return BadRequest("Укажите название");
        var e = new DiscountType
        {
            TypeName = dto.TypeName.Trim(),
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            IsActive = dto.IsActive
        };
        _context.DiscountTypes.Add(e);
        await _context.SaveChangesAsync(cancellationToken);
        return Ok(new AdminDiscountTypeDictDto
        {
            TypeId = e.TypeId,
            TypeName = e.TypeName,
            Description = e.Description,
            IsActive = e.IsActive != false
        });
    }

    [HttpPut("discount-types/{id:int}")]
    public async Task<IActionResult> UpdateDiscountType(int id, [FromBody] AdminDiscountTypeUpsertDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        if (string.IsNullOrWhiteSpace(dto.TypeName)) return BadRequest("Укажите название");
        var e = await _context.DiscountTypes.FirstOrDefaultAsync(x => x.TypeId == id, cancellationToken);
        if (e == null) return NotFound();
        e.TypeName = dto.TypeName.Trim();
        e.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        e.IsActive = dto.IsActive;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPatch("discount-types/{id:int}/active")]
    public async Task<IActionResult> PatchDiscountTypeActive(int id, [FromBody] AdminDictActiveDto dto, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        var e = await _context.DiscountTypes.FirstOrDefaultAsync(x => x.TypeId == id, cancellationToken);
        if (e == null) return NotFound();
        e.IsActive = dto.IsActive;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("discount-types/{id:int}")]
    public async Task<IActionResult> DeleteDiscountType(int id, CancellationToken cancellationToken)
    {
        if (AdminAccessGuard.ForbidIfNoAccess(this) is { } forbid) return forbid;
        if (await _context.PromoCodes.AsNoTracking().AnyAsync(p => p.TypeId == id, cancellationToken))
            return Conflict(InUseMessage + " Используйте деактивацию (is_active=false).");
        var e = await _context.DiscountTypes.FirstOrDefaultAsync(x => x.TypeId == id, cancellationToken);
        if (e == null) return NotFound();
        _context.DiscountTypes.Remove(e);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    #endregion
}
