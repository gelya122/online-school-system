namespace OnlineSchoolAPI.Dto;

public class CheckoutLineRequestDto
{
    public int CourseId { get; set; }
    public int? InstanceId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class CheckoutRequestDto
{
    /// <summary>Идентификатор пользователя (users.user_id); профиль student ищется по user_id.</summary>
    public int UserId { get; set; }

    public int? MethodId { get; set; }

    /// <summary>Строка промокода; если указана — скидка пересчитывается на сервере, при успехе увеличивается current_uses.</summary>
    public string? PromoCode { get; set; }

    /// <summary>Оплата рассрочкой: создаются installment_plan и installment_payment.</summary>
    public bool UseInstallment { get; set; }

    /// <summary>Количество платежей по рассрочке (не менее 2).</summary>
    public int? InstallmentCount { get; set; }

    public List<CheckoutLineRequestDto> Items { get; set; } = new();
}

public class CheckoutResponseDto
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public int? PromoCodeId { get; set; }
    public string? PromoMessage { get; set; }
}
