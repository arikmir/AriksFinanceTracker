using Microsoft.EntityFrameworkCore;

namespace AriksFinanceTracker.Api.Services;

public class BudgetService
{
    private readonly FinanceContext _context;
    private const decimal MONTHLY_INCOME = 8000m;

    private static readonly IReadOnlyList<DefaultCategoryConfig> DefaultCategories = new List<DefaultCategoryConfig>
    {
        new("Mortgage", "home", true, 1746m, 3750m),
        new("Rent", "apartment", true, 2340m, 0m),
        new("Groceries", "shopping_cart", true, 400m, 400m),
        new("Transport", "directions_car", true, 350m, 350m),
        new("Utilities", "power", true, 320m, 320m),
        new("Repayment", "payment", true, 500m, 500m),
        new("Food & Drinks", "restaurant", false, 200m, 300m),
        new("Entertainment", "movie", false, 150m, 200m),
        new("Health & Fitness", "fitness_center", false, 112m, 112m),
        new("Home", "home_repair_service", false, 200m, 250m),
        new("Savings", "savings", false, 300m, 400m),
        new("Shopping", "shopping_bag", false, 100m, 200m),
        new("Miscellaneous", "category", false, 150m, 200m)
    };

    public BudgetService(FinanceContext context)
    {
        _context = context;
    }

    public async Task InitializeAriksBudgetAsync()
    {
        await EnsureFinancialPeriodsAsync();
        await EnsureDefaultCategoriesAsync();

        var periods = await _context.FinancialPeriods.ToListAsync();
        foreach (var period in periods)
        {
            await EnsureBudgetsForPeriodAsync(period);
        }
    }

    private async Task EnsureFinancialPeriodsAsync()
    {
        if (await _context.FinancialPeriods.AnyAsync())
        {
            return;
        }

        var doubleHousingPeriod = new FinancialPeriod
        {
            Name = "Double Housing Period",
            Type = FinancialPeriodType.DoubleHousingPeriod,
            StartDate = new DateTime(2024, 1, 1),
            EndDate = new DateTime(2026, 1, 31),
            IsActive = DateTime.Today <= new DateTime(2026, 1, 31),
            Description = "Paying both mortgage and rent - tighter budget but manageable!"
        };

        var newHomePeriod = new FinancialPeriod
        {
            Name = "New Home Period",
            Type = FinancialPeriodType.NewHomePeriod,
            StartDate = new DateTime(2026, 2, 1),
            EndDate = new DateTime(2030, 12, 31),
            IsActive = DateTime.Today >= new DateTime(2026, 2, 1),
            Description = "New home ready! Higher mortgage but no more rent - more savings potential!"
        };

        _context.FinancialPeriods.AddRange(doubleHousingPeriod, newHomePeriod);
        await _context.SaveChangesAsync();
    }

