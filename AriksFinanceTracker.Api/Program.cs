using AriksFinanceTracker.Api.Data;
using AriksFinanceTracker.Api.Services;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework
builder.Services.AddDbContext<FinanceContext>(options =>
    options.UseSqlite("Data Source=ariks_finance.db"));

// Add Services
builder.Services.AddScoped<AriksFinanceTracker.Api.Services.BudgetService>();
builder.Services.AddScoped<AriksFinanceTracker.Api.Services.ExpenseService>();
builder.Services.AddScoped<AriksFinanceTracker.Api.Services.IncomeService>();
builder.Services.AddScoped<AriksFinanceTracker.Api.Services.IDatabaseBackupService, AriksFinanceTracker.Api.Services.DatabaseBackupService>();
builder.Services.AddHostedService<AriksFinanceTracker.Api.Services.AutoBackupService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        builder => builder.WithOrigins("http://localhost:4201")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<FinanceContext>();
    context.Database.EnsureCreated();

    var budgetService = scope.ServiceProvider.GetRequiredService<BudgetService>();
    await budgetService.InitializeAriksBudgetAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");
app.UseAuthorization();
app.MapControllers();

app.Run();

public class FinanceContext : DbContext
{
    public FinanceContext(DbContextOptions<FinanceContext> options) : base(options) { }
    
    public DbSet<SpendingCategory> SpendingCategories { get; set; }
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<Income> Incomes { get; set; }
    public DbSet<FinancialPeriod> FinancialPeriods { get; set; }
    public DbSet<BudgetLimit> BudgetLimits { get; set; }
    public DbSet<SavingsGoal> SavingsGoals { get; set; }
    public DbSet<SpendingAlert> SpendingAlerts { get; set; }
    public DbSet<MonthlyBudgetSummary> MonthlyBudgetSummaries { get; set; }
    public DbSet<TotalSavings> TotalSavings { get; set; }
}

public class SpendingCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public bool IsSystem { get; set; }
    public bool IsEssentialDefault { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public class Expense
{
    public int Id { get; set; }
    [Required]
    public DateTime Date { get; set; }
    [Required]
    public decimal Amount { get; set; }
    [Required]
    public int CategoryId { get; set; }
    public SpendingCategory? Category { get; set; }
    [Required]
    public string Description { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; }
    public string? Location { get; set; }
    public string? Tags { get; set; }
    public bool IsRecurring { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public class Income
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Source { get; set; }
    public string? Notes { get; set; }
}

public class ExpenseAnalyticsDto
{
    public decimal TotalAmount { get; set; }
    public int TransactionCount { get; set; }
    public decimal AverageAmount { get; set; }
    public List<CategoryBreakdownDto> CategoryBreakdown { get; set; } = new();
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class CategoryBreakdownDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int TransactionCount { get; set; }
}

public class DailyExpenseDto
{
    public DateTime Date { get; set; }
    public decimal TotalAmount { get; set; }
    public int TransactionCount { get; set; }
    public List<Expense> Expenses { get; set; } = new();
}

public class CategorySummaryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; }
    public decimal TotalAmount { get; set; }
    public int TransactionCount { get; set; }
    public decimal Percentage { get; set; }
}

public class PaymentMethodSummaryDto
{
    public string PaymentMethod { get; set; } = "Unspecified";
    public decimal TotalAmount { get; set; }
    public int TransactionCount { get; set; }
    public decimal Percentage { get; set; }
}

// Budget System Models
public enum FinancialPeriodType
{
    DoubleHousingPeriod, // Before Feb 2025
    NewHomePeriod        // After Feb 2025
}

public enum AlertType
{
    Info,        // 50% of budget - on track
    Warning,     // 75% of budget
    Critical,    // 90% of budget
    Exceeded,    // 100%+ of budget
    Achievement  // Savings goal met
}

public enum SavingsGoalType
{
    EmergencyFund,
    Investments,
    HouseFuture
}

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

public class BudgetLimit
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public SpendingCategory Category { get; set; } = null!;
    public decimal MonthlyLimit { get; set; }
    public int FinancialPeriodId { get; set; }
    public FinancialPeriod FinancialPeriod { get; set; }
    public bool IsEssential { get; set; } // True for Mortgage, Rent, Utilities, Groceries, Transport
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class SavingsGoal
{
    public int Id { get; set; }
    public SavingsGoalType Type { get; set; }
    public string Name { get; set; }
    public decimal MonthlyTarget { get; set; }
    public int FinancialPeriodId { get; set; }
    public FinancialPeriod FinancialPeriod { get; set; }
    public bool IsRequired { get; set; } // True for emergency fund
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
   
}

public class SpendingAlert
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public AlertType Type { get; set; }
    public string Message { get; set; }
    
    public decimal CurrentSpending { get; set; }
    public decimal BudgetLimit { get; set; }
    public decimal PercentageUsed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;
    public bool IsActive { get; set; } = true;
}

public class MonthlyBudgetSummary
{
    public int Id { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal TotalSavings { get; set; }
    public decimal SavingsRate { get; set; } // Percentage
    public decimal BudgetAdherenceScore { get; set; } // 0-100
    public string FinancialHealthGrade { get; set; } // Excellent, Good, Fair, Poor
    public int FinancialPeriodId { get; set; }
    public FinancialPeriod FinancialPeriod { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// Budget DTOs
public class BudgetStatusDto
{
    public string CurrentPeriod { get; set; }
    public string PeriodDescription { get; set; }
    public decimal MonthlyIncome { get; set; } = 8000m; // Arik's income
    public decimal TotalBudgeted { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal RemainingBudget { get; set; }
    public decimal SavingsTarget { get; set; }
    public decimal ActualSavings { get; set; }
    public decimal SavingsRate { get; set; }
    public int DaysLeftInMonth { get; set; }
    public List<CategoryBudgetDto> CategoryBudgets { get; set; } = new();
    public List<SavingsProgressDto> SavingsProgress { get; set; } = new();
    public string MotivationalMessage { get; set; }
}

public class CategoryBudgetDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; }
    public decimal Limit { get; set; }
    public decimal Spent { get; set; }
    public decimal Remaining { get; set; }
    public decimal PercentageUsed { get; set; }
    public bool IsEssential { get; set; }
    public string Status { get; set; } // Great, Good, Warning, Critical
    public string StatusColor { get; set; } // green, blue, yellow, orange
    public bool IsCustom { get; set; }
    public string? Icon { get; set; }
    public decimal DailyRecommendation { get; set; }
}

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

public class SpendingCheckDto
{
    public bool IsAllowed { get; set; }
    public string Message { get; set; }
    public AlertType? AlertLevel { get; set; }
    public decimal RemainingBudget { get; set; }
    public decimal NewPercentageUsed { get; set; }
    public string Encouragement { get; set; }
}

public class BudgetLimitDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; }
    public decimal MonthlyLimit { get; set; }
    public bool IsEssential { get; set; }
    public bool IsCustom { get; set; }
    public string? Icon { get; set; }
}

public class SpendingCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public bool IsCustom { get; set; }
    public bool IsEssentialDefault { get; set; }
}

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
