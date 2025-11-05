using AriksFinanceTracker.Api.Models.Enums;

namespace AriksFinanceTracker.Api.Models.Entities;

public class BudgetLimit
{
    public int Id { get; set; }
    public ExpenseCategory Category { get; set; }
    public decimal MonthlyLimit { get; set; }
    public int FinancialPeriodId { get; set; }
    public FinancialPeriod FinancialPeriod { get; set; }
    public bool IsEssential { get; set; } // True for Mortgage, Rent, Utilities, Groceries, Transport
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
