using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class ExpenseController : ControllerBase
{
    private readonly FinanceContext _context;

    public ExpenseController(FinanceContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Expense>>> GetExpenses([FromQuery] int? month, [FromQuery] int? year)
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

    [HttpGet("{id}")]
    public async Task<ActionResult<Expense>> GetExpense(int id)
    {
        var expense = await _context.Expenses.FindAsync(id);
        if (expense == null) return NotFound();
        return expense;
    }

    [HttpPost]
    public async Task<ActionResult<Expense>> CreateExpense(Expense expense)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        
        expense.CreatedAt = DateTime.UtcNow;
        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetExpense), new { id = expense.Id }, expense);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateExpense(int id, Expense expense)
    {
        if (id != expense.Id) return BadRequest();
        if (!ModelState.IsValid) return BadRequest(ModelState);

        _context.Entry(expense).State = EntityState.Modified;
        
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ExpenseExists(id)) return NotFound();
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExpense(int id)
    {
        var expense = await _context.Expenses.FindAsync(id);
        if (expense == null) return NotFound();
        
        _context.Expenses.Remove(expense);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("analytics/daily")]
    public async Task<ActionResult<IEnumerable<DailyExpenseDto>>> GetDailyAnalytics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
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
            
        return Ok(dailyExpenses);
    }

    [HttpGet("analytics/weekly")]
    public async Task<ActionResult<ExpenseAnalyticsDto>> GetWeeklyAnalytics([FromQuery] int? month, [FromQuery] int? year)
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
        
        return await GetAnalytics(startDate, endDate);
    }

    [HttpGet("analytics/monthly")]
    public async Task<ActionResult<ExpenseAnalyticsDto>> GetMonthlyAnalytics([FromQuery] int? month, [FromQuery] int? year)
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
        
        return await GetAnalytics(startDate, endDate);
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
            .Where(e => e.Date >= start && e.Date < end.AddSeconds(1))
            .ToListAsync();
            
        var totalAmount = expenses.Sum(e => e.Amount);
            
        var categorySummary = expenses
            .GroupBy(e => e.Category)
            .Select(g => new CategorySummaryDto
            {
                Category = g.Key,
                CategoryName = g.Key.ToString(),
                TotalAmount = g.Sum(e => e.Amount),
                TransactionCount = g.Count(),
                Percentage = totalAmount > 0 ? (g.Sum(e => e.Amount) / totalAmount) * 100 : 0
            })
            .OrderByDescending(c => c.TotalAmount)
            .ToList();
            
        return Ok(categorySummary);
    }

    private async Task<ActionResult<ExpenseAnalyticsDto>> GetAnalytics(DateTime startDate, DateTime endDate)
    {
        var expenses = await _context.Expenses
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
                .GroupBy(e => e.Category)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount))
        };
        
        return Ok(analytics);
    }

    private bool ExpenseExists(int id)
    {
        return _context.Expenses.Any(e => e.Id == id);
    }
}