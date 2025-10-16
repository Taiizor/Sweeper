using Microsoft.Extensions.Logging;
using Sweeper.Domain.Entities;
using Sweeper.Domain.Enums;
using Sweeper.Domain.Interfaces;

namespace Sweeper.Application.Services;

public class CleanupService : ICleanupService
{
    private readonly ILogger<CleanupService> _logger;
    private readonly IOperatingSystemService _osService;
    private readonly IEnumerable<ICleanupStrategy> _strategies;
    private readonly IFileSystemService _fileSystemService;

    public CleanupService(
        ILogger<CleanupService> logger,
        IOperatingSystemService osService,
        IEnumerable<ICleanupStrategy> strategies,
        IFileSystemService fileSystemService)
    {
        _logger = logger;
        _osService = osService;
        _strategies = strategies;
        _fileSystemService = fileSystemService;
    }

    public async Task<ScanResult> ScanAsync(CleanupTarget target, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting scan for target: {TargetName}", target.Name);
        
        var strategy = GetStrategyForTarget(target);
        if (strategy == null)
        {
            var result = new ScanResult(target);
            result.AddError($"No strategy found for target: {target.Name}");
            return result;
        }

        try
        {
            var scanResult = await strategy.ScanAsync(target, cancellationToken);
            _logger.LogInformation("Scan completed. Found {FileCount} files, Total size: {TotalSize} bytes", 
                scanResult.TotalFiles, scanResult.TotalSize);
            return scanResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during scan for target: {TargetName}", target.Name);
            var errorResult = new ScanResult(target);
            errorResult.AddError(ex.Message);
            return errorResult;
        }
    }

    public async Task<ScanResult> ScanAsync(CleanupCategory categories, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting scan for categories: {Categories}", categories);
        
        var aggregatedResult = new ScanResult();
        var relevantStrategies = GetStrategiesForCategories(categories);

        foreach (var strategy in relevantStrategies)
        {
            var targets = await strategy.GetTargetsAsync();
            foreach (var target in targets)
            {
                if ((target.Category & categories) != 0)
                {
                    var scanResult = await ScanAsync(target, cancellationToken);
                    MergeScanResults(aggregatedResult, scanResult);
                }
            }
        }

        return aggregatedResult;
    }

    public async Task<List<ScanResult>> ScanAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting full system scan");
        
        var results = new List<ScanResult>();
        var currentOS = _osService.GetOperatingSystem();

        foreach (var strategy in _strategies.Where(s => s.SupportedOS == currentOS))
        {
            var targets = await strategy.GetTargetsAsync();
            foreach (var target in targets)
            {
                var scanResult = await ScanAsync(target, cancellationToken);
                results.Add(scanResult);
            }
        }

        _logger.LogInformation("Full system scan completed. Total targets scanned: {Count}", results.Count);
        return results;
    }

    public async Task<CleanupResult> CleanAsync(ScanResult scanResult, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting cleanup for {FileCount} files", scanResult.TotalFiles);
        
        var cleanupResult = new CleanupResult(scanResult);
        var startTime = DateTime.UtcNow;

        if (scanResult.Target == null)
        {
            cleanupResult.AddFailedFile("Unknown", 0, "Scan result has no associated target");
            return cleanupResult;
        }

        var strategy = GetStrategyForTarget(scanResult.Target);
        if (strategy == null)
        {
            cleanupResult.AddFailedFile("Unknown", 0, $"No strategy found for target: {scanResult.Target.Name}");
            return cleanupResult;
        }

        try
        {
            cleanupResult = await strategy.CleanAsync(scanResult, cancellationToken);
            cleanupResult.CleanupDuration = DateTime.UtcNow - startTime;
            cleanupResult.IsCompleted = true;
            
            _logger.LogInformation("Cleanup completed. Deleted: {DeletedCount} files, Freed: {FreedSpace} bytes, Failed: {FailedCount} files",
                cleanupResult.TotalFilesDeleted, cleanupResult.TotalFreedSpace, cleanupResult.TotalFilesFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cleanup");
            cleanupResult.AddFailedFile("Unknown", 0, ex.Message);
        }

        return cleanupResult;
    }

    public async Task<CleanupResult> CleanAsync(CleanupCategory categories, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting cleanup for categories: {Categories}", categories);
        
        var scanResult = await ScanAsync(categories, cancellationToken);
        return await CleanAsync(scanResult, cancellationToken);
    }

    public async Task<long> EstimateSizeAsync(CleanupCategory categories, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Estimating size for categories: {Categories}", categories);
        
        var scanResult = await ScanAsync(categories, cancellationToken);
        return scanResult.TotalSize;
    }

    public async Task<bool> ValidatePermissionsAsync(CleanupTarget target)
    {
        _logger.LogDebug("Validating permissions for target: {TargetName}", target.Name);
        
        if (target.RequiresElevation && !_osService.IsElevated())
        {
            _logger.LogWarning("Target {TargetName} requires elevation but application is not elevated", target.Name);
            return false;
        }

        foreach (var path in target.Paths)
        {
            var expandedPath = _fileSystemService.ExpandEnvironmentVariables(path);
            if (_fileSystemService.DirectoryExists(expandedPath))
            {
                if (!_fileSystemService.HasWritePermission(expandedPath))
                {
                    _logger.LogWarning("No write permission for path: {Path}", expandedPath);
                    return false;
                }
            }
        }

        return true;
    }

    private ICleanupStrategy? GetStrategyForTarget(CleanupTarget target)
    {
        return _strategies.FirstOrDefault(s => 
            s.SupportedOS == target.OperatingSystem && 
            (s.Category & target.Category) != 0);
    }

    private IEnumerable<ICleanupStrategy> GetStrategiesForCategories(CleanupCategory categories)
    {
        var currentOS = _osService.GetOperatingSystem();
        return _strategies.Where(s => 
            s.SupportedOS == currentOS && 
            (s.Category & categories) != 0);
    }

    private void MergeScanResults(ScanResult target, ScanResult source)
    {
        foreach (var file in source.Files)
        {
            target.AddFile(file);
        }

        foreach (var error in source.Errors)
        {
            target.AddError(error);
        }

        target.ScanDuration += source.ScanDuration;
    }
}
