using AriksFinanceTracker.Api.Models.Entities;
using AriksFinanceTracker.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AriksFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IncomeController : ControllerBase
{
    private readonly IncomeService _incomeService;

    public IncomeController(IncomeService incomeService)
    {
        _incomeService = incomeService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Income>>> GetIncomes([FromQuery] int? month, [FromQuery] int? year)
    {
        var incomes = await _incomeService.GetIncomesAsync(month, year);
        return Ok(incomes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Income>> GetIncome(int id)
    {
        var income = await _incomeService.GetIncomeByIdAsync(id);
        if (income == null) return NotFound();
        return income;
    }

    [HttpPost]
    public async Task<ActionResult<Income>> CreateIncome(Income income)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                message = "Invalid income data",
                errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(x => x.Key, x => x.Value.Errors.Select(e => e.ErrorMessage))
            });
        }

        // Validate business rules
        var (isValid, errorMessage) = await _incomeService.ValidateIncomeAsync(income);
        if (!isValid)
        {
            return BadRequest(new { message = errorMessage });
        }

        try
        {
            var createdIncome = await _incomeService.CreateIncomeAsync(income);
            return CreatedAtAction(nameof(GetIncome), new { id = createdIncome.Id }, createdIncome);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while saving income", details = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateIncome(int id, Income income)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                message = "Invalid income data",
                errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(x => x.Key, x => x.Value.Errors.Select(e => e.ErrorMessage))
            });
        }

        // Validate business rules
        var (isValid, errorMessage) = await _incomeService.ValidateIncomeAsync(income);
        if (!isValid)
        {
            return BadRequest(new { message = errorMessage });
        }

        try
        {
            var updated = await _incomeService.UpdateIncomeAsync(id, income);
            if (!updated)
            {
                return NotFound(new { message = $"Income with ID {id} not found" });
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating income", details = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteIncome(int id)
    {
        try
        {
            var deleted = await _incomeService.DeleteIncomeAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = $"Income with ID {id} not found" });
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting income", details = ex.Message });
        }
    }
}