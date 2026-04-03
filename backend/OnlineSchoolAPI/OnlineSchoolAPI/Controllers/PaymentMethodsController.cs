using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;

namespace OnlineSchoolAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentMethodsController : ControllerBase
{
    private readonly OnlineSchoolDbContext _context;

    public PaymentMethodsController(OnlineSchoolDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Возвращает уникальные значения payment_method из заказов.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<string>>> GetPaymentMethods()
    {
        var appOrderMethods = _context.AppOrders
            .Where(o => o.PaymentMethod != null && o.PaymentMethod != "")
            .Select(o => o.PaymentMethod!);

        var paymentMethods = _context.Payments
            .Where(p => p.PaymentMethod != null && p.PaymentMethod != "")
            .Select(p => p.PaymentMethod!);

        var methods = await appOrderMethods
            .Union(paymentMethods)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

        // Если в БД пока нет данных (таблицы пустые/NULL), вернем базовый набор
        // чтобы форма checkout не была пустой.
        if (methods.Count == 0)
        {
            methods = new List<string> { "Банковская карта", "Онлайн‑кошелёк", "Безналичный расчёт" };
        }

        return Ok(methods);
    }
}

