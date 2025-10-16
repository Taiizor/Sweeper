using Microsoft.Extensions.Logging;
using Spectre.Console;
using Sweeper.CLI.Helpers;
using Sweeper.Domain.Enums;
using Sweeper.Domain.Interfaces;
using System.CommandLine.Invocation;

namespace Sweeper.CLI.Commands;

public class EstimateCommandHandler : ICommandHandler
{
    private readonly ILogger<EstimateCommandHandler> _logger;
    private readonly ICleanupService _cleanupService;
    private readonly ILocalizationService _localizationService;
    private readonly IOperatingSystemService _osService;

    public EstimateCommandHandler(
        ILogger<EstimateCommandHandler> logger,
        ICleanupService cleanupService,
        ILocalizationService localizationService,
        IOperatingSystemService osService)
    {
        _logger = logger;
        _cleanupService = cleanupService;
        _localizationService = localizationService;
        _osService = osService;
    }

    public int Invoke(InvocationContext context)
    {
        return InvokeAsync(context).GetAwaiter().GetResult();
    }

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        var categories = context.ParseResult.GetValueForOption<string[]>("--categories") ?? Array.Empty<string>();
        var showDetails = context.ParseResult.GetValueForOption<bool>("--details");
        var quiet = context.ParseResult.GetValueForOption<bool>("--quiet");
        var verbose = context.ParseResult.GetValueForOption<bool>("--verbose");

        _logger.LogInformation("Estimating disk space");

