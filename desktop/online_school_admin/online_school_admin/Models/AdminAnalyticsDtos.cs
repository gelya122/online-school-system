namespace online_school_admin.Models;

public sealed class AdminAnalyticsSummaryDto
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public int NewApplications { get; set; }
    public decimal ApplicationToStudentConversion { get; set; }
    public decimal ApplicationToPaymentConversion { get; set; }
    public decimal Revenue { get; set; }
    public int PaidOrders { get; set; }
    public int UnpaidOrders { get; set; }
    public int ActiveStudents { get; set; }
    public int ActiveStreams { get; set; }
    public int AverageProgressPercent { get; set; }
    public decimal? AverageHomeworkScore { get; set; }
    public int HomeworkOnReview { get; set; }
    public int PromoCodeUsages { get; set; }
    public decimal PromoCodeDiscountTotal { get; set; }
}

public sealed class AdminDateCountPointDto
{
    public DateOnly Date { get; set; }
    public int Count { get; set; }
}

public sealed class AdminDateAmountPointDto
{
    public DateOnly Date { get; set; }
    public decimal Amount { get; set; }
}

public sealed class AdminNameCountPointDto
{
    public string Name { get; set; } = "";
    public int Count { get; set; }
}

public sealed class AdminNamePercentPointDto
{
    public string Name { get; set; } = "";
    public int Percent { get; set; }
}

public sealed class AdminPromoCodeUsageAggDto
{
    public string Code { get; set; } = "";
    public int Uses { get; set; }
    public decimal DiscountTotal { get; set; }
}

