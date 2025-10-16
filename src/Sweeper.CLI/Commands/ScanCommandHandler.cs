using Microsoft.Extensions.Logging;
using Spectre.Console;
using Sweeper.CLI.Helpers;
using Sweeper.Domain.Enums;
using Sweeper.Domain.Interfaces;
using System.CommandLine.Invocation;

namespace Sweeper.CLI.Commands;

public class ScanCommandHandler : ICommandHandler
{
    private readonly ILogger<ScanCommandHandler> _logger;
    private readonly ICleanupService _cleanupService;
    private readonly ILocalizationService _localizationService;

    public ScanCommandHandler(
        ILogger<ScanCommandHandler> logger,
        ICleanupService cleanupService,
        ILocalizationService localizationService)
    {
        _logger = logger;
        _cleanupService = cleanupService;
        _localizationService = localizationService;
    }

    public int Invoke(InvocationContext context)
    {
        return InvokeAsync(context).GetAwaiter().GetResult();
    }

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        var categories = context.ParseResult.GetValueForOption<string[]>("--categories") ?? Array.Empty<string>();
        var showDetails = context.ParseResult.GetValueForOption<bool>("--details");
        var sortBy = context.ParseResult.GetValueForOption<string>("--sort") ?? "size";
        var limit = context.ParseResult.GetValueForOption<int>("--limit");
        var quiet = context.ParseResult.GetValueForOption<bool>("--quiet");
        var verbose = context.ParseResult.GetValueForOption<bool>("--verbose");

        _logger.LogInformation("Starting scan operation");

        try
        {
            var cleanupCategories = ParseCategories(categories);

            if (!quiet)
            {
                AnsiConsole.Write(new Rule($"[blue]{_localizationService.GetString("command.scan")}[/]"));
                AnsiConsole.WriteLine();
            }

            // Perform scan with progress indicator
            var scanResult = await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync($"[yellow]{_localizationService.GetString("message.scanning", "system")}[/]", 
                    async ctx => await _cleanupService.ScanAsync(cleanupCategories));

            if (scanResult.TotalFiles == 0)
            {
                if (!quiet)
                {
                    AnsiConsole.MarkupLine($"[green]✓[/] {_localizationService.GetString("message.noFilesFound")}");
                }
                return 0;
            }

            // Sort files
            var sortedFiles = sortBy.ToLower() switch
            {
                "size" => scanResult.Files.OrderByDescending(f => f.Size),
                "date" => scanResult.Files.OrderByDescending(f => f.ModifiedAt),
                "name" => scanResult.Files.OrderBy(f => Path.GetFileName(f.Path)),
                _ => scanResult.Files.OrderByDescending(f => f.Size)
            };

            // Apply limit if specified
            if (limit > 0)
            {
                sortedFiles = sortedFiles.Take(limit);
            }

            if (!quiet)
            {
                // Display summary
                var summaryPanel = new Panel(new Markup(
                    $"[bold]Scan Results[/]\n" +
                    $"Total Files: [yellow]{scanResult.TotalFiles:N0}[/]\n" +
                    $"Total Size: [yellow]{FileSizeHelper.FormatSize(scanResult.TotalSize)}[/]\n" +
                    $"Scan Duration: [dim]{scanResult.ScanDuration.TotalSeconds:F2} seconds[/]"))
                {
                    Border = BoxBorder.Rounded,
                    BorderStyle = Style.Parse("blue"),
                    Padding = new Padding(1)
                };

                AnsiConsole.Write(summaryPanel);
                AnsiConsole.WriteLine();

                if (showDetails)
                {
                    // Create detailed table
                    var table = new Table();
                    table.Border(TableBorder.Rounded);
                    table.AddColumn("File Path");
                    table.AddColumn("Size");
                    table.AddColumn("Modified");
                    table.AddColumn("Attributes");

                    foreach (var file in sortedFiles)
                    {
                        var attributes = new List<string>();
                        if (file.IsReadOnly) attributes.Add("R");
                        if (file.IsSystemFile) attributes.Add("S");
                        if (file.IsHidden) attributes.Add("H");

                        table.AddRow(
                            Markup.Escape(TruncatePath(file.Path, 60)),
                            FileSizeHelper.FormatSize(file.Size),
                            file.ModifiedAt.ToString("yyyy-MM-dd HH:mm"),
                            string.Join(",", attributes));
                    }

                    AnsiConsole.Write(table);
                }
                else
                {
                    // Display top 10 largest files
                    var topFiles = sortedFiles.Take(10).ToList();
                    if (topFiles.Any())
                    {
                        AnsiConsole.MarkupLine("[bold]Top files by size:[/]");
                        foreach (var file in topFiles)
                        {
                            var bar = new BarChart()
                                .Width(40)
                                .Label($"[dim]{TruncatePath(file.Path, 50)}[/]")
                                .AddItem("", file.Size, Color.Yellow);
                            
                            AnsiConsole.Write(bar);
                            AnsiConsole.MarkupLine($" [dim]{FileSizeHelper.FormatSize(file.Size)}[/]");
                        }
                    }
                }

                // Show errors if any
                if (scanResult.HasErrors && verbose)
                {
                    AnsiConsole.WriteLine();
                    AnsiConsole.MarkupLine("[red]Errors encountered during scan:[/]");
                    foreach (var error in scanResult.Errors)
                    {
                        AnsiConsole.MarkupLine($"  [red]•[/] {error}");
                    }
                }
            }

            _logger.LogInformation("Scan completed successfully");
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during scan operation");
            
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

    private string TruncatePath(string path, int maxLength)
    {
        if (path.Length <= maxLength)
            return path;

        var fileName = Path.GetFileName(path);
        if (fileName.Length >= maxLength)
            return "..." + fileName.Substring(fileName.Length - maxLength + 3);

        var directory = Path.GetDirectoryName(path) ?? "";
        var availableLength = maxLength - fileName.Length - 4; // 4 for "\..."
        
        if (availableLength > 0)
        {
            return directory.Substring(0, Math.Min(directory.Length, availableLength)) + @"\..." + fileName;
        }

        return "..." + fileName;
    }
}
