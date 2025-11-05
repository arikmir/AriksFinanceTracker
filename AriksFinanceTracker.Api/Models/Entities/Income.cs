namespace AriksFinanceTracker.Api.Models.Entities;

public class Income
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Source { get; set; }
    public string? Notes { get; set; }
}
