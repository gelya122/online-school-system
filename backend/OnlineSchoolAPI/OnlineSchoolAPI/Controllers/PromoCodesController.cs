// OnlineSchoolAPI/Controllers/PromoCodesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;
using OnlineSchoolAPI.Services;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PromoCodesController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public PromoCodesController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PromoCodeDto>>> GetPromoCodes()
    {
        var codes = await _context.PromoCodes
            .Select(p => new PromoCodeDto
            {
                PromoCodeId = p.PromoCodeId,
                Code = p.Code,
                TypeId = p.TypeId,
                DiscountValue = p.DiscountValue,
                ValidFrom = p.ValidFrom,
                ValidUntil = p.ValidUntil,
                MaxUses = p.MaxUses,
                CurrentUses = p.CurrentUses,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();
        return Ok(codes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PromoCodeDto>> GetPromoCode(int id)
    {
        var code = await _context.PromoCodes.FindAsync(id);
        if (code == null) return NotFound();

        return Ok(new PromoCodeDto
        {
            PromoCodeId = code.PromoCodeId,
            Code = code.Code,
            TypeId = code.TypeId,
            DiscountValue = code.DiscountValue,
            ValidFrom = code.ValidFrom,
            ValidUntil = code.ValidUntil,
            MaxUses = code.MaxUses,
            CurrentUses = code.CurrentUses,
            IsActive = code.IsActive,
            CreatedAt = code.CreatedAt
        });
    }

    [HttpPost]
    public async Task<ActionResult<PromoCodeDto>> CreatePromoCode(CreatePromoCodeDto dto)
    {
        var code = new PromoCode
        {
            Code = dto.Code,
            TypeId = dto.TypeId,
            DiscountValue = dto.DiscountValue,
            ValidFrom = dto.ValidFrom,
            ValidUntil = dto.ValidUntil,
            MaxUses = dto.MaxUses,
            CurrentUses = 0,
            IsActive = dto.IsActive ?? true,
            CreatedAt = DateTime.UtcNow
        };

        _context.PromoCodes.Add(code);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPromoCode), new { id = code.PromoCodeId }, new PromoCodeDto
        {
            PromoCodeId = code.PromoCodeId,
            Code = code.Code,
            TypeId = code.TypeId,
            DiscountValue = code.DiscountValue,
            ValidFrom = code.ValidFrom,
            ValidUntil = code.ValidUntil,
            MaxUses = code.MaxUses,
            CurrentUses = code.CurrentUses,
            IsActive = code.IsActive,
            CreatedAt = code.CreatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePromoCode(int id, UpdatePromoCodeDto dto)
    {
        var code = await _context.PromoCodes.FindAsync(id);
        if (code == null) return NotFound();

        if (dto.TypeId.HasValue) code.TypeId = dto.TypeId.Value;
        if (dto.DiscountValue.HasValue) code.DiscountValue = dto.DiscountValue.Value;
        if (dto.ValidFrom.HasValue) code.ValidFrom = dto.ValidFrom.Value;
        if (dto.ValidUntil.HasValue) code.ValidUntil = dto.ValidUntil;
        if (dto.MaxUses.HasValue) code.MaxUses = dto.MaxUses;
        if (dto.IsActive.HasValue) code.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePromoCode(int id)
    {
        var code = await _context.PromoCodes.FindAsync(id);
        if (code == null) return NotFound();

        _context.PromoCodes.Remove(code);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Проверка промокода и расчёт скидки. Счётчик использований в БД не меняется — он увеличивается при успешном оформлении заказа (POST /api/Checkout).</summary>
    [HttpPost("validate")]
    public async Task<ActionResult<ValidatePromoCodeResponseDto>> ValidatePromoCode([FromBody] ValidatePromoCodeDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Code))
        {
            return Ok(new ValidatePromoCodeResponseDto
            {
                IsValid = false,
                Message = "Введите промокод",
                DiscountAmount = 0m
            });
        }

        if (dto.CartTotal <= 0)
        {
            return Ok(new ValidatePromoCodeResponseDto
            {
                IsValid = false,
                Message = "Корзина пуста. Добавьте товары для применения промокода",
                DiscountAmount = 0m
            });
        }

        var codeNormalized = dto.Code.Trim();

        var promo = await _context.PromoCodes
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Code != null && p.Code.ToLower() == codeNormalized.ToLower());

        if (promo == null)
        {
            return Ok(new ValidatePromoCodeResponseDto
            {
                IsValid = false,
                Message = "Промокод не найден",
                DiscountAmount = 0m
            });
        }

        string? typeName = null;
        if (promo.TypeId.HasValue)
        {
            typeName = await _context.DiscountTypes.AsNoTracking()
                .Where(t => t.TypeId == promo.TypeId.Value)
                .Select(t => t.TypeName)
                .FirstOrDefaultAsync();
        }

        var (ok, message, discountAmount) = PromoCodeDiscountCalculator.Compute(promo, typeName, dto.CartTotal);

        return Ok(new ValidatePromoCodeResponseDto
        {
            IsValid = ok,
            Message = message,
            DiscountAmount = discountAmount
        });
    }
}