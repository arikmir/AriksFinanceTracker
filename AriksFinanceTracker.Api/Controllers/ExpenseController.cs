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
        var expenses = await _expenseService.GetExpensesAsync(month, year);
        return Ok(expenses);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Expense>> GetExpense(int id)
    {
        var expense = await _expenseService.GetExpenseByIdAsync(id);
        if (expense == null) return NotFound();
        return expense;
    }

    [HttpPost]
    public async Task<ActionResult<Expense>> CreateExpense(Expense expense)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var createdExpense = await _expenseService.CreateExpenseAsync(expense);
        return CreatedAtAction(nameof(GetExpense), new { id = createdExpense.Id }, createdExpense);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateExpense(int id, Expense expense)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

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
        var dailyExpenses = await _expenseService.GetDailyAnalyticsAsync(startDate, endDate);
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
        var categorySummary = await _expenseService.GetCategorySummaryAsync(month, year);
        return Ok(categorySummary);
    }
}
