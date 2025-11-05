namespace AriksFinanceTracker.Api.Models.Dto;

public class FinancialHealthDto
{
    public string Grade { get; set; } // Excellent, Good, Fair, Poor
    public decimal SavingsRate { get; set; }
    public decimal Score { get; set; } // 0-100
    public string Message { get; set; }
    public List<string> Achievements { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public bool IsOnTrack { get; set; }
}
