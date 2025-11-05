using AriksFinanceTracker.Api.Models.Enums;

namespace AriksFinanceTracker.Api.Models.Dto;

public class CategoryBudgetDto
{
    public ExpenseCategory Category { get; set; }
    public string CategoryName { get; set; }
    public decimal Limit { get; set; }
    public decimal Spent { get; set; }
    public decimal Remaining { get; set; }
    public decimal PercentageUsed { get; set; }
    public bool IsEssential { get; set; }
    public string Status { get; set; } // Great, Good, Warning, Critical
    public string StatusColor { get; set; } // green, blue, yellow, orange
}
