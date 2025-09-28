using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework
builder.Services.AddDbContext<FinanceContext>(options =>
    options.UseSqlite("Data Source=ariks_finance.db"));

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        builder => builder.WithOrigins("http://localhost:4201")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

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

public class FinanceContext : DbContext
{
    public FinanceContext(DbContextOptions<FinanceContext> options) : base(options) { }
    
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<Income> Incomes { get; set; }
}

public class Expense
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Category { get; set; }
    public string Description { get; set; }
}

public class Income
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Source { get; set; }
}
