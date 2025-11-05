using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class FinanceContextFactory : IDesignTimeDbContextFactory<FinanceContext>
{
    public FinanceContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FinanceContext>();
        optionsBuilder.UseSqlite("Data Source=ariks_finance.db");

        return new FinanceContext(optionsBuilder.Options);
    }
}
