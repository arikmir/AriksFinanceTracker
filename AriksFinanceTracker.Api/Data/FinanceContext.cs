using AriksFinanceTracker.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AriksFinanceTracker.Api.Data;

public class FinanceContext : DbContext
{
    public FinanceContext(DbContextOptions<FinanceContext> options) : base(options) { }

    public DbSet<Expense> Expenses { get; set; }
    public DbSet<Income> Incomes { get; set; }
    public DbSet<FinancialPeriod> FinancialPeriods { get; set; }
    public DbSet<BudgetLimit> BudgetLimits { get; set; }
    public DbSet<SavingsGoal> SavingsGoals { get; set; }
    public DbSet<SpendingAlert> SpendingAlerts { get; set; }
    public DbSet<MonthlyBudgetSummary> MonthlyBudgetSummaries { get; set; }
    public DbSet<TotalSavings> TotalSavings { get; set; }
}
