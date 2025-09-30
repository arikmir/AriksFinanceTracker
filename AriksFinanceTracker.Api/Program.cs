using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework
builder.Services.AddDbContext<FinanceContext>(options =>
    options.UseSqlite("Data Source=ariks_finance.db"));

// Add Budget Service
builder.Services.AddScoped<AriksFinanceTracker.Api.Services.BudgetService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        builder => builder.WithOrigins("http://localhost:4201")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

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
    
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<Income> Incomes { get; set; }
    public DbSet<FinancialPeriod> FinancialPeriods { get; set; }
    public DbSet<BudgetLimit> BudgetLimits { get; set; }
    public DbSet<SavingsGoal> SavingsGoals { get; set; }
    public DbSet<SpendingAlert> SpendingAlerts { get; set; }
    public DbSet<MonthlyBudgetSummary> MonthlyBudgetSummaries { get; set; }
}

public enum ExpenseCategory
{
    FoodAndDrinks,
    Groceries,
    Shopping,
    Transport,
    Entertainment,
    Utilities,
    HealthAndFitness,
    Home,
    Savings,
    Repayment,
    Miscellaneous,
    Mortgage,
    Rent
}

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

public class Income
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Source { get; set; }
}

public class ExpenseAnalyticsDto
{
    public decimal TotalAmount { get; set; }
    public int TransactionCount { get; set; }
    public decimal AverageAmount { get; set; }
    public Dictionary<ExpenseCategory, decimal> CategoryBreakdown { get; set; } = new();
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
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
    public ExpenseCategory Category { get; set; }
    public string CategoryName { get; set; }
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
    public ExpenseCategory Category { get; set; }
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
    public ExpenseCategory Category { get; set; }
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
    public ExpenseCategory Category { get; set; }
    public string CategoryName { get; set; }
    public decimal Limit { get; set; }
    public decimal Spent { get; set; }
    public decimal Remaining { get; set; }
    public decimal PercentageUsed { get; set; }
    public decimal DailyRecommendation { get; set; }
    public bool IsEssential { get; set; }
    public string Status { get; set; } // Great, Good, Warning, Critical
    public string StatusColor { get; set; } // green, blue, yellow, orange
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
