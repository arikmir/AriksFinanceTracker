using AriksFinanceTracker.Api.Models.Entities;

namespace AriksFinanceTracker.Api.Models.Dto;

public class DailyExpenseDto
{
    public DateTime Date { get; set; }
    public decimal TotalAmount { get; set; }
    public int TransactionCount { get; set; }
    public List<Expense> Expenses { get; set; } = new();
}
