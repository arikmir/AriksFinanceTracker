using AriksFinanceTracker.Api.Data;
using AriksFinanceTracker.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AriksFinanceTracker.Api.Services;

public class IncomeService
{
    private readonly FinanceContext _context;

    public IncomeService(FinanceContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Income>> GetIncomesAsync(int? month = null, int? year = null)
    {
        var query = _context.Incomes.AsQueryable();

        if (month.HasValue && year.HasValue)
        {
            var startDate = new DateTime(year.Value, month.Value, 1);
            var endDate = startDate.AddMonths(1);
            query = query.Where(i => i.Date >= startDate && i.Date < endDate);
        }

        return await query.OrderByDescending(i => i.Date).ToListAsync();
    }

    public async Task<Income?> GetIncomeByIdAsync(int id)
    {
        return await _context.Incomes.FindAsync(id);
    }

    public Task<(bool isValid, string? errorMessage)> ValidateIncomeAsync(Income income)
    {
        if (income.Amount <= 0)
        {
            return Task.FromResult<(bool, string?)>((false, "Income amount must be greater than zero"));
        }

        if (string.IsNullOrWhiteSpace(income.Source))
        {
            return Task.FromResult<(bool, string?)>((false, "Income source is required"));
        }

        return Task.FromResult<(bool, string?)>((true, null));
    }

    public async Task<Income> CreateIncomeAsync(Income income)
    {
        _context.Incomes.Add(income);
        await _context.SaveChangesAsync();
        return income;
    }

    public async Task<bool> UpdateIncomeAsync(int id, Income income)
    {
        if (id != income.Id)
        {
            return false;
        }

        _context.Entry(income).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await IncomeExistsAsync(id))
            {
                return false;
            }
            throw;
        }
    }

    public async Task<bool> DeleteIncomeAsync(int id)
    {
        var income = await _context.Incomes.FindAsync(id);
        if (income == null)
        {
            return false;
        }

        _context.Incomes.Remove(income);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IncomeExistsAsync(int id)
    {
        return await _context.Incomes.AnyAsync(i => i.Id == id);
    }
}
