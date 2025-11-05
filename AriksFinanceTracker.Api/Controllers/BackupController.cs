using Microsoft.AspNetCore.Mvc;
using AriksFinanceTracker.Api.Services;

namespace AriksFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BackupController : ControllerBase
{
    private readonly IDatabaseBackupService _backupService;
    private readonly ILogger<BackupController> _logger;

    public BackupController(IDatabaseBackupService backupService, ILogger<BackupController> logger)
    {
        _backupService = backupService;
        _logger = logger;
    }

    [HttpPost("create")]
    public async Task<ActionResult<BackupResponse>> CreateBackup([FromBody] CreateBackupRequest? request = null)
    {
        try
        {
            var backupFileName = await _backupService.CreateBackupAsync(request?.Name);
            return Ok(new BackupResponse 
            { 
                Success = true, 
                Message = "Backup created successfully", 
                BackupFileName = backupFileName 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create backup");
            return StatusCode(500, new BackupResponse 
            { 
                Success = false, 
                Message = "Failed to create backup: " + ex.Message 
            });
        }
    }

    [HttpGet("list")]
    public async Task<ActionResult<List<BackupInfo>>> GetBackups()
    {
        try
        {
            var backups = await _backupService.GetBackupsAsync();
            return Ok(backups);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get backup list");
            return StatusCode(500, "Failed to get backup list");
        }
    }

    [HttpPost("restore/{backupFileName}")]
    public async Task<ActionResult<BackupResponse>> RestoreBackup(string backupFileName)
    {
        try
        {
            var success = await _backupService.RestoreBackupAsync(backupFileName);
            
            if (success)
            {
                return Ok(new BackupResponse 
                { 
                    Success = true, 
                    Message = "Database restored successfully" 
                });
            }
            else
            {
                return BadRequest(new BackupResponse 
                { 
                    Success = false, 
                    Message = "Failed to restore backup" 
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restore backup");
            return StatusCode(500, new BackupResponse 
            { 
                Success = false, 
                Message = "Failed to restore backup: " + ex.Message 
            });
        }
    }

    [HttpGet("export")]
    public async Task<ActionResult> ExportData()
    {
        try
        {
            var jsonData = await _backupService.ExportDataAsync();
            var fileName = $"finance_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
            
            return File(
                System.Text.Encoding.UTF8.GetBytes(jsonData),
                "application/json",
                fileName
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export data");
            return StatusCode(500, "Failed to export data");
        }
    }

    [HttpPost("import")]
    public async Task<ActionResult<BackupResponse>> ImportData(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new BackupResponse 
                { 
                    Success = false, 
                    Message = "No file provided" 
                });
            }

            using var stream = new StreamReader(file.OpenReadStream());
            var jsonData = await stream.ReadToEndAsync();

            var success = await _backupService.ImportDataAsync(jsonData);

            if (success)
            {
                return Ok(new BackupResponse 
                { 
                    Success = true, 
                    Message = "Data imported successfully" 
                });
            }
            else
            {
                return BadRequest(new BackupResponse 
                { 
                    Success = false, 
                    Message = "Failed to import data" 
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import data");
            return StatusCode(500, new BackupResponse 
            { 
                Success = false, 
                Message = "Failed to import data: " + ex.Message 
            });
        }
    }

    [HttpPost("cleanup")]
    public async Task<ActionResult<BackupResponse>> CleanupOldBackups([FromBody] CleanupRequest? request = null)
    {
        try
        {
            var keepCount = request?.KeepCount ?? 10;
            await _backupService.CleanupOldBackupsAsync(keepCount);
            
            return Ok(new BackupResponse 
            { 
                Success = true, 
                Message = $"Old backups cleaned up, keeping {keepCount} most recent" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup old backups");
            return StatusCode(500, new BackupResponse 
            { 
                Success = false, 
                Message = "Failed to cleanup old backups: " + ex.Message 
            });
        }
    }

    [HttpGet("download/{backupFileName}")]
    public async Task<ActionResult> DownloadBackup(string backupFileName)
    {
        try
        {
            var backupPath = Path.Combine(Directory.GetCurrentDirectory(), "backups", backupFileName);
            
            if (!System.IO.File.Exists(backupPath))
            {
                return NotFound("Backup file not found");
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(backupPath);
            return File(fileBytes, "application/octet-stream", backupFileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download backup");
            return StatusCode(500, "Failed to download backup");
        }
    }
}

public class BackupResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? BackupFileName { get; set; }
}

public class CreateBackupRequest
{
    public string? Name { get; set; }
}

public class CleanupRequest
{
    public int KeepCount { get; set; } = 10;
}