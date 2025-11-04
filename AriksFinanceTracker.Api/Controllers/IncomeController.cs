using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class IncomeController : ControllerBase
{
    private readonly FinanceContext _context;

    public IncomeController(FinanceContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Income>>> GetIncomes()
    {
        return await _context.Incomes.OrderByDescending(i => i.Date).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Income>> GetIncome(int id)
    {
        var income = await _context.Incomes.FindAsync(id);
        if (income == null) return NotFound();
        return income;
    }

    [HttpPost]
    public async Task<ActionResult<Income>> CreateIncome(Income income)
    {
        if (!ModelState.IsValid) 
        {
            return BadRequest(new { 
                message = "Invalid income data", 
                errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(x => x.Key, x => x.Value.Errors.Select(e => e.ErrorMessage))
            });
        }
        
        // Validate business rules
        if (income.Amount <= 0)
        {
            return BadRequest(new { message = "Income amount must be greater than zero" });
        }
        
        if (string.IsNullOrWhiteSpace(income.Source))
        {
            return BadRequest(new { message = "Income source is required" });
        }
        
        try
        {
            _context.Incomes.Add(income);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetIncome), new { id = income.Id }, income);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while saving income", details = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateIncome(int id, Income income)
    {
        if (id != income.Id) 
        {
            return BadRequest(new { message = "Income ID mismatch" });
        }
        
        if (!ModelState.IsValid) 
        {
            return BadRequest(new { 
                message = "Invalid income data", 
                errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(x => x.Key, x => x.Value.Errors.Select(e => e.ErrorMessage))
            });
        }
        
        // Validate business rules
        if (income.Amount <= 0)
        {
            return BadRequest(new { message = "Income amount must be greater than zero" });
        }
        
        if (string.IsNullOrWhiteSpace(income.Source))
        {
            return BadRequest(new { message = "Income source is required" });
        }

        _context.Entry(income).State = EntityState.Modified;
        
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!IncomeExists(id)) 
            {
                return NotFound(new { message = $"Income with ID {id} not found" });
            }
            return Conflict(new { message = "Income was modified by another user. Please refresh and try again." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating income", details = ex.Message });
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteIncome(int id)
    {
        var income = await _context.Incomes.FindAsync(id);
        if (income == null) 
        {
            return NotFound(new { message = $"Income with ID {id} not found" });
        }
        
        try
        {
            _context.Incomes.Remove(income);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting income", details = ex.Message });
        }
    }

    private bool IncomeExists(int id)
    {
        return _context.Incomes.Any(i => i.Id == id);
    }
}