using Microsoft.EntityFrameworkCore;

namespace AriksFinanceTracker.Api.Services;

public class BudgetService
{
    private readonly FinanceContext _context;
    private const decimal MONTHLY_INCOME = 8000m;

    public BudgetService(FinanceContext context)
    {
        _context = context;
    }

    public async Task InitializeAriksBudgetAsync()
    {
        // Check if already initialized
        if (await _context.FinancialPeriods.AnyAsync())
            return;

        // Create financial periods
        var doubleHousingPeriod = new FinancialPeriod
        {
            Name = "Double Housing Period",
            Type = FinancialPeriodType.DoubleHousingPeriod,
            StartDate = new DateTime(2024, 1, 1),
            EndDate = new DateTime(2025, 1, 31),
            IsActive = DateTime.Today <= new DateTime(2025, 1, 31),
            Description = "Paying both mortgage and rent - tighter budget but manageable!"
        };

        var newHomePeriod = new FinancialPeriod
        {
            Name = "New Home Period",
            Type = FinancialPeriodType.NewHomePeriod,
            StartDate = new DateTime(2025, 2, 1),
            EndDate = new DateTime(2030, 12, 31),
            IsActive = DateTime.Today >= new DateTime(2025, 2, 1),
            Description = "New home ready! Higher mortgage but no more rent - more savings potential!"
        };

        _context.FinancialPeriods.AddRange(doubleHousingPeriod, newHomePeriod);
        await _context.SaveChangesAsync();

        // Create budget limits for Double Housing Period
        var doubleHousingBudgets = new List<BudgetLimit>
        {
            new() { Category = ExpenseCategory.Mortgage, MonthlyLimit = 1746m, FinancialPeriodId = doubleHousingPeriod.Id, IsEssential = true },
            new() { Category = ExpenseCategory.Rent, MonthlyLimit = 2340m, FinancialPeriodId = doubleHousingPeriod.Id, IsEssential = true },
            new() { Category = ExpenseCategory.Groceries, MonthlyLimit = 400m, FinancialPeriodId = doubleHousingPeriod.Id, IsEssential = true },
            new() { Category = ExpenseCategory.Transport, MonthlyLimit = 350m, FinancialPeriodId = doubleHousingPeriod.Id, IsEssential = true },
            new() { Category = ExpenseCategory.Utilities, MonthlyLimit = 320m, FinancialPeriodId = doubleHousingPeriod.Id, IsEssential = true },
            new() { Category = ExpenseCategory.FoodAndDrinks, MonthlyLimit = 200m, FinancialPeriodId = doubleHousingPeriod.Id, IsEssential = false },
            new() { Category = ExpenseCategory.Entertainment, MonthlyLimit = 150m, FinancialPeriodId = doubleHousingPeriod.Id, IsEssential = false },
            new() { Category = ExpenseCategory.HealthAndFitness, MonthlyLimit = 112m, FinancialPeriodId = doubleHousingPeriod.Id, IsEssential = false },
            new() { Category = ExpenseCategory.Shopping, MonthlyLimit = 100m, FinancialPeriodId = doubleHousingPeriod.Id, IsEssential = false },
            new() { Category = ExpenseCategory.Miscellaneous, MonthlyLimit = 150m, FinancialPeriodId = doubleHousingPeriod.Id, IsEssential = false }
        };

        // Create budget limits for New Home Period
        var newHomeBudgets = new List<BudgetLimit>
        {
            new() { Category = ExpenseCategory.Mortgage, MonthlyLimit = 3750m, FinancialPeriodId = newHomePeriod.Id, IsEssential = true },
            new() { Category = ExpenseCategory.Groceries, MonthlyLimit = 400m, FinancialPeriodId = newHomePeriod.Id, IsEssential = true },
            new() { Category = ExpenseCategory.Transport, MonthlyLimit = 350m, FinancialPeriodId = newHomePeriod.Id, IsEssential = true },
            new() { Category = ExpenseCategory.Utilities, MonthlyLimit = 320m, FinancialPeriodId = newHomePeriod.Id, IsEssential = true },
            new() { Category = ExpenseCategory.FoodAndDrinks, MonthlyLimit = 300m, FinancialPeriodId = newHomePeriod.Id, IsEssential = false },
            new() { Category = ExpenseCategory.Entertainment, MonthlyLimit = 200m, FinancialPeriodId = newHomePeriod.Id, IsEssential = false },
            new() { Category = ExpenseCategory.Shopping, MonthlyLimit = 200m, FinancialPeriodId = newHomePeriod.Id, IsEssential = false },
            new() { Category = ExpenseCategory.Miscellaneous, MonthlyLimit = 200m, FinancialPeriodId = newHomePeriod.Id, IsEssential = false },
            new() { Category = ExpenseCategory.HealthAndFitness, MonthlyLimit = 112m, FinancialPeriodId = newHomePeriod.Id, IsEssential = false }
        };

        _context.BudgetLimits.AddRange(doubleHousingBudgets);
        _context.BudgetLimits.AddRange(newHomeBudgets);

        // Create savings goals for Double Housing Period
        var doubleHousingSavings = new List<SavingsGoal>
        {
            new() { Type = SavingsGoalType.EmergencyFund, Name = "Emergency Fund", MonthlyTarget = 1000m, FinancialPeriodId = doubleHousingPeriod.Id, IsRequired = true },
            new() { Type = SavingsGoalType.Investments, Name = "Investment Portfolio", MonthlyTarget = 800m, FinancialPeriodId = doubleHousingPeriod.Id, IsRequired = false }
        };

        // Create savings goals for New Home Period
        var newHomeSavings = new List<SavingsGoal>
        {
            new() { Type = SavingsGoalType.EmergencyFund, Name = "Emergency Fund", MonthlyTarget = 600m, FinancialPeriodId = newHomePeriod.Id, IsRequired = true },
            new() { Type = SavingsGoalType.Investments, Name = "Investment Portfolio", MonthlyTarget = 800m, FinancialPeriodId = newHomePeriod.Id, IsRequired = true },
            new() { Type = SavingsGoalType.HouseFuture, Name = "House & Future Goals", MonthlyTarget = 520m, FinancialPeriodId = newHomePeriod.Id, IsRequired = false }
        };

        _context.SavingsGoals.AddRange(doubleHousingSavings);
        _context.SavingsGoals.AddRange(newHomeSavings);

        await _context.SaveChangesAsync();
    }

