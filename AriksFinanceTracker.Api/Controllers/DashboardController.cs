using AriksFinanceTracker.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AriksFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly FinanceContext _context;

    public DashboardController(FinanceContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<object>> GetDashboard(int? month = null, int? year = null)
    {
        var currentMonth = month ?? DateTime.Now.Month;
        var currentYear = year ?? DateTime.Now.Year;
        
        var totalIncome = await _context.Incomes
            .Where(i => i.Date.Month == currentMonth && i.Date.Year == currentYear)
            .SumAsync(i => i.Amount);
            
        var totalExpenses = await _context.Expenses
            .Where(e => e.Date.Month == currentMonth && e.Date.Year == currentYear)
            .SumAsync(e => e.Amount);
        
        var expensesByCategory = await _context.Expenses
            .Where(e => e.Date.Month == currentMonth && e.Date.Year == currentYear)
            .GroupBy(e => e.Category)
            .Select(g => new { 
                Category = g.Key.ToString(), 
                Amount = g.Sum(e => e.Amount) 
            })
            .ToListAsync();
        
        return Ok(new 
        {
            totalIncome,
            totalExpenses,
            netSavings = totalIncome - totalExpenses,
            savingsRate = totalIncome > 0 ? ((totalIncome - totalExpenses) / totalIncome) * 100 : 0,
            expensesByCategory
        });
    }

    [HttpGet("yearly")]
    public async Task<ActionResult<object>> GetYearlyDashboard(int year = 0)
    {
        if (year == 0) year = DateTime.Now.Year;
        
        var totalIncome = await _context.Incomes
            .Where(i => i.Date.Year == year)
            .SumAsync(i => i.Amount);
            
        var totalExpenses = await _context.Expenses
            .Where(e => e.Date.Year == year)
            .SumAsync(e => e.Amount);
        
        var monthlyData = await _context.Expenses
            .Where(e => e.Date.Year == year)
            .GroupBy(e => e.Date.Month)
            .Select(g => new { 
                Month = g.Key, 
                Expenses = g.Sum(e => e.Amount) 
            })
            .ToListAsync();
        
        return Ok(new 
        {
            year,
            totalIncome,
            totalExpenses,
            netSavings = totalIncome - totalExpenses,
            monthlyExpenses = monthlyData
        });
    }
}