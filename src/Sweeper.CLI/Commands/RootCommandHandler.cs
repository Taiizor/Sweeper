using Microsoft.Extensions.Logging;
using Spectre.Console;
using Sweeper.Domain.Interfaces;
using System.CommandLine.Invocation;
using System.Reflection;

namespace Sweeper.CLI.Commands;

public class RootCommandHandler : ICommandHandler
{
    private readonly ILogger<RootCommandHandler> _logger;
    private readonly ILocalizationService _localizationService;

    public RootCommandHandler(
        ILogger<RootCommandHandler> logger,
        ILocalizationService localizationService)
    {
        _logger = logger;
        _localizationService = localizationService;
    }

    public int Invoke(InvocationContext context)
    {
        return InvokeAsync(context).GetAwaiter().GetResult();
    }

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
        
        AnsiConsole.Write(new FigletText("SWEEPER")
            .LeftJustified()
            .Color(Color.Blue));
        
        AnsiConsole.WriteLine();
        
        var panel = new Panel(new Markup(
            $"[bold blue]Sweeper[/] v{version}\n" +
            $"[dim]{_localizationService.GetString("app.description")}[/]\n\n" +
            $"[yellow]Usage:[/] sweeper [green]<command>[/] [blue][options][/]\n\n" +
            $"[yellow]Commands:[/]\n" +
            $"  [green]clean[/]     {_localizationService.GetString("command.clean")}\n" +
            $"  [green]scan[/]      {_localizationService.GetString("command.scan")}\n" +
            $"  [green]list[/]      {_localizationService.GetString("command.list")}\n" +
            $"  [green]estimate[/]  {_localizationService.GetString("command.estimate")}\n" +
            $"  [green]config[/]    {_localizationService.GetString("command.config")}\n\n" +
            $"[yellow]Options:[/]\n" +
            $"  [blue]--help, -h[/]     {_localizationService.GetString("option.help")}\n" +
            $"  [blue]--verbose, -v[/]  {_localizationService.GetString("option.verbose")}\n" +
            $"  [blue]--quiet, -q[/]    {_localizationService.GetString("option.quiet")}\n" +
            $"  [blue]--language, -l[/] {_localizationService.GetString("option.language")}\n\n" +
            $"[dim]Run 'sweeper [green]<command>[/] [blue]--help[/]' for more information on a command.[/]"))
        {
            Header = new PanelHeader("Welcome to Sweeper", Justify.Center),
            Padding = new Padding(2, 1),
            BorderStyle = Style.Parse("blue")
        };
        
        AnsiConsole.Write(panel);
        
        return 0;
    }
}
