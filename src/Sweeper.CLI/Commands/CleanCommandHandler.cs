using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Spectre.Console;
using Sweeper.CLI.Configuration;
using Sweeper.CLI.Helpers;
using Sweeper.Domain.Enums;
using Sweeper.Domain.Interfaces;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace Sweeper.CLI.Commands;

public class CleanCommandHandler : ICommandHandler
{
    private readonly ILogger<CleanCommandHandler> _logger;
    private readonly ICleanupService _cleanupService;
    private readonly ILocalizationService _localizationService;
    private readonly SweeperOptions _options;

    public CleanCommandHandler(
        ILogger<CleanCommandHandler> logger,
        ICleanupService cleanupService,
        ILocalizationService localizationService,
        IOptions<SweeperOptions> options)
    {
        _logger = logger;
        _cleanupService = cleanupService;
        _localizationService = localizationService;
        _options = options.Value;
    }

    public int Invoke(InvocationContext context)
    {
        return InvokeAsync(context).GetAwaiter().GetResult();
    }

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        var categories = context.ParseResult.GetValueForOption<string[]>("--categories") ?? Array.Empty<string>();
        var force = context.ParseResult.GetValueForOption<bool>("--force");
        var dryRun = context.ParseResult.GetValueForOption<bool>("--dry-run");
        var verbose = context.ParseResult.GetValueForOption<bool>("--verbose");
        var quiet = context.ParseResult.GetValueForOption<bool>("--quiet");
        
        _logger.LogInformation("Starting cleanup operation");

        try
        {
            // Parse categories
            var cleanupCategories = ParseCategories(categories);
            
            if (!quiet)
            {
                AnsiConsole.Write(new Rule($"[blue]{_localizationService.GetString("command.clean")}[/]"));
                AnsiConsole.WriteLine();
            }

            // Scan for files
            if (!quiet)
            {
                AnsiConsole.Status()
                    .Spinner(Spinner.Known.Dots)
                    .Start($"[yellow]{_localizationService.GetString("message.scanning", "system")}[/]", async ctx =>
                    {
                        await Task.Delay(100); // Small delay for visual effect
                    });
            }

            var scanResult = await _cleanupService.ScanAsync(cleanupCategories);

            if (scanResult.TotalFiles == 0)
            {
                if (!quiet)
                {
                    AnsiConsole.MarkupLine($"[green]✓[/] {_localizationService.GetString("message.noFilesFound")}");
                }
                return 0;
            }

            // Display scan results
            if (!quiet)
            {
                var table = new Table();
                table.AddColumn("Category");
                table.AddColumn("Files");
                table.AddColumn("Size");

                // Group files by category (simplified for now)
                table.AddRow(
                    cleanupCategories.ToString(),
                    scanResult.TotalFiles.ToString(),
                    FileSizeHelper.FormatSize(scanResult.TotalSize));

                AnsiConsole.Write(table);
                AnsiConsole.WriteLine();
            }

            // Confirm cleanup
            if (!force && !dryRun && !quiet)
            {
                var confirmMessage = _localizationService.GetString("message.confirmCleanup", 
                    scanResult.TotalFiles, 
                    FileSizeHelper.FormatSize(scanResult.TotalSize));
                
                if (!AnsiConsole.Confirm(confirmMessage, false))
                {
                    AnsiConsole.MarkupLine("[yellow]Cleanup cancelled by user[/]");
                    return 0;
                }
            }

            if (dryRun)
            {
                if (!quiet)
                {
                    AnsiConsole.MarkupLine("[yellow]DRY RUN MODE - No files will be deleted[/]");
                    
                    if (verbose)
                    {
                        foreach (var file in scanResult.Files.Take(20))
                        {
                            AnsiConsole.MarkupLine($"  [dim]Would delete: {file.Path}[/]");
                        }
                        
                        if (scanResult.Files.Count > 20)
                        {
                            AnsiConsole.MarkupLine($"  [dim]... and {scanResult.Files.Count - 20} more files[/]");
                        }
                    }
                }
                return 0;
            }

            // Perform cleanup
            if (!quiet)
            {
                await AnsiConsole.Progress()
                    .Columns(new ProgressColumn[]
                    {
                        new TaskDescriptionColumn(),
                        new ProgressBarColumn(),
                        new PercentageColumn(),
                        new RemainingTimeColumn(),
                        new SpinnerColumn(),
                    })
                    .StartAsync(async ctx =>
                    {
                        var task = ctx.AddTask("[green]Cleaning files[/]", maxValue: scanResult.TotalFiles);

                        var cleanupResult = await _cleanupService.CleanAsync(scanResult);

                        task.Increment(cleanupResult.TotalFilesDeleted);
                        task.StopTask();
                    });
            }
            else
            {
                await _cleanupService.CleanAsync(scanResult);
            }

            // Display results
            if (!quiet)
            {
                AnsiConsole.WriteLine();
                AnsiConsole.Write(new Rule("[green]Cleanup Complete[/]"));
                
                var resultPanel = new Panel(new Markup(
                    $"[green]✓[/] Successfully cleaned [bold]{scanResult.TotalFiles}[/] files\n" +
                    $"[green]✓[/] Freed [bold]{FileSizeHelper.FormatSize(scanResult.TotalSize)}[/] of disk space"))
                {
                    Border = BoxBorder.Rounded,
                    BorderStyle = Style.Parse("green"),
                    Padding = new Padding(1)
                };
                
                AnsiConsole.Write(resultPanel);
            }

            _logger.LogInformation("Cleanup completed successfully");
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cleanup operation");
            
            if (!quiet)
            {
                AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
                
                if (verbose)
                {
                    AnsiConsole.WriteException(ex);
                }
            }
            
            return 1;
        }
    }

    private CleanupCategory ParseCategories(string[] categories)
    {
        if (categories == null || categories.Length == 0)
        {
            // Default to common temporary files
            return CleanupCategory.SystemTemp | CleanupCategory.UserTemp;
        }

        var result = CleanupCategory.None;
        
        foreach (var category in categories)
        {
            if (Enum.TryParse<CleanupCategory>(category, true, out var parsed))
            {
                result |= parsed;
            }
            else if (category.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                return CleanupCategory.All;
            }
        }

        return result == CleanupCategory.None ? 
            (CleanupCategory.SystemTemp | CleanupCategory.UserTemp) : result;
    }
}