    public async Task<BudgetStatusDto> GetCurrentBudgetStatusAsync()
    {
        var currentPeriod = await GetCurrentFinancialPeriodAsync();
        var currentMonth = DateTime.Today.Year * 100 + DateTime.Today.Month;
        
        var currentMonthExpenses = await _context.Expenses
            .Where(e => e.Date.Year == DateTime.Today.Year && e.Date.Month == DateTime.Today.Month)
            .ToListAsync();

        var currentMonthIncome = await _context.Incomes
            .Where(i => i.Date.Year == DateTime.Today.Year && i.Date.Month == DateTime.Today.Month)
            .SumAsync(i => i.Amount);

        var budgetLimits = await _context.BudgetLimits
            .Where(bl => bl.FinancialPeriodId == currentPeriod.Id)
            .ToListAsync();

        var savingsGoals = await _context.SavingsGoals
            .Where(sg => sg.FinancialPeriodId == currentPeriod.Id)
            .ToListAsync();

        var categoryBudgets = new List<CategoryBudgetDto>();
        decimal totalBudgeted = 0;
        decimal totalSpent = 0;

        foreach (var budget in budgetLimits)
        {
            var spent = currentMonthExpenses
                .Where(e => e.Category == budget.Category)
                .Sum(e => e.Amount);

            var percentageUsed = budget.MonthlyLimit > 0 ? (spent / budget.MonthlyLimit) * 100 : 0;
            var remaining = budget.MonthlyLimit - spent;
            var daysLeftInMonth = DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month) - DateTime.Today.Day + 1;
            var dailyRecommendation = daysLeftInMonth > 0 ? remaining / daysLeftInMonth : 0;

            var status = GetCategoryStatus(percentageUsed);
            var statusColor = GetStatusColor(percentageUsed);

            categoryBudgets.Add(new CategoryBudgetDto
            {
                Category = budget.Category,
                CategoryName = GetCategoryDisplayName(budget.Category),
                Limit = budget.MonthlyLimit,
                Spent = spent,
                Remaining = remaining,
                PercentageUsed = percentageUsed,
                DailyRecommendation = Math.Max(0, dailyRecommendation),
                IsEssential = budget.IsEssential,
                Status = status,
                StatusColor = statusColor
            });

            totalBudgeted += budget.MonthlyLimit;
            totalSpent += spent;
        }

