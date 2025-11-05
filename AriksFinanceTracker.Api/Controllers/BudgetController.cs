using AriksFinanceTracker.Api.Models.Dto;
using AriksFinanceTracker.Api.Models.Entities;
using AriksFinanceTracker.Api.Models.Enums;
using AriksFinanceTracker.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AriksFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BudgetController : ControllerBase
{
    private readonly BudgetService _budgetService;

    public BudgetController(BudgetService budgetService)
    {
        _budgetService = budgetService;
    }

    [HttpPost("initialize")]
    public async Task<ActionResult> InitializeBudget()
    {
        try
        {
            await _budgetService.InitializeAriksBudgetAsync();
            return Ok(new { message = "Budget system initialized successfully! 🎉" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error initializing budget", error = ex.Message });
        }
    }

    [HttpGet("status")]
    public async Task<ActionResult<BudgetStatusDto>> GetBudgetStatus()
    {
        try
        {
            var status = await _budgetService.GetCurrentBudgetStatusAsync();
            return Ok(status);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error getting budget status", error = ex.Message });
        }
    }

    [HttpPost("check-spending")]
    public async Task<ActionResult<SpendingCheckDto>> CheckSpending([FromBody] CheckSpendingRequest request)
    {
        try
        {
            var result = await _budgetService.CheckSpendingAsync(request.CategoryId, request.Amount);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error checking spending", error = ex.Message });
        }
    }

    [HttpGet("alerts")]
    public async Task<ActionResult<List<SpendingAlert>>> GetAlerts()
    {
        try
        {
            var alerts = await _budgetService.GetActiveAlertsAsync();
            return Ok(alerts);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error getting alerts", error = ex.Message });
        }
    }

    [HttpGet("financial-health")]
    public async Task<ActionResult<FinancialHealthDto>> GetFinancialHealth()
    {
        try
        {
            var health = await _budgetService.GetFinancialHealthAsync();
            return Ok(health);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error getting financial health", error = ex.Message });
        }
    }

    [HttpGet("savings-celebration")]
    public async Task<ActionResult> GetSavingsCelebration()
    {
        try
        {
            var status = await _budgetService.GetCurrentBudgetStatusAsync();
            
            var celebrationMessage = status.SavingsRate switch
            {
                >= 30 => "🚀 INCREDIBLE! You're saving 30%+ - You're a financial rockstar!",
                >= 24 => "🎉 PERFECT! You've hit your 24% target - Building serious wealth!",
                >= 20 => "💪 EXCELLENT! 20%+ savings rate - You're crushing your goals!",
                >= 15 => "👍 GREAT! Solid progress - Keep building that wealth!",
                _ => "🌱 GROWING! Every dollar saved is progress towards financial freedom!"
            };

            var achievements = new List<string>();
            if (status.SavingsRate >= 20) achievements.Add("🏆 Strong Saver Badge");
            if (status.SavingsRate >= 24) achievements.Add("🎯 Target Achieved Badge");
            if (status.SavingsRate >= 30) achievements.Add("🚀 Savings Superstar Badge");

            return Ok(new
            {
                message = celebrationMessage,
                savingsRate = status.SavingsRate,
                savingsAmount = status.ActualSavings,
                achievements = achievements,
                encouragement = "Your future self will thank you for every dollar saved today! 💫"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error getting celebration", error = ex.Message });
        }
    }

    [HttpGet("limits")]
    public async Task<ActionResult<List<BudgetLimitDto>>> GetBudgetLimits()
    {
        try
        {
            var limits = await _budgetService.GetBudgetLimitsAsync();
            return Ok(limits);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error getting budget limits", error = ex.Message });
        }
    }

    [HttpGet("categories")]
    public async Task<ActionResult<List<SpendingCategoryDto>>> GetBudgetCategories()
    {
        try
        {
            var categories = await _budgetService.GetSpendingCategoriesAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error getting budget categories", error = ex.Message });
        }
    }

    [HttpPost("categories")]
    public async Task<ActionResult<CategoryBudgetDto>> CreateBudgetCategory([FromBody] CreateBudgetCategoryRequest request)
    {
        try
        {
            var category = await _budgetService.CreateCustomCategoryAsync(request.Name, request.MonthlyLimit, request.IsEssential);
            return Ok(category);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error creating budget category", error = ex.Message });
        }
    }

    [HttpPut("category/{categoryId}/limit")]
    public async Task<ActionResult> UpdateCategoryLimit(int categoryId, [FromBody] UpdateBudgetLimitRequest request)
    {
        try
        {
            await _budgetService.UpdateCategoryLimitAsync(categoryId, request.NewLimit, request.IsEssential, request.Name);
            return Ok(new { message = "Budget limit updated successfully!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error updating budget limit", error = ex.Message });
        }
    }
}

public class CheckSpendingRequest
{
    public int CategoryId { get; set; }
    public decimal Amount { get; set; }
}

public class UpdateBudgetLimitRequest
{
    public decimal NewLimit { get; set; }
    public bool? IsEssential { get; set; }
    public string? Name { get; set; }
}

public class CreateBudgetCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal MonthlyLimit { get; set; }
    public bool IsEssential { get; set; }
}
