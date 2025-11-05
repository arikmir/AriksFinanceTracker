using AriksFinanceTracker.Api.Models.Enums;

namespace AriksFinanceTracker.Api.Models.Dto;

public class ExpenseAnalyticsDto
{
    public decimal TotalAmount { get; set; }
    public int TransactionCount { get; set; }
    public decimal AverageAmount { get; set; }
    public Dictionary<ExpenseCategory, decimal> CategoryBreakdown { get; set; } = new();
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
