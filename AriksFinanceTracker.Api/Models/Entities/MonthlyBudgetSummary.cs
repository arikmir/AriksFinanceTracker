namespace AriksFinanceTracker.Api.Models.Entities;

public class MonthlyBudgetSummary
{
    public int Id { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal TotalSavings { get; set; }
    public decimal SavingsRate { get; set; } // Percentage
    public decimal BudgetAdherenceScore { get; set; } // 0-100
    public string FinancialHealthGrade { get; set; } // Excellent, Good, Fair, Poor
    public int FinancialPeriodId { get; set; }
    public FinancialPeriod FinancialPeriod { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
