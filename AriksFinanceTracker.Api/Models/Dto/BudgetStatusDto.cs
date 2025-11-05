namespace AriksFinanceTracker.Api.Models.Dto;

public class BudgetStatusDto
{
    public string CurrentPeriod { get; set; }
    public string PeriodDescription { get; set; }
    public decimal MonthlyIncome { get; set; } = 8000m; // Arik's income
    public decimal TotalBudgeted { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal RemainingBudget { get; set; }
    public decimal SavingsTarget { get; set; }
    public decimal ActualSavings { get; set; }
    public decimal SavingsRate { get; set; }
    public int DaysLeftInMonth { get; set; }
    public List<CategoryBudgetDto> CategoryBudgets { get; set; } = new();
    public List<SavingsProgressDto> SavingsProgress { get; set; } = new();
    public string MotivationalMessage { get; set; }
}
