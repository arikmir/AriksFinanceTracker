using Microsoft.AspNetCore.Mvc;
using AriksFinanceTracker.Api.Services;

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
            var result = await _budgetService.CheckSpendingAsync(request.Category, request.Amount);
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
}

public class CheckSpendingRequest
{
    public ExpenseCategory Category { get; set; }
    public decimal Amount { get; set; }
}