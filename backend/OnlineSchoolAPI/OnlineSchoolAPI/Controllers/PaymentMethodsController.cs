using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Dto;

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
    /// Активные способы оплаты из таблицы payment_method.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaymentMethodDto>>> GetPaymentMethods()
    {
        var list = await _context.PaymentMethods
            .Where(m => m.IsActive != false)
            .OrderBy(m => m.MethodName)
            .Select(m => new PaymentMethodDto
            {
                MethodId = m.MethodId,
                MethodName = m.MethodName,
                Description = m.Description
            })
            .ToListAsync();

        return Ok(list);
    }
}
