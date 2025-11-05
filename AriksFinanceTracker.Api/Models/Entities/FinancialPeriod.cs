using AriksFinanceTracker.Api.Models.Enums;

namespace AriksFinanceTracker.Api.Models.Entities;

public class FinancialPeriod
{
    public int Id { get; set; }
    public string Name { get; set; }
    public FinancialPeriodType Type { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public string Description { get; set; }
}
