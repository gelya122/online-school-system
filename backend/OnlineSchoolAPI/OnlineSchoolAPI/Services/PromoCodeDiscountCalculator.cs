using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI.Services;

/// <summary>
/// Единая логика расчёта скидки по промокоду (проверка + сумма). Без изменения БД.
/// </summary>
public static class PromoCodeDiscountCalculator
{
    public static (bool IsValid, string Message, decimal DiscountAmount) Compute(
        PromoCode promo,
        string? discountTypeName,
        decimal cartTotal)
    {
        if (cartTotal <= 0)
            return (false, "Корзина пуста", 0m);

        if (promo.IsActive == false)
            return (false, "Промокод деактивирован", 0m);

        var today = DateOnly.FromDateTime(DateTime.Today);
        if (today < promo.ValidFrom)
            return (false, $"Промокод начнёт действовать с {promo.ValidFrom:dd.MM.yyyy}", 0m);

        if (promo.ValidUntil.HasValue && today > promo.ValidUntil.Value)
            return (false, "Срок действия промокода истёк", 0m);

        if (promo.MaxUses.HasValue)
        {
            var currentUses = promo.CurrentUses ?? 0;
            if (currentUses >= promo.MaxUses.Value)
                return (false, "Лимит использований промокода исчерпан", 0m);
        }

        var normalizedTypeName = (discountTypeName ?? string.Empty).Trim().ToLowerInvariant();
        decimal discountAmount;
        string successMessage;

        if (normalizedTypeName.Contains("free") ||
            normalizedTypeName.Contains("бесплатно") ||
            normalizedTypeName.Contains("full") ||
            normalizedTypeName.Contains("полный"))
        {
            discountAmount = cartTotal;
            successMessage = $"Промокод применён! Полная стоимость ({cartTotal:N0} ₽) списана.";
        }
        else if (normalizedTypeName.Contains("percent") ||
                 normalizedTypeName.Contains("%") ||
                 normalizedTypeName.Contains("процент") ||
                 normalizedTypeName.Contains("percentage"))
        {
            discountAmount = cartTotal * promo.DiscountValue / 100m;
            successMessage = $"Промокод применён! Скидка {promo.DiscountValue}% ({discountAmount:N0} ₽)";
        }
        else if (normalizedTypeName.Contains("fixed") ||
                 normalizedTypeName.Contains("flat") ||
                 normalizedTypeName.Contains("amount") ||
                 normalizedTypeName.Contains("руб") ||
                 normalizedTypeName.Contains("сумм"))
        {
            discountAmount = promo.DiscountValue;
            if (discountAmount > cartTotal)
                discountAmount = cartTotal;
            successMessage = $"Промокод применён! Скидка {promo.DiscountValue:N0} ₽";
        }
        else
        {
            if (promo.DiscountValue <= 100)
            {
                discountAmount = cartTotal * promo.DiscountValue / 100m;
                successMessage = $"Промокод применён! Скидка {promo.DiscountValue}% ({discountAmount:N0} ₽)";
            }
            else if (promo.DiscountValue >= cartTotal)
            {
                discountAmount = cartTotal;
                successMessage = $"Промокод применён! Полная стоимость ({cartTotal:N0} ₽) списана.";
            }
            else
            {
                discountAmount = promo.DiscountValue;
                successMessage = $"Промокод применён! Скидка {promo.DiscountValue:N0} ₽";
            }
        }

        discountAmount = Math.Ceiling(discountAmount);
        if (discountAmount > cartTotal)
            discountAmount = cartTotal;

        return (true, successMessage, discountAmount);
    }
}
