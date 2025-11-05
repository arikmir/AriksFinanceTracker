using AriksFinanceTracker.Api.Models.Enums;

namespace AriksFinanceTracker.Api.Models.Entities;

public class Expense
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public ExpenseCategory Category { get; set; }
    public string Description { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Location { get; set; }
    public string? Tags { get; set; }
    public bool IsRecurring { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
