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
    public async Task<ActionResult<IEnumerable<Expense>>> GetExpenses()
    {
        return await _context.Expenses.OrderByDescending(e => e.Date).ToListAsync();
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
    public async Task<ActionResult<ExpenseAnalyticsDto>> GetWeeklyAnalytics()
    {
        var startDate = DateTime.Today.AddDays(-7);
        return await GetAnalytics(startDate, DateTime.Today);
    }

    [HttpGet("analytics/monthly")]
    public async Task<ActionResult<ExpenseAnalyticsDto>> GetMonthlyAnalytics()
    {
        var startDate = DateTime.Today.AddDays(-30);
        return await GetAnalytics(startDate, DateTime.Today);
    }

    [HttpGet("categories/summary")]
    public async Task<ActionResult<IEnumerable<CategorySummaryDto>>> GetCategorySummary([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var start = startDate ?? DateTime.Today.AddDays(-30);
        var end = endDate ?? DateTime.Today;
        
        var totalAmount = await _context.Expenses
            .Where(e => e.Date >= start && e.Date <= end)
            .SumAsync(e => e.Amount);
            
        var categorySummary = await _context.Expenses
            .Where(e => e.Date >= start && e.Date <= end)
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
            .ToListAsync();
            
        return Ok(categorySummary);
    }

    private async Task<ActionResult<ExpenseAnalyticsDto>> GetAnalytics(DateTime startDate, DateTime endDate)
    {
        var expenses = await _context.Expenses
            .Where(e => e.Date >= startDate && e.Date <= endDate)
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