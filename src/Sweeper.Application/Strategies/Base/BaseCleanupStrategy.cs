using Microsoft.Extensions.Logging;
using Sweeper.Domain.Entities;
using Sweeper.Domain.Enums;
using Sweeper.Domain.Interfaces;

namespace Sweeper.Application.Strategies.Base;

public abstract class BaseCleanupStrategy : ICleanupStrategy
{
    protected readonly ILogger<BaseCleanupStrategy> Logger;
    protected readonly IFileSystemService FileSystemService;
    protected readonly IOperatingSystemService OsService;

    public abstract OperatingSystemType SupportedOS { get; }
    public abstract CleanupCategory Category { get; }
    public abstract string Name { get; }
    public abstract string Description { get; }
    public virtual int Priority { get; } = 0;
    public virtual bool RequiresElevation { get; } = false;

    protected BaseCleanupStrategy(
        ILogger<BaseCleanupStrategy> logger,
        IFileSystemService fileSystemService,
        IOperatingSystemService osService)
    {
        Logger = logger;
        FileSystemService = fileSystemService;
        OsService = osService;
    }

    public abstract Task<List<CleanupTarget>> GetTargetsAsync();

    public virtual async Task<ScanResult> ScanAsync(CleanupTarget target, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Scanning target: {TargetName}", target.Name);
        var result = new ScanResult(target);
        var startTime = DateTime.UtcNow;

        try
        {
            foreach (var path in target.Paths)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                var expandedPath = FileSystemService.ExpandEnvironmentVariables(path);
                Logger.LogDebug("Scanning path: {Path}", expandedPath);

                if (!FileSystemService.DirectoryExists(expandedPath) && !FileSystemService.FileExists(expandedPath))
                {
                    Logger.LogWarning("Path does not exist: {Path}", expandedPath);
                    continue;
                }

                var files = await FileSystemService.ScanDirectoryAsync(
                    expandedPath, 
                    target.Patterns, 
                    target.Exclusions, 
                    cancellationToken);

                foreach (var file in files)
                {
                    result.AddFile(file);
                }
            }

            result.ScanDuration = DateTime.UtcNow - startTime;
            Logger.LogInformation("Scan completed for {TargetName}. Found {FileCount} files, Total size: {Size} bytes", 
                target.Name, result.TotalFiles, result.TotalSize);
        }
        catch (OperationCanceledException)
        {
            Logger.LogWarning("Scan cancelled for target: {TargetName}", target.Name);
            result.AddError("Scan was cancelled");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error scanning target: {TargetName}", target.Name);
            result.AddError($"Scan error: {ex.Message}");
        }

        return result;
    }

    public virtual async Task<CleanupResult> CleanAsync(ScanResult scanResult, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Starting cleanup for {FileCount} files", scanResult.TotalFiles);
        var result = new CleanupResult(scanResult);
        var startTime = DateTime.UtcNow;

        foreach (var file in scanResult.Files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                bool deleted;
                if (scanResult.Target?.Category == CleanupCategory.RecycleBin)
                {
                    deleted = await FileSystemService.MoveToRecycleBinAsync(file.Path);
                }
                else
                {
                    if (FileSystemService.DirectoryExists(file.Path))
                    {
                        deleted = await FileSystemService.DeleteDirectoryAsync(file.Path, true);
                    }
                    else
                    {
                        deleted = await FileSystemService.DeleteFileAsync(file.Path);
                    }
                }

                if (deleted)
                {
                    result.AddCleanedFile(file.Path, file.Size);
                    Logger.LogDebug("Deleted: {Path}", file.Path);
                }
                else
                {
                    result.AddFailedFile(file.Path, file.Size, "Failed to delete file");
                    Logger.LogWarning("Failed to delete: {Path}", file.Path);
                }
            }
            catch (Exception ex)
            {
                result.AddFailedFile(file.Path, file.Size, ex.Message);
                Logger.LogError(ex, "Error deleting file: {Path}", file.Path);
            }
        }

        result.CleanupDuration = DateTime.UtcNow - startTime;
        result.IsCompleted = true;
        
        Logger.LogInformation("Cleanup completed. Deleted: {DeletedCount}, Failed: {FailedCount}, Freed: {FreedSpace} bytes",
            result.TotalFilesDeleted, result.TotalFilesFailed, result.TotalFreedSpace);

        return result;
    }

    public virtual async Task<bool> ValidateAsync(CleanupTarget target)
    {
        if (target.OperatingSystem != SupportedOS)
        {
            Logger.LogWarning("Target OS {TargetOS} does not match strategy OS {StrategyOS}", 
                target.OperatingSystem, SupportedOS);
            return false;
        }

        if ((target.Category & Category) == 0)
        {
            Logger.LogWarning("Target category {TargetCategory} does not match strategy category {StrategyCategory}", 
                target.Category, Category);
            return false;
        }

        if (target.RequiresElevation && !OsService.IsElevated())
        {
            Logger.LogWarning("Target requires elevation but application is not elevated");
            return false;
        }

        return true;
    }
}