        try
        {
            var cleanupCategories = ParseCategories(categories);

            if (!quiet)
            {
                AnsiConsole.Write(new Rule($"[blue]{_localizationService.GetString("command.estimate")}[/]"));
                AnsiConsole.WriteLine();
            }

            // Get available disk space
            var systemDrive = _osService.IsWindows() ? "C:" : "/";
            var availableSpace = _osService.GetAvailableDiskSpace(systemDrive);
            var totalSpace = _osService.GetTotalDiskSpace(systemDrive);

            if (!quiet)
            {
                // Display disk information
                var diskInfo = new Panel(new Markup(
                    $"[bold]Disk Space Information[/]\n" +
                    $"Total Space: [dim]{FileSizeHelper.FormatSize(totalSpace)}[/]\n" +
                    $"Available Space: [green]{FileSizeHelper.FormatSize(availableSpace)}[/]\n" +
                    $"Used Space: [yellow]{FileSizeHelper.FormatSize(totalSpace - availableSpace)}[/]"))
                {
                    Border = BoxBorder.Rounded,
                    BorderStyle = Style.Parse("dim"),
                    Padding = new Padding(1)
                };

                AnsiConsole.Write(diskInfo);
                AnsiConsole.WriteLine();
            }

            // Perform estimation
            long totalEstimatedSize = 0;
            var categoryResults = new Dictionary<CleanupCategory, long>();

            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync($"[yellow]Estimating space to be freed...[/]", async ctx =>
                {
                    if (showDetails)
                    {
                        // Estimate each category separately
                        foreach (var category in Enum.GetValues<CleanupCategory>())
                        {
                            if (category == CleanupCategory.None || category == CleanupCategory.All)
                                continue;

                            if ((cleanupCategories & category) != 0)
                            {
                                ctx.Status($"[yellow]Estimating {category}...[/]");
                                var size = await _cleanupService.EstimateSizeAsync(category);
                                categoryResults[category] = size;
                                totalEstimatedSize += size;
                            }
                        }
                    }
                    else
                    {
                        // Estimate all at once
                        totalEstimatedSize = await _cleanupService.EstimateSizeAsync(cleanupCategories);
                    }
                });

            if (!quiet)
            {
                if (showDetails && categoryResults.Any())
                {
                    // Create detailed breakdown
                    var table = new Table();
                    table.Border(TableBorder.Rounded);
                    table.AddColumn("[bold]Category[/]");
                    table.AddColumn("[bold]Estimated Size[/]");
                    table.AddColumn("[bold]Percentage[/]");

                    foreach (var (category, size) in categoryResults.OrderByDescending(x => x.Value))
                    {
                        var percentage = totalEstimatedSize > 0 ? 
                            (double)size / totalEstimatedSize * 100 : 0;
                        
                        var localizedName = _localizationService.GetString($"category.{category.ToString().ToLower()}");
                        
                        table.AddRow(
                            localizedName,
                            FileSizeHelper.FormatSize(size),
                            $"{percentage:F1}%");
                    }

                    table.AddEmptyRow();
                    table.AddRow(
                        "[bold]Total[/]",
                        $"[bold green]{FileSizeHelper.FormatSize(totalEstimatedSize)}[/]",
                        "100%");

                    AnsiConsole.Write(table);
                    AnsiConsole.WriteLine();

                    // Create visual chart
                    if (categoryResults.Count > 0)
                    {
                        var chart = new BreakdownChart()
                            .Width(60)
                            .ShowPercentage();

                        foreach (var (category, size) in categoryResults.Where(x => x.Value > 0))
                        {
                            var localizedName = _localizationService.GetString($"category.{category.ToString().ToLower()}");
                            chart.AddItem(localizedName, size, GetCategoryColor(category));
                        }

                        AnsiConsole.Write(new Panel(chart)
                        {
                            Header = new PanelHeader("Space Distribution", Justify.Center),
                            Border = BoxBorder.Rounded,
                            BorderStyle = Style.Parse("blue")
                        });
                    }
                }
                else
                {
                    // Simple result
                    var resultPanel = new Panel(new Markup(
                        $"[bold]Estimation Complete[/]\n\n" +
                        $"Estimated space that can be freed: [bold green]{FileSizeHelper.FormatSize(totalEstimatedSize)}[/]\n" +
                        $"This is approximately [yellow]{(totalEstimatedSize * 100.0 / totalSpace):F2}%[/] of total disk space"))
                    {
                        Border = BoxBorder.Rounded,
                        BorderStyle = Style.Parse("green"),
                        Padding = new Padding(1)
                    };

                    AnsiConsole.Write(resultPanel);
                }

                AnsiConsole.WriteLine();

                // Show recommendation
                if (totalEstimatedSize > 0)
                {
                    var recommendation = totalEstimatedSize switch
                    {
                        > 10_737_418_240 => "[red]High[/] - Significant space can be recovered. Cleanup is highly recommended.",
                        > 1_073_741_824 => "[yellow]Medium[/] - Notable space can be recovered. Consider running cleanup.",
                        > 104_857_600 => "[green]Low[/] - Some space can be recovered.",
                        _ => "[dim]Minimal[/] - Very little space to recover."
                    };

                    AnsiConsole.MarkupLine($"[bold]Recommendation:[/] {recommendation}");
                }
            }

            _logger.LogInformation("Estimation completed: {Size} bytes", totalEstimatedSize);
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during estimation");
            
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
            return CleanupCategory.All;
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

        return result == CleanupCategory.None ? CleanupCategory.All : result;
    }

    private Color GetCategoryColor(CleanupCategory category)
    {
        return category switch
        {
            CleanupCategory.SystemTemp => Color.Red,
            CleanupCategory.UserTemp => Color.Orange1,
            CleanupCategory.BrowserCache => Color.Blue,
            CleanupCategory.ApplicationCache => Color.Green,
            CleanupCategory.LogFiles => Color.Yellow,
            CleanupCategory.Thumbnails => Color.Purple,
            CleanupCategory.RecycleBin => Color.Grey,
            CleanupCategory.DownloadedPrograms => Color.Cyan1,
            CleanupCategory.WindowsUpdate => Color.Magenta1,
            CleanupCategory.PackageManagerCache => Color.Lime,
            CleanupCategory.BuildArtifacts => Color.Teal,
            _ => Color.White
        };
    }
}
