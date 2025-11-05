using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using System.Text.Json;

namespace AriksFinanceTracker.Api.Services;

public interface IDatabaseBackupService
{
    Task<string> CreateBackupAsync(string? backupName = null);
    Task<List<BackupInfo>> GetBackupsAsync();
    Task<bool> RestoreBackupAsync(string backupFileName);
    Task<string> ExportDataAsync();
    Task<bool> ImportDataAsync(string jsonData);
    Task CleanupOldBackupsAsync(int keepCount = 10);
}

public class DatabaseBackupService : IDatabaseBackupService
{
    private readonly FinanceContext _context;
    private readonly ILogger<DatabaseBackupService> _logger;
    private readonly string _backupDirectory;

    public DatabaseBackupService(FinanceContext context, ILogger<DatabaseBackupService> logger)
    {
        _context = context;
        _logger = logger;
        _backupDirectory = Path.Combine(Directory.GetCurrentDirectory(), "backups");
        
        // Ensure backup directory exists
        if (!Directory.Exists(_backupDirectory))
        {
            Directory.CreateDirectory(_backupDirectory);
        }
    }

    public async Task<string> CreateBackupAsync(string? backupName = null)
    {
        try
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            backupName ??= $"backup_{timestamp}";
            var backupFileName = $"{backupName}.db";
            var backupPath = Path.Combine(_backupDirectory, backupFileName);

            // Get the connection string to find the source database file
            var connectionString = _context.Database.GetConnectionString();
            var sourcePath = ExtractDatabasePath(connectionString);

            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException($"Source database not found: {sourcePath}");
            }

            // Close any open connections to ensure clean backup
            await _context.Database.CloseConnectionAsync();

            // Copy the database file
            File.Copy(sourcePath, backupPath, overwrite: true);

            // Create backup metadata
            var metadata = new BackupInfo
            {
                FileName = backupFileName,
                CreatedAt = DateTime.UtcNow,
                Description = $"Automatic backup created at {DateTime.UtcNow}",
                FileSize = new FileInfo(backupPath).Length
            };

            var metadataPath = Path.Combine(_backupDirectory, $"{backupName}.json");
            var metadataJson = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(metadataPath, metadataJson);