    private async Task EnsureDefaultCategoriesAsync()
    {
        var existingNames = await _context.SpendingCategories
            .Select(c => c.Name)
            .ToListAsync();

        foreach (var config in DefaultCategories)
        {
            if (!existingNames.Contains(config.Name))
            {
                _context.SpendingCategories.Add(new SpendingCategory
                {
                    Name = config.Name,
                    Icon = config.Icon,
                    IsSystem = true,
                    IsEssentialDefault = config.IsEssential
                });
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task EnsureBudgetsForPeriodAsync(FinancialPeriod period)
    {
        var existingCategoryIds = await _context.BudgetLimits
            .Where(bl => bl.FinancialPeriodId == period.Id)
            .Select(bl => bl.CategoryId)
            .ToListAsync();

        var categories = await _context.SpendingCategories.ToDictionaryAsync(c => c.Name);

        foreach (var config in DefaultCategories)
        {
            if (!categories.TryGetValue(config.Name, out var category))
            {
                continue;
            }

            var limit = period.Type == FinancialPeriodType.DoubleHousingPeriod
                ? config.DoubleHousingLimit
                : config.NewHomeLimit;

            if (limit <= 0)
            {
                continue;
            }

            if (existingCategoryIds.Contains(category.Id))
            {
                continue;
            }

            _context.BudgetLimits.Add(new BudgetLimit
            {
                CategoryId = category.Id,
                FinancialPeriodId = period.Id,
                MonthlyLimit = limit,
                IsEssential = config.IsEssential
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task<BudgetStatusDto> GetCurrentBudgetStatusAsync()
    {
        var currentPeriod = await GetCurrentFinancialPeriodAsync();

        var currentMonthExpenses = await _context.Expenses
            .Where(e => e.Date.Year == DateTime.Today.Year && e.Date.Month == DateTime.Today.Month)
            .ToListAsync();

        var currentMonthIncome = await _context.Incomes
            .Where(i => i.Date.Year == DateTime.Today.Year && i.Date.Month == DateTime.Today.Month)
            .SumAsync(i => i.Amount);

        var daysLeftInMonth = DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month) - DateTime.Today.Day + 1;
        if (daysLeftInMonth < 0)
        {
            daysLeftInMonth = 0;
        }

        var budgetLimits = await _context.BudgetLimits
            .Include(bl => bl.Category)
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
                .Where(e => e.CategoryId == budget.CategoryId)
                .Sum(e => e.Amount);

            var percentageUsed = budget.MonthlyLimit > 0
                ? (spent / budget.MonthlyLimit) * 100
                : 0;

            var remaining = budget.MonthlyLimit - spent;
            var dailyRecommendation = daysLeftInMonth > 0 && remaining > 0
                ? remaining / daysLeftInMonth
                : 0;

            categoryBudgets.Add(new CategoryBudgetDto
            {
                CategoryId = budget.CategoryId,
                CategoryName = budget.Category.Name,
                Limit = budget.MonthlyLimit,
                Spent = spent,
                Remaining = remaining,
                PercentageUsed = percentageUsed,
                IsEssential = budget.IsEssential,
                Status = GetCategoryStatus(percentageUsed),
                StatusColor = GetStatusColor(percentageUsed),
                IsCustom = !budget.Category.IsSystem,
                Icon = budget.Category.Icon,
                DailyRecommendation = dailyRecommendation
            });

            totalBudgeted += budget.MonthlyLimit;
            totalSpent += spent;
        }

        var savingsProgress = new List<SavingsProgressDto>();
        decimal totalSavingsTarget = savingsGoals.Sum(sg => sg.MonthlyTarget);
        decimal actualSavings = Math.Max(0, (currentMonthIncome > 0 ? currentMonthIncome : MONTHLY_INCOME) - totalSpent);

        foreach (var goal in savingsGoals)
        {
            var allocationRatio = totalSavingsTarget > 0 ? goal.MonthlyTarget / totalSavingsTarget : 0;
            var actualForThisGoal = actualSavings * allocationRatio;
            var progress = goal.MonthlyTarget > 0 ? (actualForThisGoal / goal.MonthlyTarget) * 100 : 0;

            savingsProgress.Add(new SavingsProgressDto
            {
                Type = goal.Type,
                Name = goal.Name,
                Target = goal.MonthlyTarget,
                Actual = actualForThisGoal,
                Progress = Math.Min(100, progress),
                IsAchieved = actualForThisGoal >= goal.MonthlyTarget,
                MotivationalMessage = GetSavingsMotivationalMessage(progress, goal.Name)
            });
        }

        var incomeBaseline = currentMonthIncome > 0 ? currentMonthIncome : MONTHLY_INCOME;
        var savingsRate = incomeBaseline > 0 ? (actualSavings / incomeBaseline) * 100 : 0;
        return new BudgetStatusDto
        {
            CurrentPeriod = currentPeriod.Name,
            PeriodDescription = currentPeriod.Description,
            MonthlyIncome = incomeBaseline,
            TotalBudgeted = totalBudgeted,
            TotalSpent = totalSpent,
            RemainingBudget = totalBudgeted - totalSpent,
            SavingsTarget = totalSavingsTarget,
            ActualSavings = actualSavings,
            SavingsRate = savingsRate,
            DaysLeftInMonth = daysLeftInMonth,
            CategoryBudgets = categoryBudgets
                .OrderByDescending(cb => cb.IsEssential)
                .ThenBy(cb => cb.CategoryName)
                .ToList(),
            SavingsProgress = savingsProgress,
            MotivationalMessage = GetMotivationalMessage(savingsRate, currentPeriod.Type)
        };
    }

    public async Task<SpendingCheckDto> CheckSpendingAsync(int categoryId, decimal amount)
    {
        var currentPeriod = await GetCurrentFinancialPeriodAsync();

        var budgetLimit = await _context.BudgetLimits
            .Include(bl => bl.Category)
            .FirstOrDefaultAsync(bl => bl.CategoryId == categoryId && bl.FinancialPeriodId == currentPeriod.Id);

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
            .Where(e => e.CategoryId == categoryId &&
                        e.Date.Year == DateTime.Today.Year &&
                        e.Date.Month == DateTime.Today.Month)
            .SumAsync(e => e.Amount);

        var newTotal = currentMonthSpending + amount;
        var newPercentageUsed = budgetLimit.MonthlyLimit > 0
            ? (newTotal / budgetLimit.MonthlyLimit) * 100
            : 0;
        var remainingBudget = budgetLimit.MonthlyLimit - newTotal;

        var alertLevel = GetAlertLevel(newPercentageUsed);
        var message = GetSpendingCheckMessage(budgetLimit.Category.Name, newPercentageUsed, budgetLimit.IsEssential);
        var encouragement = GetEncouragementMessage(newPercentageUsed);

        return new SpendingCheckDto
        {
            IsAllowed = true,
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

    public async Task<List<BudgetLimitDto>> GetBudgetLimitsAsync()
    {
        var currentPeriod = await GetCurrentFinancialPeriodAsync();

        var limits = await _context.BudgetLimits
            .Include(bl => bl.Category)
            .Where(bl => bl.FinancialPeriodId == currentPeriod.Id)
            .OrderByDescending(bl => bl.IsEssential)
            .ThenBy(bl => bl.Category.Name)
            .ToListAsync();

        return limits.Select(bl => new BudgetLimitDto
        {
            CategoryId = bl.CategoryId,
            CategoryName = bl.Category.Name,
            MonthlyLimit = bl.MonthlyLimit,
            IsEssential = bl.IsEssential,
            IsCustom = !bl.Category.IsSystem,
            Icon = bl.Category.Icon
        }).ToList();
    }

    public async Task<List<SpendingCategoryDto>> GetSpendingCategoriesAsync()
    {
        var categories = await _context.SpendingCategories
            .OrderBy(c => c.IsSystem ? 0 : 1)
            .ThenBy(c => c.Name)
            .ToListAsync();

        return categories.Select(c => new SpendingCategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Icon = c.Icon,
            IsCustom = !c.IsSystem,
            IsEssentialDefault = c.IsEssentialDefault
        }).ToList();
    }

    public async Task<CategoryBudgetDto> CreateCustomCategoryAsync(string name, decimal monthlyLimit, bool isEssential)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Category name is required", nameof(name));
        }

        var sanitizedName = name.Trim();

        if (await _context.SpendingCategories.AnyAsync(c => c.Name == sanitizedName))
        {
            throw new InvalidOperationException($"A category named '{sanitizedName}' already exists.");
        }

        var category = new SpendingCategory
        {
            Name = sanitizedName,
            Icon = "category",
            IsSystem = false,
            IsEssentialDefault = isEssential
        };

        _context.SpendingCategories.Add(category);
        await _context.SaveChangesAsync();

        var currentPeriod = await GetCurrentFinancialPeriodAsync();

        var budgetLimit = new BudgetLimit
        {
            CategoryId = category.Id,
            FinancialPeriodId = currentPeriod.Id,
            MonthlyLimit = monthlyLimit,
            IsEssential = isEssential
        };

        _context.BudgetLimits.Add(budgetLimit);
        await _context.SaveChangesAsync();

        return new CategoryBudgetDto
        {
            CategoryId = category.Id,
            CategoryName = category.Name,
            Limit = budgetLimit.MonthlyLimit,
            Spent = 0,
            Remaining = budgetLimit.MonthlyLimit,
            PercentageUsed = 0,
            IsEssential = isEssential,
            Status = GetCategoryStatus(0),
            StatusColor = GetStatusColor(0),
            IsCustom = true,
            Icon = category.Icon,
            DailyRecommendation = 0
        };
    }

    public async Task UpdateCategoryLimitAsync(int categoryId, decimal newLimit, bool? isEssential = null, string? name = null)
    {
        var currentPeriod = await GetCurrentFinancialPeriodAsync();

        var budgetLimit = await _context.BudgetLimits
            .Include(bl => bl.Category)
            .FirstOrDefaultAsync(bl => bl.CategoryId == categoryId && bl.FinancialPeriodId == currentPeriod.Id);

        if (budgetLimit == null)
        {
            throw new InvalidOperationException($"No budget limit found for category id {categoryId} in the current period.");
        }

        budgetLimit.MonthlyLimit = newLimit;

        if (isEssential.HasValue)
        {
            budgetLimit.IsEssential = isEssential.Value;
        }

        if (!string.IsNullOrWhiteSpace(name) && !budgetLimit.Category.IsSystem)
        {
            budgetLimit.Category.Name = name.Trim();
            budgetLimit.Category.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    private static string GetCategoryStatus(decimal percentageUsed)
    {
        return percentageUsed switch
        {
            < 50 => "Great",
            < 75 => "Good",
            < 90 => "Caution",
            _ => "Watch"
        };
    }

    private static string GetStatusColor(decimal percentageUsed)
    {
        return percentageUsed switch
        {
            < 50 => "green",
            < 75 => "blue",
            < 90 => "yellow",
            _ => "orange"
        };
    }

    private static AlertType? GetAlertLevel(decimal percentageUsed)
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

    private static string GetSpendingCheckMessage(string categoryName, decimal percentageUsed, bool isEssential)
    {
        return percentageUsed switch
        {
            < 50 => $"You're doing great with {categoryName}! Still plenty of room in your budget.",
            < 75 => $"You're on track with {categoryName} spending. You've used {percentageUsed:F0}% of your budget.",
            < 90 => $"Heads up! You're at {percentageUsed:F0}% of your {categoryName} budget. Still manageable!",
            < 100 => $"You're approaching your {categoryName} limit at {percentageUsed:F0}%. Consider if this expense is necessary.",
            _ => isEssential
                ? $"This would push essential spending for {categoryName} above the plan. Is there a way to soften this expense?"
                : $"This would put you over your {categoryName} budget. You've got this - maybe save this for next month?"
        };
    }

    private static string GetEncouragementMessage(decimal percentageUsed)
    {
        return percentageUsed switch
        {
            < 50 => "You're crushing your budget goals! 🎉",
            < 75 => "Keep up the great work! You're staying on track! 👍",
            < 90 => "You're still doing well - just keeping an eye on things! 👀",
            _ => "Every dollar counts towards your financial goals! 💪"
        };
    }

    private static string GetSavingsMotivationalMessage(decimal progress, string goalName)
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

        return savingsRate switch
        {
            >= 30 => "🚀 Exceptional! You're a savings superstar with 30%+ savings rate!",
            >= 24 => "🎉 Perfect! You've hit your 24% savings target - you're building serious wealth!",
            >= 20 => "💪 Excellent! 20%+ savings rate means you're on track for financial independence!",
            >= 15 => "👍 Good work! You're building a solid financial foundation!",
            _ => "🌱 Every month gets you closer to your financial goals!"
        };
    }

    private static string GetFinancialHealthGrade(decimal savingsRate)
    {
        return savingsRate switch
        {
            >= 25 => "Excellent",
            >= 20 => "Good",
            >= 15 => "Fair",
            _ => "Improving"
        };
    }

    private static decimal CalculateFinancialHealthScore(decimal savingsRate)
    {
        return Math.Min(100, Math.Max(0, savingsRate * 4));
    }

    private static List<string> GetAchievements(decimal savingsRate)
    {
        var achievements = new List<string>();

        if (savingsRate >= 30) achievements.Add("🚀 Savings Superstar (30%+)");
        if (savingsRate >= 25) achievements.Add("🎯 Excellent Saver (25%+)");
        if (savingsRate >= 20) achievements.Add("💪 Strong Saver (20%+)");
        if (savingsRate >= 15) achievements.Add("👍 Good Financial Health (15%+)");
        if (savingsRate >= 10) achievements.Add("🌱 Building Wealth (10%+)");

        return achievements;
    }

    private static List<string> GetRecommendations(decimal savingsRate)
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

    private static string GetHealthMessage(string grade, decimal savingsRate)
    {
        return grade switch
        {
            "Excellent" => $"Outstanding financial health! Your {savingsRate:F1}% savings rate is building serious wealth.",
            "Good" => $"Strong financial position! Your {savingsRate:F1}% savings rate is impressive.",
            "Fair" => $"Good progress! Your {savingsRate:F1}% savings rate shows you're building wealth.",
            _ => "You're on the right track! Every step towards financial health counts."
        };
    }

    private async Task<FinancialPeriod> GetCurrentFinancialPeriodAsync()
    {
        var active = await _context.FinancialPeriods.FirstOrDefaultAsync(fp => fp.IsActive);
        if (active != null)
        {
            return active;
        }

        return await _context.FinancialPeriods
            .OrderByDescending(fp => fp.StartDate)
            .FirstAsync();
    }

    private record DefaultCategoryConfig(string Name, string Icon, bool IsEssential, decimal DoubleHousingLimit, decimal NewHomeLimit);
}
