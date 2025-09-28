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

    private bool ExpenseExists(int id)
    {
        return _context.Expenses.Any(e => e.Id == id);
    }
}