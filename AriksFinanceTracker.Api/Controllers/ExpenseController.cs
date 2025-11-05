using AriksFinanceTracker.Api.Models.Dto;
using AriksFinanceTracker.Api.Models.Entities;
using AriksFinanceTracker.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        var expenses = await _expenseService.GetExpensesWithCategoryAsync(month, year);
        return Ok(expenses);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Expense>> GetExpense(int id)
    {
        var expense = await _expenseService.GetExpenseWithCategoryAsync(id);
        if (expense == null) return NotFound();
        return expense;
    }

    [HttpPost]
    public async Task<ActionResult<Expense>> CreateExpense(Expense expense)
    {
        ModelState.Remove("Category");
        
        if (!ModelState.IsValid) return BadRequest(ModelState);
        
        var createdExpense = await _expenseService.CreateExpenseWithCategoryAsync(expense);
        return CreatedAtAction(nameof(GetExpense), new { id = createdExpense.Id }, createdExpense);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateExpense(int id, Expense expense)
    {
        if (id != expense.Id) return BadRequest();
        
        ModelState.Remove("Category");
        
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
        var analytics = await _expenseService.GetDailyAnalyticsAsync(startDate, endDate);
        return Ok(analytics);
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
        var summary = await _expenseService.GetCategorySummaryAsync(month, year);
        return Ok(summary);
    }

    [HttpGet("payment-methods/summary")]
    public async Task<ActionResult<IEnumerable<PaymentMethodSummaryDto>>> GetPaymentMethodSummary([FromQuery] int? month, [FromQuery] int? year)
    {
        var summary = await _expenseService.GetPaymentMethodSummaryAsync(month, year);
        return Ok(summary);
    }

    private async Task<bool> ExpenseExists(int id)
    {
        var expense = await _expenseService.GetExpenseByIdAsync(id);
        return expense != null;
    }
}
