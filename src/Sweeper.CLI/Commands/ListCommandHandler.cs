using Microsoft.Extensions.Logging;
using Spectre.Console;
using Sweeper.Domain.Enums;
using Sweeper.Domain.Interfaces;
using System.CommandLine.Invocation;

namespace Sweeper.CLI.Commands;

public class ListCommandHandler : ICommandHandler
{
    private readonly ILogger<ListCommandHandler> _logger;
    private readonly ILocalizationService _localizationService;
    private readonly IOperatingSystemService _osService;

    public ListCommandHandler(
        ILogger<ListCommandHandler> logger,
        ILocalizationService localizationService,
        IOperatingSystemService osService)
    {
        _logger = logger;
        _localizationService = localizationService;
        _osService = osService;
    }

    public int Invoke(InvocationContext context)
    {
        return InvokeAsync(context).GetAwaiter().GetResult();
    }

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        var showAll = context.ParseResult.GetValueForOption<bool>("--all");
        var showDetails = context.ParseResult.GetValueForOption<bool>("--details");
        var quiet = context.ParseResult.GetValueForOption<bool>("--quiet");

        _logger.LogInformation("Listing cleanup categories");

        try
        {
            if (!quiet)
            {
                AnsiConsole.Write(new Rule($"[blue]{_localizationService.GetString("command.list")}[/]"));
                AnsiConsole.WriteLine();

                var currentOS = _osService.GetOperatingSystem();
                AnsiConsole.MarkupLine($"[dim]Operating System: {currentOS}[/]");
                AnsiConsole.WriteLine();
            }

            var categories = Enum.GetValues<CleanupCategory>()
                .Where(c => c != CleanupCategory.None && c != CleanupCategory.All)
                .ToList();

            if (!quiet)
            {
                var table = new Table();
                table.Border(TableBorder.Rounded);
                table.AddColumn("[bold]Category[/]");
                table.AddColumn("[bold]Name[/]");
                table.AddColumn("[bold]Description[/]");
                
                if (showDetails)
                {
                    table.AddColumn("[bold]Supported OS[/]");
                    table.AddColumn("[bold]Requires Admin[/]");
                }

                foreach (var category in categories)
                {
                    var localizedName = _localizationService.GetString($"category.{category.ToString().ToLower()}");
                    var description = GetCategoryDescription(category);
                    var supportedOS = GetSupportedOS(category);
                    var requiresAdmin = RequiresAdminPrivileges(category);

                    if (!showAll && !IsCategorySupported(category, _osService.GetOperatingSystem()))
                        continue;

                    var row = new List<string>
                    {
                        category.ToString(),
                        localizedName,
                        description
                    };

                    if (showDetails)
                    {
                        row.Add(supportedOS);
                        row.Add(requiresAdmin ? "[red]Yes[/]" : "[green]No[/]");
                    }

                    table.AddRow(row.ToArray());
                }

                AnsiConsole.Write(table);
                AnsiConsole.WriteLine();

                // Show usage example
                var examplePanel = new Panel(new Markup(
                    "[bold]Usage Examples:[/]\n\n" +
                    "• Clean specific categories:\n" +
                    "  [blue]sweeper clean --categories SystemTemp UserTemp[/]\n\n" +
                    "• Scan all categories:\n" +
                    "  [blue]sweeper scan --categories all[/]\n\n" +
                    "• Estimate space for browser cache:\n" +
                    "  [blue]sweeper estimate --categories BrowserCache[/]"))
                {
                    Header = new PanelHeader("How to Use Categories", Justify.Left),
                    Border = BoxBorder.Rounded,
                    BorderStyle = Style.Parse("dim"),
                    Padding = new Padding(1)
                };

                AnsiConsole.Write(examplePanel);
            }

            _logger.LogInformation("Category listing completed");
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing categories");
            
            if (!quiet)
            {
                AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            }
            
            return 1;
        }
    }

    private string GetCategoryDescription(CleanupCategory category)
    {
        return category switch
        {
            CleanupCategory.SystemTemp => "Temporary files created by the operating system",
            CleanupCategory.UserTemp => "Temporary files created by user applications",
            CleanupCategory.BrowserCache => "Web browser cache and temporary internet files",
            CleanupCategory.ApplicationCache => "Cache files from various applications",
            CleanupCategory.LogFiles => "System and application log files",
            CleanupCategory.Thumbnails => "Thumbnail cache for images and videos",
            CleanupCategory.RecycleBin => "Files in the recycle bin or trash",
            CleanupCategory.DownloadedPrograms => "Downloaded program installers and updates",
            CleanupCategory.WindowsUpdate => "Windows Update cache and old update files",
            CleanupCategory.PackageManagerCache => "Package manager cache (npm, pip, nuget, etc.)",
            CleanupCategory.BuildArtifacts => "Build output and intermediate files",
            _ => "Unknown category"
        };
    }

    private string GetSupportedOS(CleanupCategory category)
    {
        return category switch
        {
            CleanupCategory.WindowsUpdate => "Windows",
            CleanupCategory.RecycleBin => "Windows, Linux, macOS",
            _ => "All"
        };
    }

    private bool RequiresAdminPrivileges(CleanupCategory category)
    {
        return category switch
        {
            CleanupCategory.SystemTemp => true,
            CleanupCategory.WindowsUpdate => true,
            _ => false
        };
    }

    private bool IsCategorySupported(CleanupCategory category, OperatingSystemType os)
    {
        return category switch
        {
            CleanupCategory.WindowsUpdate => os == OperatingSystemType.Windows,
            _ => true
        };
    }
}