        // Calculate savings
        var savingsProgress = new List<SavingsProgressDto>();
        decimal totalSavingsTarget = savingsGoals.Sum(sg => sg.MonthlyTarget);
        decimal actualSavings = Math.Max(0, currentMonthIncome - totalSpent);
        
        foreach (var goal in savingsGoals)
        {
            var actualForThisGoal = actualSavings * (goal.MonthlyTarget / totalSavingsTarget);
            var progress = goal.MonthlyTarget > 0 ? (actualForThisGoal / goal.MonthlyTarget) * 100 : 0;
            var isAchieved = actualForThisGoal >= goal.MonthlyTarget;

            savingsProgress.Add(new SavingsProgressDto
            {
                Type = goal.Type,
                Name = goal.Name,
                Target = goal.MonthlyTarget,
                Actual = actualForThisGoal,
                Progress = Math.Min(100, progress),
                IsAchieved = isAchieved,
                MotivationalMessage = GetSavingsMotivationalMessage(progress, goal.Name)
            });
        }

        var savingsRate = currentMonthIncome > 0 ? (actualSavings / currentMonthIncome) * 100 : 0;
        var remainingDaysInMonth = DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month) - DateTime.Today.Day + 1;

        return new BudgetStatusDto
        {
            CurrentPeriod = currentPeriod.Name,
            PeriodDescription = currentPeriod.Description,
            MonthlyIncome = currentMonthIncome > 0 ? currentMonthIncome : MONTHLY_INCOME,
            TotalBudgeted = totalBudgeted,
            TotalSpent = totalSpent,
            RemainingBudget = totalBudgeted - totalSpent,
            SavingsTarget = totalSavingsTarget,
            ActualSavings = actualSavings,
            SavingsRate = savingsRate,
            DaysLeftInMonth = remainingDaysInMonth,
            CategoryBudgets = categoryBudgets.OrderByDescending(cb => cb.IsEssential).ThenBy(cb => cb.CategoryName).ToList(),
            SavingsProgress = savingsProgress,
            MotivationalMessage = GetMotivationalMessage(savingsRate, currentPeriod.Type)
        };
    }

    public async Task<SpendingCheckDto> CheckSpendingAsync(ExpenseCategory category, decimal amount)
    {
        var currentPeriod = await GetCurrentFinancialPeriodAsync();
        
        var budgetLimit = await _context.BudgetLimits
            .FirstOrDefaultAsync(bl => bl.Category == category && bl.FinancialPeriodId == currentPeriod.Id);

        if (budgetLimit == null)
        {
            return new SpendingCheckDto
            {
                IsAllowed = true,
                Message = "No budget limit set for this category",
                Encouragement = "Keep tracking your spending - you're doing great!"
            };
        }

        var currentMonthSpending = await _context.Expenses
            .Where(e => e.Category == category && 
                       e.Date.Year == DateTime.Today.Year && 
                       e.Date.Month == DateTime.Today.Month)
            .SumAsync(e => e.Amount);

        var newTotal = currentMonthSpending + amount;
        var newPercentageUsed = (newTotal / budgetLimit.MonthlyLimit) * 100;
        var remainingBudget = budgetLimit.MonthlyLimit - newTotal;

        var alertLevel = GetAlertLevel(newPercentageUsed);
        var message = GetSpendingCheckMessage(category, newPercentageUsed, budgetLimit.IsEssential);
        var encouragement = GetEncouragementMessage(newPercentageUsed);

        return new SpendingCheckDto
        {
            IsAllowed = true, // We always allow but provide guidance
            Message = message,
            AlertLevel = alertLevel,
            RemainingBudget = remainingBudget,
            NewPercentageUsed = newPercentageUsed,
            Encouragement = encouragement
        };
    }

    public async Task<FinancialHealthDto> GetFinancialHealthAsync()
    {
        var currentMonthIncome = await _context.Incomes
            .Where(i => i.Date.Year == DateTime.Today.Year && i.Date.Month == DateTime.Today.Month)
            .SumAsync(i => i.Amount);

        var currentMonthExpenses = await _context.Expenses
            .Where(e => e.Date.Year == DateTime.Today.Year && e.Date.Month == DateTime.Today.Month)
            .SumAsync(e => e.Amount);

        var income = currentMonthIncome > 0 ? currentMonthIncome : MONTHLY_INCOME;
        var savings = income - currentMonthExpenses;
        var savingsRate = income > 0 ? (savings / income) * 100 : 0;

        var grade = GetFinancialHealthGrade(savingsRate);
        var score = CalculateFinancialHealthScore(savingsRate);
        var achievements = GetAchievements(savingsRate);
        var recommendations = GetRecommendations(savingsRate);

        return new FinancialHealthDto
        {
            Grade = grade,
            SavingsRate = savingsRate,
            Score = score,
            Message = GetHealthMessage(grade, savingsRate),
            Achievements = achievements,
            Recommendations = recommendations,
            IsOnTrack = savingsRate >= 20
        };
    }

    public async Task<List<SpendingAlert>> GetActiveAlertsAsync()
    {
        return await _context.SpendingAlerts
            .Where(sa => sa.IsActive && !sa.IsRead)
            .OrderByDescending(sa => sa.CreatedAt)
            .ToListAsync();
    }

    private async Task<FinancialPeriod> GetCurrentFinancialPeriodAsync()
    {
        return await _context.FinancialPeriods
            .FirstAsync(fp => fp.IsActive);
    }

    private string GetCategoryDisplayName(ExpenseCategory category)
    {
        return category switch
        {
            ExpenseCategory.FoodAndDrinks => "Food & Drinks",
            ExpenseCategory.HealthAndFitness => "Health & Fitness",
            _ => category.ToString()
        };
    }

    private string GetCategoryStatus(decimal percentageUsed)
    {
        return percentageUsed switch
        {
            < 50 => "Great",
            < 75 => "Good",
            < 90 => "Caution",
            _ => "Watch"
        };
    }

    private string GetStatusColor(decimal percentageUsed)
    {
        return percentageUsed switch
        {
            < 50 => "green",
            < 75 => "blue",
            < 90 => "yellow",
            _ => "orange"
        };
    }

    private AlertType? GetAlertLevel(decimal percentageUsed)
    {
        return percentageUsed switch
        {
            < 50 => null,
            < 75 => AlertType.Info,
            < 90 => AlertType.Warning,
            < 100 => AlertType.Critical,
            _ => AlertType.Exceeded
        };
    }

    private string GetSpendingCheckMessage(ExpenseCategory category, decimal percentageUsed, bool isEssential)
    {
        var categoryName = GetCategoryDisplayName(category);
        
        return percentageUsed switch
        {
            < 50 => $"You're doing great with {categoryName}! Still plenty of room in your budget.",
            < 75 => $"You're on track with {categoryName} spending. You've used {percentageUsed:F0}% of your budget.",
            < 90 => $"Heads up! You're at {percentageUsed:F0}% of your {categoryName} budget. Still manageable!",
            < 100 => $"You're approaching your {categoryName} limit at {percentageUsed:F0}%. Consider if this expense is necessary.",
            _ => $"This would put you over your {categoryName} budget. You've got this - maybe save this for next month?"
        };
    }

    private string GetEncouragementMessage(decimal percentageUsed)
    {
        return percentageUsed switch
        {
            < 50 => "You're crushing your budget goals! 🎉",
            < 75 => "Keep up the great work! You're staying on track! 👍",
            < 90 => "You're still doing well - just keeping an eye on things! 👀",
            _ => "Every dollar counts towards your financial goals! 💪"
        };
    }

    private string GetSavingsMotivationalMessage(decimal progress, string goalName)
    {
        return progress switch
        {
            >= 100 => $"🎉 Amazing! You've exceeded your {goalName} goal!",
            >= 90 => $"🔥 So close! You're almost at your {goalName} target!",
            >= 75 => $"💪 Great progress on {goalName} - you're doing awesome!",
            >= 50 => $"👍 Good work on {goalName} - keep it up!",
            _ => $"💡 Every dollar towards {goalName} is progress!"
        };
    }

    private string GetMotivationalMessage(decimal savingsRate, FinancialPeriodType periodType)
    {
        if (periodType == FinancialPeriodType.DoubleHousingPeriod)
        {
            return savingsRate switch
            {
                >= 22 => "🎉 Incredible! You're saving over 22% even with double housing costs!",
                >= 20 => "💪 Amazing! 20%+ savings rate during double housing period is fantastic!",
                >= 15 => "👍 Great job! You're building wealth even during this tight period!",
                >= 10 => "🌱 Good progress! Every dollar saved now makes February even better!",
                _ => "💡 February is coming - your financial freedom is just around the corner!"
            };
        }
        else
        {
            return savingsRate switch
            {
                >= 30 => "🚀 Exceptional! You're a savings superstar with 30%+ savings rate!",
                >= 24 => "🎉 Perfect! You've hit your 24% savings target - you're building serious wealth!",
                >= 20 => "💪 Excellent! 20%+ savings rate means you're on track for financial independence!",
                >= 15 => "👍 Good work! You're building a solid financial foundation!",
                _ => "🌱 Every month gets you closer to your financial goals!"
            };
        }
    }

    private string GetFinancialHealthGrade(decimal savingsRate)
    {
        return savingsRate switch
        {
            >= 25 => "Excellent",
            >= 20 => "Good",
            >= 15 => "Fair",
            _ => "Improving"
        };
    }

    private decimal CalculateFinancialHealthScore(decimal savingsRate)
    {
        return Math.Min(100, Math.Max(0, savingsRate * 4)); // Scale savings rate to 0-100
    }

    private List<string> GetAchievements(decimal savingsRate)
    {
        var achievements = new List<string>();
        
        if (savingsRate >= 30) achievements.Add("🚀 Savings Superstar (30%+)");
        if (savingsRate >= 25) achievements.Add("🎯 Excellent Saver (25%+)");
        if (savingsRate >= 20) achievements.Add("💪 Strong Saver (20%+)");
        if (savingsRate >= 15) achievements.Add("👍 Good Financial Health (15%+)");
        if (savingsRate >= 10) achievements.Add("🌱 Building Wealth (10%+)");
        
        return achievements;
    }

    private List<string> GetRecommendations(decimal savingsRate)
    {
        var recommendations = new List<string>();
        
        if (savingsRate < 15)
        {
            recommendations.Add("Focus on increasing your savings rate to 15%+");
            recommendations.Add("Look for opportunities to reduce non-essential spending");
        }
        else if (savingsRate < 20)
        {
            recommendations.Add("Great progress! Aim for 20% to build wealth faster");
            recommendations.Add("Consider automating your savings");
        }
        else if (savingsRate < 25)
        {
            recommendations.Add("Excellent work! You're in the top tier of savers");
            recommendations.Add("Consider increasing investment allocation");
        }
        else
        {
            recommendations.Add("Outstanding! You're achieving financial independence");
            recommendations.Add("Consider diversifying your investment strategy");
        }
        
        return recommendations;
    }

    private string GetHealthMessage(string grade, decimal savingsRate)
    {
        return grade switch
        {
            "Excellent" => $"Outstanding financial health! Your {savingsRate:F1}% savings rate is building serious wealth.",
            "Good" => $"Strong financial position! Your {savingsRate:F1}% savings rate is impressive.",
            "Fair" => $"Good progress! Your {savingsRate:F1}% savings rate shows you're building wealth.",
            _ => $"You're on the right track! Every step towards financial health counts."
        };
    }
}