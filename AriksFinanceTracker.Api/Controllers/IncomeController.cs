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
        if (!ModelState.IsValid) return BadRequest(ModelState);
        
        _context.Incomes.Add(income);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetIncome), new { id = income.Id }, income);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateIncome(int id, Income income)
    {
        if (id != income.Id) return BadRequest();
        if (!ModelState.IsValid) return BadRequest(ModelState);

        _context.Entry(income).State = EntityState.Modified;
        
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!IncomeExists(id)) return NotFound();
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteIncome(int id)
    {
        var income = await _context.Incomes.FindAsync(id);
        if (income == null) return NotFound();
        
        _context.Incomes.Remove(income);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private bool IncomeExists(int id)
    {
        return _context.Incomes.Any(i => i.Id == id);
    }
}