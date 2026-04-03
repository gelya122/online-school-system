using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InstallmentPlansController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public InstallmentPlansController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InstallmentPlanDto>>> GetInstallmentPlans()
    {
        var plans = await _context.InstallmentPlans
            .Select(p => new InstallmentPlanDto
            {
                PlanId = p.PlanId,
                OrderId = p.OrderId,
                TotalAmount = p.TotalAmount,
                InstallmentCount = p.InstallmentCount,
                MonthlyPayment = p.MonthlyPayment,
                NextPaymentDate = p.NextPaymentDate,
                PlanStatus = p.PlanStatus,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();
        return Ok(plans);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InstallmentPlanDto>> GetInstallmentPlan(int id)
    {
        var plan = await _context.InstallmentPlans.FindAsync(id);
        if (plan == null) return NotFound();

        return Ok(new InstallmentPlanDto
        {
            PlanId = plan.PlanId,
            OrderId = plan.OrderId,
            TotalAmount = plan.TotalAmount,
            InstallmentCount = plan.InstallmentCount,
            MonthlyPayment = plan.MonthlyPayment,
            NextPaymentDate = plan.NextPaymentDate,
            PlanStatus = plan.PlanStatus,
            CreatedAt = plan.CreatedAt
        });
    }

    [HttpPost]
    public async Task<ActionResult<InstallmentPlanDto>> CreateInstallmentPlan(CreateInstallmentPlanDto dto)
    {
        var plan = new InstallmentPlan
        {
            OrderId = dto.OrderId,
            TotalAmount = dto.TotalAmount,
            InstallmentCount = dto.InstallmentCount,
            MonthlyPayment = dto.MonthlyPayment,
            NextPaymentDate = dto.NextPaymentDate,
            PlanStatus = dto.PlanStatus
        };

        _context.InstallmentPlans.Add(plan);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetInstallmentPlan), new { id = plan.PlanId }, new InstallmentPlanDto
        {
            PlanId = plan.PlanId,
            OrderId = plan.OrderId,
            TotalAmount = plan.TotalAmount,
            InstallmentCount = plan.InstallmentCount,
            MonthlyPayment = plan.MonthlyPayment,
            NextPaymentDate = plan.NextPaymentDate,
            PlanStatus = plan.PlanStatus,
            CreatedAt = plan.CreatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateInstallmentPlan(int id, UpdateInstallmentPlanDto dto)
    {
        var plan = await _context.InstallmentPlans.FindAsync(id);
        if (plan == null) return NotFound();

        if (dto.NextPaymentDate.HasValue) plan.NextPaymentDate = dto.NextPaymentDate;
        if (dto.PlanStatus != null) plan.PlanStatus = dto.PlanStatus;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteInstallmentPlan(int id)
    {
        var plan = await _context.InstallmentPlans.FindAsync(id);
        if (plan == null) return NotFound();

        _context.InstallmentPlans.Remove(plan);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

