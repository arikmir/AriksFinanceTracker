using AriksFinanceTracker.Api.Data;
using AriksFinanceTracker.Api.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AriksFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TotalSavingsController : ControllerBase
{
    private readonly FinanceContext _context;

    public TotalSavingsController(FinanceContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TotalSavings>>> GetTotalSavings()
    {
        return await _context.TotalSavings.OrderByDescending(s => s.Date).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TotalSavings>> GetTotalSavings(int id)
    {
        var totalSavings = await _context.TotalSavings.FindAsync(id);

        if (totalSavings == null)
        {
            return NotFound();
        }

        return totalSavings;
    }

    [HttpGet("monthly/{year}/{month}")]
    public async Task<ActionResult<object>> GetMonthlySavings(int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var savings = await _context.TotalSavings
            .Where(s => s.Date >= startDate && s.Date <= endDate)
            .OrderByDescending(s => s.Date)
            .ToListAsync();

        var totalAmount = savings.Sum(s => s.Amount);

        return Ok(new { 
            TotalAmount = totalAmount,
            Savings = savings,
            Count = savings.Count
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutTotalSavings(int id, TotalSavings totalSavings)
    {
        if (id != totalSavings.Id)
        {
            return BadRequest();
        }

        totalSavings.UpdatedAt = DateTime.UtcNow;
        _context.Entry(totalSavings).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!TotalSavingsExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult<TotalSavings>> PostTotalSavings(TotalSavings totalSavings)
    {
        totalSavings.CreatedAt = DateTime.UtcNow;
        _context.TotalSavings.Add(totalSavings);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetTotalSavings", new { id = totalSavings.Id }, totalSavings);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTotalSavings(int id)
    {
        var totalSavings = await _context.TotalSavings.FindAsync(id);
        if (totalSavings == null)
        {
            return NotFound();
        }

        _context.TotalSavings.Remove(totalSavings);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool TotalSavingsExists(int id)
    {
        return _context.TotalSavings.Any(e => e.Id == id);
    }
}