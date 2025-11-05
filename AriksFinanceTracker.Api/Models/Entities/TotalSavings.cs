namespace AriksFinanceTracker.Api.Models.Entities;

public class TotalSavings
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public string Category { get; set; } // e.g., "Emergency Fund", "Investment", "Goal Savings"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
