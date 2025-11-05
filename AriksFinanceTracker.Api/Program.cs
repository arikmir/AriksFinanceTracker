using AriksFinanceTracker.Api.Data;
using AriksFinanceTracker.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework
builder.Services.AddDbContext<FinanceContext>(options =>
    options.UseSqlite("Data Source=ariks_finance.db"));

// Register Services
builder.Services.AddScoped<BudgetService>();
builder.Services.AddScoped<ExpenseService>();
builder.Services.AddScoped<IncomeService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        builder => builder.WithOrigins("http://localhost:4201")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<FinanceContext>();
    context.Database.EnsureCreated();

    var budgetService = scope.ServiceProvider.GetRequiredService<BudgetService>();
    await budgetService.InitializeAriksBudgetAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");
app.UseAuthorization();
app.MapControllers();

app.Run();
