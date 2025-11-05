using AriksFinanceTracker.Api.Models.Enums;

namespace AriksFinanceTracker.Api.Models.Entities;

public class SpendingAlert
{
    public int Id { get; set; }
    public ExpenseCategory Category { get; set; }
    public AlertType Type { get; set; }
    public string Message { get; set; }

    public decimal CurrentSpending { get; set; }
    public decimal BudgetLimit { get; set; }
    public decimal PercentageUsed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;
    public bool IsActive { get; set; } = true;
}
