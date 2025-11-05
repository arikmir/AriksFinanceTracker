using AriksFinanceTracker.Api.Data;
using AriksFinanceTracker.Api.Models.Dto;
using AriksFinanceTracker.Api.Models.Entities;
using AriksFinanceTracker.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace AriksFinanceTracker.Api.Services;

public class ExpenseService
{
    private readonly FinanceContext _context;

    public ExpenseService(FinanceContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Expense>> GetExpensesAsync(int? month = null, int? year = null)
    {
        var query = _context.Expenses.AsQueryable();

        if (month.HasValue && year.HasValue)
        {
            var startDate = new DateTime(year.Value, month.Value, 1);
            var endDate = startDate.AddMonths(1);
            query = query.Where(e => e.Date >= startDate && e.Date < endDate);
        }

        return await query.OrderByDescending(e => e.Date).ToListAsync();
    }

    public async Task<Expense?> GetExpenseByIdAsync(int id)
    {
        return await _context.Expenses.FindAsync(id);
    }

    public async Task<Expense> CreateExpenseAsync(Expense expense)
    {
        expense.CreatedAt = DateTime.UtcNow;
        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();
        return expense;
    }

    public async Task<bool> UpdateExpenseAsync(int id, Expense expense)
    {
        if (id != expense.Id)
        {
            return false;
        }

        expense.UpdatedAt = DateTime.UtcNow;
        _context.Entry(expense).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await ExpenseExistsAsync(id))
            {
                return false;
            }
            throw;
        }
    }

    public async Task<bool> DeleteExpenseAsync(int id)
    {
        var expense = await _context.Expenses.FindAsync(id);
        if (expense == null)
        {
            return false;
        }

        _context.Expenses.Remove(expense);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<DailyExpenseDto>> GetDailyAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.Today.AddDays(-30);
        var end = endDate ?? DateTime.Today;

        var dailyExpenses = await _context.Expenses
            .Where(e => e.Date >= start && e.Date <= end)
            .GroupBy(e => e.Date.Date)
            .Select(g => new DailyExpenseDto
            {
                Date = g.Key,
                TotalAmount = g.Sum(e => e.Amount),
                TransactionCount = g.Count(),
                Expenses = g.ToList()
            })
            .OrderBy(d => d.Date)
            .ToListAsync();

        return dailyExpenses;
    }

    public async Task<ExpenseAnalyticsDto> GetWeeklyAnalyticsAsync(int? month = null, int? year = null)
    {
        var (startDate, endDate) = CalculateWeeklyDateRange(month, year);
        return await GetAnalyticsAsync(startDate, endDate);
    }

    public async Task<ExpenseAnalyticsDto> GetMonthlyAnalyticsAsync(int? month = null, int? year = null)
    {
        var (startDate, endDate) = CalculateMonthlyDateRange(month, year);
        return await GetAnalyticsAsync(startDate, endDate);
    }

    public async Task<IEnumerable<CategorySummaryDto>> GetCategorySummaryAsync(int? month = null, int? year = null)
    {
        var (start, end) = CalculateMonthlyDateRange(month, year);

        // Get all expenses and calculate on client side for SQLite compatibility
        var expenses = await _context.Expenses
            .Where(e => e.Date >= start && e.Date < end.AddSeconds(1))
            .ToListAsync();

        var totalAmount = expenses.Sum(e => e.Amount);

        var categorySummary = expenses
            .GroupBy(e => e.CategoryId)
            .Select(g => new CategorySummaryDto
            {
                CategoryId = g.Key,
                CategoryName = g.First().Category?.Name ?? "Unknown",
                TotalAmount = g.Sum(e => e.Amount),
                TransactionCount = g.Count(),
                Percentage = totalAmount > 0 ? (g.Sum(e => e.Amount) / totalAmount) * 100 : 0
            })
            .OrderByDescending(c => c.TotalAmount)
            .ToList();

        return categorySummary;
    }

    private async Task<ExpenseAnalyticsDto> GetAnalyticsAsync(DateTime startDate, DateTime endDate)
    {
        var expenses = await _context.Expenses
            .Where(e => e.Date >= startDate && e.Date < endDate.AddSeconds(1))
            .ToListAsync();

        if (!expenses.Any())
        {
            return new ExpenseAnalyticsDto
            {
                StartDate = startDate,
                EndDate = endDate
            };
        }

        var analytics = new ExpenseAnalyticsDto
        {
            TotalAmount = expenses.Sum(e => e.Amount),
            TransactionCount = expenses.Count,
            AverageAmount = expenses.Average(e => e.Amount),
            StartDate = startDate,
            EndDate = endDate,
            CategoryBreakdown = expenses
                .GroupBy(e => e.CategoryId)
                .Select(g => new CategoryBreakdownDto
                {
                    CategoryId = g.Key,
                    CategoryName = g.First().Category?.Name ?? "Unknown",
                    TotalAmount = g.Sum(e => e.Amount),
                    TransactionCount = g.Count()
                })
                .OrderByDescending(cb => cb.TotalAmount)
                .ToList()
        };

        return analytics;
    }

    private (DateTime startDate, DateTime endDate) CalculateWeeklyDateRange(int? month, int? year)
    {
        DateTime startDate, endDate;

        if (month.HasValue && year.HasValue)
        {
            // Get the specific week within the provided month
            var firstDayOfMonth = new DateTime(year.Value, month.Value, 1);
            var today = DateTime.Today;

            // If looking at current month, use current date as reference
            if (firstDayOfMonth.Month == today.Month && firstDayOfMonth.Year == today.Year)
            {
                // For current month, get the current week (Sunday to today)
                endDate = today.AddDays(1).AddSeconds(-1); // End of today
                var daysFromSunday = (int)today.DayOfWeek;
                startDate = today.AddDays(-daysFromSunday); // Start from Sunday
            }
            else
            {
                // For past months, get the last full week of that month
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
                endDate = lastDayOfMonth.AddDays(1).AddSeconds(-1); // End of last day
                var daysFromSunday = (int)lastDayOfMonth.DayOfWeek;
                startDate = lastDayOfMonth.AddDays(-daysFromSunday); // Start from Sunday
            }
        }
        else
        {
            // Default behavior: current week (Sunday to today)
            var today = DateTime.Today;
            endDate = today.AddDays(1).AddSeconds(-1); // End of today
            var daysFromSunday = (int)today.DayOfWeek;
            startDate = today.AddDays(-daysFromSunday); // Start from Sunday
        }

        return (startDate, endDate);
    }

    private (DateTime startDate, DateTime endDate) CalculateMonthlyDateRange(int? month, int? year)
    {
        DateTime startDate, endDate;

        if (month.HasValue && year.HasValue)
        {
            // Get the entire month
            startDate = new DateTime(year.Value, month.Value, 1);
            var lastDayOfMonth = startDate.AddMonths(1).AddDays(-1);

            // If it's the current month, only include up to today (inclusive)
            var today = DateTime.Today;
            if (startDate <= today && lastDayOfMonth >= today)
            {
                endDate = today.AddDays(1).AddSeconds(-1); // End of today
            }
            else
            {
                endDate = lastDayOfMonth.AddDays(1).AddSeconds(-1); // End of last day of month
            }
        }
        else
        {
            // Default behavior: current month from start to today
            var today = DateTime.Today;
            startDate = new DateTime(today.Year, today.Month, 1);
            endDate = today.AddDays(1).AddSeconds(-1); // End of today
        }

        return (startDate, endDate);
    }

    private async Task<bool> ExpenseExistsAsync(int id)
    {
        return await _context.Expenses.AnyAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<PaymentMethodSummaryDto>> GetPaymentMethodSummaryAsync(int? month = null, int? year = null)
    {
        var (start, end) = CalculateMonthlyDateRange(month, year);

        var expenses = await _context.Expenses
            .Where(e => e.Date >= start && e.Date < end.AddSeconds(1))
            .ToListAsync();

        var totalAmount = expenses.Sum(e => e.Amount);

        var paymentSummary = expenses
            .GroupBy(e => string.IsNullOrWhiteSpace(e.PaymentMethod) ? "Unspecified" : e.PaymentMethod!.Trim())
            .Select(g =>
            {
                var groupTotal = g.Sum(e => e.Amount);
                return new PaymentMethodSummaryDto
                {
                    PaymentMethod = g.Key,
                    TotalAmount = groupTotal,
                    TransactionCount = g.Count(),
                    Percentage = totalAmount > 0 ? (groupTotal / totalAmount) * 100 : 0
                };
            })
            .OrderByDescending(p => p.TotalAmount)
            .ToList();

        return paymentSummary;
    }

    public async Task<IEnumerable<Expense>> GetExpensesWithCategoryAsync(int? month = null, int? year = null)
    {
        var query = _context.Expenses
            .Include(e => e.Category)
            .AsQueryable();

        if (month.HasValue && year.HasValue)
        {
            var startDate = new DateTime(year.Value, month.Value, 1);
            var endDate = startDate.AddMonths(1);
            query = query.Where(e => e.Date >= startDate && e.Date < endDate);
        }

        return await query.OrderByDescending(e => e.Date).ToListAsync();
    }

    public async Task<Expense?> GetExpenseWithCategoryAsync(int id)
    {
        return await _context.Expenses
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Expense> CreateExpenseWithCategoryAsync(Expense expense)
    {
        expense.CreatedAt = DateTime.UtcNow;
        expense.Category = null!;
        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();
        await _context.Entry(expense).Reference(e => e.Category).LoadAsync();
        return expense;
    }
}
