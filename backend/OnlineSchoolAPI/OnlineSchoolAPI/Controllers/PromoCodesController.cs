// OnlineSchoolAPI/Controllers/PromoCodesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;
using OnlineSchoolAPI.Models;

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

    [HttpPost("validate")]
    public async Task<ActionResult<ValidatePromoCodeResponseDto>> ValidatePromoCode([FromBody] ValidatePromoCodeDto dto)
    {
        // Проверка на пустой код
        if (dto == null || string.IsNullOrWhiteSpace(dto.Code))
        {
            return Ok(new ValidatePromoCodeResponseDto
            {
                IsValid = false,
                Message = "Введите промокод",
                DiscountAmount = 0m
            });
        }

        // Проверка корзины
        if (dto.CartTotal <= 0)
        {
            return Ok(new ValidatePromoCodeResponseDto
            {
                IsValid = false,
                Message = "Корзина пуста. Добавьте товары для применения промокода",
                DiscountAmount = 0m
            });
        }

        // Нормализуем код для сравнения без учета регистра
        var codeNormalized = dto.Code.Trim();

        // Получаем промокод из базы данных
        var promo = await _context.PromoCodes
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

        // Проверка активности
        if (promo.IsActive.HasValue && promo.IsActive.Value == false)
        {
            return Ok(new ValidatePromoCodeResponseDto
            {
                IsValid = false,
                Message = "Промокод деактивирован",
                DiscountAmount = 0m
            });
        }

        var today = DateOnly.FromDateTime(DateTime.Today);

        // Проверка даты начала действия
        if (today < promo.ValidFrom)
        {
            return Ok(new ValidatePromoCodeResponseDto
            {
                IsValid = false,
                Message = $"Промокод начнёт действовать с {promo.ValidFrom:dd.MM.yyyy}",
                DiscountAmount = 0m
            });
        }

        // Проверка даты окончания действия
        if (promo.ValidUntil.HasValue && today > promo.ValidUntil.Value)
        {
            return Ok(new ValidatePromoCodeResponseDto
            {
                IsValid = false,
                Message = "Срок действия промокода истёк",
                DiscountAmount = 0m
            });
        }

        // Проверка лимита использований
        if (promo.MaxUses.HasValue)
        {
            var currentUses = promo.CurrentUses ?? 0;
            if (currentUses >= promo.MaxUses.Value)
            {
                return Ok(new ValidatePromoCodeResponseDto
                {
                    IsValid = false,
                    Message = "Лимит использований промокода исчерпан",
                    DiscountAmount = 0m
                });
            }
        }

        // Получаем тип промокода
        string? typeName = null;
        if (promo.TypeId.HasValue)
        {
            var discountType = await _context.DiscountTypes
                .FirstOrDefaultAsync(t => t.TypeId == promo.TypeId.Value);
            typeName = discountType?.TypeName;
        }

        // Рассчитываем скидку в зависимости от типа
        decimal discountAmount = 0;
        string successMessage = "";

        var normalizedTypeName = (typeName ?? string.Empty).Trim().ToLowerInvariant();

        // Проверяем тип промокода
        if (normalizedTypeName.Contains("free") ||
            normalizedTypeName.Contains("бесплатно") ||
            normalizedTypeName.Contains("full") ||
            normalizedTypeName.Contains("полный"))
        {
            // Бесплатный доступ - скидка на всю сумму
            discountAmount = dto.CartTotal;
            successMessage = $"Промокод применён! Полная стоимость ({dto.CartTotal:N0} ₽) списана. Итоговая сумма: 0 ₽";
        }
        else if (normalizedTypeName.Contains("percent") ||
                 normalizedTypeName.Contains("%") ||
                 normalizedTypeName.Contains("процент") ||
                 normalizedTypeName.Contains("percentage"))
        {
            // Процентная скидка
            discountAmount = dto.CartTotal * promo.DiscountValue / 100m;
            successMessage = $"Промокод применён! Скидка {promo.DiscountValue}% ({discountAmount:N0} ₽)";
        }
        else if (normalizedTypeName.Contains("fixed") ||
                 normalizedTypeName.Contains("flat") ||
                 normalizedTypeName.Contains("amount") ||
                 normalizedTypeName.Contains("руб") ||
                 normalizedTypeName.Contains("сумм"))
        {
            // Фиксированная скидка
            discountAmount = promo.DiscountValue;
            // Скидка не может превышать сумму корзины
            if (discountAmount > dto.CartTotal)
            {
                discountAmount = dto.CartTotal;
            }
            successMessage = $"Промокод применён! Скидка {promo.DiscountValue:N0} ₽";
        }
        else
        {
            // Если тип не определён, считаем по значению discountValue
            if (promo.DiscountValue <= 100)
            {
                // Процентная скидка
                discountAmount = dto.CartTotal * promo.DiscountValue / 100m;
                successMessage = $"Промокод применён! Скидка {promo.DiscountValue}% ({discountAmount:N0} ₽)";
            }
            else if (promo.DiscountValue >= dto.CartTotal)
            {
                // Бесплатный доступ
                discountAmount = dto.CartTotal;
                successMessage = $"Промокод применён! Полная стоимость ({dto.CartTotal:N0} ₽) списана. Итоговая сумма: 0 ₽";
            }
            else
            {
                // Фиксированная скидка
                discountAmount = promo.DiscountValue;
                successMessage = $"Промокод применён! Скидка {promo.DiscountValue:N0} ₽";
            }
        }

        // Округляем до целых рублей вверх для большей прозрачности
        discountAmount = Math.Ceiling(discountAmount);

        // Убеждаемся, что скидка не превышает сумму корзины
        if (discountAmount > dto.CartTotal)
        {
            discountAmount = dto.CartTotal;
        }

        // Обновляем количество использований промокода
        promo.CurrentUses = (promo.CurrentUses ?? 0) + 1;
        await _context.SaveChangesAsync();

        return Ok(new ValidatePromoCodeResponseDto
        {
            IsValid = true,
            Message = successMessage,
            DiscountAmount = discountAmount
        });
    }
}