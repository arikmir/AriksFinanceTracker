namespace AriksFinanceTracker.Api.Models.Enums;

public enum AlertType
{
    Info,        // 50% of budget - on track
    Warning,     // 75% of budget
    Critical,    // 90% of budget
    Exceeded,    // 100%+ of budget
    Achievement  // Savings goal met
}
