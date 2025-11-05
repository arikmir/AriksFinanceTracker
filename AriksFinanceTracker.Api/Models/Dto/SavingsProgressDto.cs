using AriksFinanceTracker.Api.Models.Enums;

namespace AriksFinanceTracker.Api.Models.Dto;

public class SavingsProgressDto
{
    public SavingsGoalType Type { get; set; }
    public string Name { get; set; }
    public decimal Target { get; set; }
    public decimal Actual { get; set; }
    public decimal Progress { get; set; } // Percentage
    public bool IsAchieved { get; set; }
    public string MotivationalMessage { get; set; }
}
