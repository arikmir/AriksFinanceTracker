using Microsoft.EntityFrameworkCore;

namespace AriksFinanceTracker.Api.Services;

public class AutoBackupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AutoBackupService> _logger;
    private readonly TimeSpan _backupInterval;

    public AutoBackupService(IServiceProvider serviceProvider, ILogger<AutoBackupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _backupInterval = TimeSpan.FromHours(6); // Backup every 6 hours
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Auto backup service started");

        // Create initial backup on startup
        await CreateBackupAsync("startup");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_backupInterval, stoppingToken);
                
                if (!stoppingToken.IsCancellationRequested)
                {
                    await CreateBackupAsync("auto");
                    await CleanupOldBackupsAsync();
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in auto backup service");
                // Continue running even if backup fails
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken); // Wait 30 minutes before retry
            }
        }

        _logger.LogInformation("Auto backup service stopped");
    }

    private async Task CreateBackupAsync(string prefix)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var backupService = scope.ServiceProvider.GetRequiredService<IDatabaseBackupService>();
            
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var backupName = $"{prefix}_{timestamp}";
            
            var backupFileName = await backupService.CreateBackupAsync(backupName);
            _logger.LogInformation($"Auto backup created: {backupFileName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create auto backup");
        }
    }

    private async Task CleanupOldBackupsAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var backupService = scope.ServiceProvider.GetRequiredService<IDatabaseBackupService>();
            
            await backupService.CleanupOldBackupsAsync(20); // Keep 20 most recent backups
            _logger.LogInformation("Old backups cleaned up");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup old backups");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Auto backup service is stopping, creating final backup...");
        
        try
        {
            await CreateBackupAsync("shutdown");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create shutdown backup");
        }

        await base.StopAsync(cancellationToken);
    }
}