using AriksFinanceTracker.Api.Models.Enums;

namespace AriksFinanceTracker.Api.Models.Entities;

public class SavingsGoal
{
    public int Id { get; set; }
    public SavingsGoalType Type { get; set; }
    public string Name { get; set; }
    public decimal MonthlyTarget { get; set; }
    public int FinancialPeriodId { get; set; }
    public FinancialPeriod FinancialPeriod { get; set; }
    public bool IsRequired { get; set; } // True for emergency fund
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
