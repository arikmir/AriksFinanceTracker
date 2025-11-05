using AriksFinanceTracker.Api.Models.Dto;
using AriksFinanceTracker.Api.Models.Entities;
using AriksFinanceTracker.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AriksFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpenseController : ControllerBase
{
    private readonly ExpenseService _expenseService;

    public ExpenseController(ExpenseService expenseService)
    {
        _expenseService = expenseService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Expense>>> GetExpenses([FromQuery] int? month, [FromQuery] int? year)
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

    [HttpGet("{id}")]
    public async Task<ActionResult<Expense>> GetExpense(int id)
    {
        var expense = await _context.Expenses
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (expense == null) return NotFound();
        return expense;
    }

    [HttpPost]
    public async Task<ActionResult<Expense>> CreateExpense(Expense expense)
    {
        // Remove Category validation since it's a navigation property
        ModelState.Remove("Category");
        
        if (!ModelState.IsValid) return BadRequest(ModelState);
        
        expense.CreatedAt = DateTime.UtcNow;
        expense.Category = null!;
        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();
        await _context.Entry(expense).Reference(e => e.Category).LoadAsync();
        return CreatedAtAction(nameof(GetExpense), new { id = expense.Id }, expense);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateExpense(int id, Expense expense)
    {
        if (id != expense.Id) return BadRequest();
        
        // Remove Category validation since it's a navigation property
        ModelState.Remove("Category");
        
        if (!ModelState.IsValid) return BadRequest(ModelState);

        expense.Category = null!;
        _context.Entry(expense).State = EntityState.Modified;
        _context.Entry(expense).Reference(e => e.Category).IsModified = false;
        
        try
        {
            var updated = await _expenseService.UpdateExpenseAsync(id, expense);
            if (!updated) return BadRequest("Expense ID mismatch or not found");
            return NoContent();
        }
        catch (Exception)
        {
            throw;
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExpense(int id)
    {
        var deleted = await _expenseService.DeleteExpenseAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }

    [HttpGet("analytics/daily")]
    public async Task<ActionResult<IEnumerable<DailyExpenseDto>>> GetDailyAnalytics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var start = startDate ?? DateTime.Today.AddDays(-30);
        var end = endDate ?? DateTime.Today;
        
        var expensesInRange = await _context.Expenses
            .Include(e => e.Category)
            .Where(e => e.Date >= start && e.Date <= end)
            .ToListAsync();

        var dailyExpenses = expensesInRange
            .GroupBy(e => e.Date.Date)
            .Select(g => new DailyExpenseDto
            {
                Date = g.Key,
                TotalAmount = g.Sum(e => e.Amount),
                TransactionCount = g.Count(),
                Expenses = g
                    .OrderByDescending(exp => exp.Date)
                    .ToList()
            })
            .OrderBy(d => d.Date)
            .ToList();
        
        return Ok(dailyExpenses);
    }

    [HttpGet("analytics/weekly")]
    public async Task<ActionResult<ExpenseAnalyticsDto>> GetWeeklyAnalytics([FromQuery] int? month, [FromQuery] int? year)
    {
        var analytics = await _expenseService.GetWeeklyAnalyticsAsync(month, year);
        return Ok(analytics);
    }

    [HttpGet("analytics/monthly")]
    public async Task<ActionResult<ExpenseAnalyticsDto>> GetMonthlyAnalytics([FromQuery] int? month, [FromQuery] int? year)
    {
        var analytics = await _expenseService.GetMonthlyAnalyticsAsync(month, year);
        return Ok(analytics);
    }

    [HttpGet("categories/summary")]
    public async Task<ActionResult<IEnumerable<CategorySummaryDto>>> GetCategorySummary([FromQuery] int? month, [FromQuery] int? year)
    {
        DateTime start, end;
        
        if (month.HasValue && year.HasValue)
        {
            // Get the entire month
            start = new DateTime(year.Value, month.Value, 1);
            var lastDayOfMonth = start.AddMonths(1).AddDays(-1);
            
            // If it's the current month, only include up to today (inclusive)
            var today = DateTime.Today;
            if (start <= today && lastDayOfMonth >= today)
            {
                end = today.AddDays(1).AddSeconds(-1); // End of today
            }
            else
            {
                end = lastDayOfMonth.AddDays(1).AddSeconds(-1); // End of last day of month
            }
        }
        else
        {
            // Default behavior: current month from start to today
            var today = DateTime.Today;
            start = new DateTime(today.Year, today.Month, 1);
            end = today.AddDays(1).AddSeconds(-1); // End of today
        }
        
        // Get all expenses and calculate on client side for SQLite compatibility
        var expenses = await _context.Expenses
            .Include(e => e.Category)
            .Where(e => e.Date >= start && e.Date < end.AddSeconds(1))
            .ToListAsync();
            
        var totalAmount = expenses.Sum(e => e.Amount);
            
        var categorySummary = expenses
            .GroupBy(e => e.CategoryId)
            .Select(g => new CategorySummaryDto
            {
                CategoryId = g.Key,
                CategoryName = g.First().Category.Name,
                TotalAmount = g.Sum(e => e.Amount),
                TransactionCount = g.Count(),
                Percentage = totalAmount > 0 ? (g.Sum(e => e.Amount) / totalAmount) * 100 : 0
            })
            .OrderByDescending(c => c.TotalAmount)
            .ToList();
            
        return Ok(categorySummary);
    }

    [HttpGet("payment-methods/summary")]
    public async Task<ActionResult<IEnumerable<PaymentMethodSummaryDto>>> GetPaymentMethodSummary([FromQuery] int? month, [FromQuery] int? year)
    {
        DateTime start, end;

        if (month.HasValue && year.HasValue)
        {
            start = new DateTime(year.Value, month.Value, 1);
            var lastDayOfMonth = start.AddMonths(1).AddDays(-1);

            var today = DateTime.Today;
            if (start <= today && lastDayOfMonth >= today)
            {
                end = today.AddDays(1).AddSeconds(-1);
            }
            else
            {
                end = lastDayOfMonth.AddDays(1).AddSeconds(-1);
            }
        }
        else
        {
            var today = DateTime.Today;
            start = new DateTime(today.Year, today.Month, 1);
            end = today.AddDays(1).AddSeconds(-1);
        }

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

        return Ok(paymentSummary);
    }

    private async Task<ActionResult<ExpenseAnalyticsDto>> GetAnalytics(DateTime startDate, DateTime endDate)
    {
        var expenses = await _context.Expenses
            .Include(e => e.Category)
            .Where(e => e.Date >= startDate && e.Date < endDate.AddSeconds(1))
            .ToListAsync();
            
        if (!expenses.Any())
        {
            return Ok(new ExpenseAnalyticsDto
            {
                StartDate = startDate,
                EndDate = endDate
            });
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
                    CategoryName = g.First().Category.Name,
                    TotalAmount = g.Sum(e => e.Amount),
                    TransactionCount = g.Count()
                })
                .OrderByDescending(cb => cb.TotalAmount)
                .ToList()
        };
        
        return Ok(analytics);
    }

    private bool ExpenseExists(int id)
    {
        return _context.Expenses.Any(e => e.Id == id);
    }
}
