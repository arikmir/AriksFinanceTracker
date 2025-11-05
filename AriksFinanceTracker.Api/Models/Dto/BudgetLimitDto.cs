using AriksFinanceTracker.Api.Models.Enums;

namespace AriksFinanceTracker.Api.Models.Dto;

public class BudgetLimitDto
{
    public ExpenseCategory Category { get; set; }
    public string CategoryName { get; set; }
    public decimal MonthlyLimit { get; set; }
    public bool IsEssential { get; set; }
}