            _logger.LogInformation($"Database backup created successfully: {backupFileName}");
            return backupFileName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create database backup");
            throw;
        }
    }

    public async Task<List<BackupInfo>> GetBackupsAsync()
    {
        var backups = new List<BackupInfo>();

        try
        {
            var metadataFiles = Directory.GetFiles(_backupDirectory, "*.json");

            foreach (var metadataFile in metadataFiles)
            {
                try
                {
                    var jsonContent = await File.ReadAllTextAsync(metadataFile);
                    var backup = JsonSerializer.Deserialize<BackupInfo>(jsonContent);
                    
                    if (backup != null)
                    {
                        // Verify the actual backup file exists
                        var backupFilePath = Path.Combine(_backupDirectory, backup.FileName);
                        if (File.Exists(backupFilePath))
                        {
                            backup.FileSize = new FileInfo(backupFilePath).Length;
                            backups.Add(backup);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Failed to read backup metadata: {metadataFile}");
                }
            }

            return backups.OrderByDescending(b => b.CreatedAt).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get backup list");
            return backups;
        }
    }

    public async Task<bool> RestoreBackupAsync(string backupFileName)
    {
        try
        {
            var backupPath = Path.Combine(_backupDirectory, backupFileName);
            
            if (!File.Exists(backupPath))
            {
                _logger.LogError($"Backup file not found: {backupFileName}");
                return false;
            }

            // Get the current database path
            var connectionString = _context.Database.GetConnectionString();
            var currentDbPath = ExtractDatabasePath(connectionString);

            // Create a backup of current database before restoring
            await CreateBackupAsync($"before_restore_{DateTime.UtcNow:yyyyMMdd_HHmmss}");

            // Close connections
            await _context.Database.CloseConnectionAsync();

            // Wait a moment to ensure connections are closed
            await Task.Delay(1000);

            // Replace the current database with the backup
            File.Copy(backupPath, currentDbPath, overwrite: true);

            _logger.LogInformation($"Database restored successfully from: {backupFileName}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to restore backup: {backupFileName}");
            return false;
        }
    }

    public async Task<string> ExportDataAsync()
    {
        try
        {
            var exportData = new DatabaseExport
            {
                ExportedAt = DateTime.UtcNow,
                Expenses = await _context.Expenses.Include(e => e.Category).ToListAsync(),
                Incomes = await _context.Incomes.ToListAsync(),
                SpendingCategories = await _context.SpendingCategories.ToListAsync(),
                BudgetLimits = await _context.BudgetLimits.Include(b => b.Category).Include(b => b.FinancialPeriod).ToListAsync(),
                FinancialPeriods = await _context.FinancialPeriods.ToListAsync(),
                SavingsGoals = await _context.SavingsGoals.Include(s => s.FinancialPeriod).ToListAsync(),
                TotalSavings = await _context.TotalSavings.ToListAsync()
            };

            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            };

            return JsonSerializer.Serialize(exportData, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export data");
            throw;
        }
    }

    public async Task<bool> ImportDataAsync(string jsonData)
    {
        try
        {
            // Create backup before import
            await CreateBackupAsync($"before_import_{DateTime.UtcNow:yyyyMMdd_HHmmss}");

            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            };

            var importData = JsonSerializer.Deserialize<DatabaseExport>(jsonData, options);
            
            if (importData == null)
            {
                _logger.LogError("Failed to deserialize import data");
                return false;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Clear existing data (in correct order to respect foreign keys)
                _context.BudgetLimits.RemoveRange(_context.BudgetLimits);
                _context.SavingsGoals.RemoveRange(_context.SavingsGoals);
                _context.Expenses.RemoveRange(_context.Expenses);
                _context.SpendingCategories.RemoveRange(_context.SpendingCategories);
                _context.FinancialPeriods.RemoveRange(_context.FinancialPeriods);
                _context.Incomes.RemoveRange(_context.Incomes);
                _context.TotalSavings.RemoveRange(_context.TotalSavings);

                await _context.SaveChangesAsync();

                // Import data (in correct order to respect foreign keys)
                if (importData.SpendingCategories?.Count > 0)
                {
                    _context.SpendingCategories.AddRange(importData.SpendingCategories);
                    await _context.SaveChangesAsync();
                }

                if (importData.FinancialPeriods?.Count > 0)
                {
                    _context.FinancialPeriods.AddRange(importData.FinancialPeriods);
                    await _context.SaveChangesAsync();
                }

                if (importData.Expenses?.Count > 0)
                {
                    // Clear navigation properties to avoid conflicts
                    foreach (var expense in importData.Expenses)
                    {
                        expense.Category = null!;
                    }
                    _context.Expenses.AddRange(importData.Expenses);
                    await _context.SaveChangesAsync();
                }

                if (importData.Incomes?.Count > 0)
                {
                    _context.Incomes.AddRange(importData.Incomes);
                    await _context.SaveChangesAsync();
                }

                if (importData.BudgetLimits?.Count > 0)
                {
                    // Clear navigation properties to avoid conflicts
                    foreach (var budget in importData.BudgetLimits)
                    {
                        budget.Category = null!;
                        budget.FinancialPeriod = null!;
                    }
                    _context.BudgetLimits.AddRange(importData.BudgetLimits);
                    await _context.SaveChangesAsync();
                }

                if (importData.SavingsGoals?.Count > 0)
                {
                    // Clear navigation properties to avoid conflicts
                    foreach (var goal in importData.SavingsGoals)
                    {
                        goal.FinancialPeriod = null!;
                    }
                    _context.SavingsGoals.AddRange(importData.SavingsGoals);
                    await _context.SaveChangesAsync();
                }

                if (importData.TotalSavings?.Count > 0)
                {
                    _context.TotalSavings.AddRange(importData.TotalSavings);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                _logger.LogInformation("Data imported successfully");
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import data");
            return false;
        }
    }

    public async Task CleanupOldBackupsAsync(int keepCount = 10)
    {
        try
        {
            var backups = await GetBackupsAsync();
            var backupsToDelete = backups.Skip(keepCount).ToList();

            foreach (var backup in backupsToDelete)
            {
                var backupPath = Path.Combine(_backupDirectory, backup.FileName);
                var metadataPath = Path.Combine(_backupDirectory, Path.GetFileNameWithoutExtension(backup.FileName) + ".json");

                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }

                if (File.Exists(metadataPath))
                {
                    File.Delete(metadataPath);
                }

                _logger.LogInformation($"Deleted old backup: {backup.FileName}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup old backups");
        }
    }

    private string ExtractDatabasePath(string? connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            return "ariks_finance.db";
        }

        // Parse SQLite connection string to get the database file path
        var parts = connectionString.Split(';');
        foreach (var part in parts)
        {
            var keyValue = part.Split('=');
            if (keyValue.Length == 2 && keyValue[0].Trim().Equals("Data Source", StringComparison.OrdinalIgnoreCase))
            {
                return keyValue[1].Trim();
            }
        }

        return "ariks_finance.db";
    }
}

public class BackupInfo
{
    public string FileName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Description { get; set; } = string.Empty;
    public long FileSize { get; set; }
}

public class DatabaseExport
{
    public DateTime ExportedAt { get; set; }
    public List<Expense>? Expenses { get; set; }
    public List<Income>? Incomes { get; set; }
    public List<SpendingCategory>? SpendingCategories { get; set; }
    public List<BudgetLimit>? BudgetLimits { get; set; }
    public List<FinancialPeriod>? FinancialPeriods { get; set; }
    public List<SavingsGoal>? SavingsGoals { get; set; }
    public List<TotalSavings>? TotalSavings { get; set; }
}