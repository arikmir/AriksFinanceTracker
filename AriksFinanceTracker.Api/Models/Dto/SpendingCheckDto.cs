using AriksFinanceTracker.Api.Models.Enums;

namespace AriksFinanceTracker.Api.Models.Dto;

public class SpendingCheckDto
{
    public bool IsAllowed { get; set; }
    public string Message { get; set; }
    public AlertType? AlertLevel { get; set; }
    public decimal RemainingBudget { get; set; }
    public decimal NewPercentageUsed { get; set; }
    public string Encouragement { get; set; }
}
