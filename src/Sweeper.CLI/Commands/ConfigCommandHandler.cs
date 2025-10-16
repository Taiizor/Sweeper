using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Spectre.Console;
using Sweeper.CLI.Configuration;
using Sweeper.Domain.Interfaces;
using System.CommandLine.Invocation;
using System.Text.Json;

namespace Sweeper.CLI.Commands;

public class ConfigCommandHandler : ICommandHandler
{
    private readonly ILogger<ConfigCommandHandler> _logger;
    private readonly IConfiguration _configuration;
    private readonly IOptions<SweeperOptions> _options;
    private readonly ILocalizationService _localizationService;
    private readonly string _configFilePath;

    public ConfigCommandHandler(
        ILogger<ConfigCommandHandler> logger,
        IConfiguration configuration,
        IOptions<SweeperOptions> options,
        ILocalizationService localizationService)
    {
        _logger = logger;
        _configuration = configuration;
        _options = options;
        _localizationService = localizationService;
        _configFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Sweeper",
            "config.json");
    }

    public int Invoke(InvocationContext context)
    {
        return InvokeAsync(context).GetAwaiter().GetResult();
    }

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        var getKey = context.ParseResult.GetValueForOption<string>("--get");
        var setValues = context.ParseResult.GetValueForOption<string[]>("--set") ?? Array.Empty<string>();
        var list = context.ParseResult.GetValueForOption<bool>("--list");
        var reset = context.ParseResult.GetValueForOption<bool>("--reset");
        var quiet = context.ParseResult.GetValueForOption<bool>("--quiet");

        try
        {
            if (!string.IsNullOrEmpty(getKey))
            {
                return GetConfigValue(getKey, quiet);
            }
            else if (setValues.Length > 0)
            {
                return await SetConfigValues(setValues, quiet);
            }
            else if (list)
            {
                return ListConfigValues(quiet);
            }
            else if (reset)
            {
                return await ResetConfig(quiet);
            }
            else
            {
                if (!quiet)
                {
                    ShowConfigHelp();
                }
                return 0;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in config command");
            
            if (!quiet)
            {
                AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            }
            
            return 1;
        }
    }

    private int GetConfigValue(string key, bool quiet)
    {
        var value = _configuration[key];
        
        if (value == null)
        {
            if (!quiet)
            {
                AnsiConsole.MarkupLine($"[yellow]Configuration key '{key}' not found[/]");
            }
            return 1;
        }

        if (!quiet)
        {
            AnsiConsole.MarkupLine($"[blue]{key}[/] = [green]{value}[/]");
        }
        else
        {
            Console.WriteLine(value);
        }

        return 0;
    }

    private async Task<int> SetConfigValues(string[] values, bool quiet)
    {
        var config = await LoadOrCreateConfig();
        var hasChanges = false;

        foreach (var value in values)
        {
            var parts = value.Split('=', 2);
            if (parts.Length != 2)
            {
                if (!quiet)
                {
                    AnsiConsole.MarkupLine($"[red]Invalid format: {value}. Expected key=value[/]");
                }
                continue;
            }

            var key = parts[0].Trim();
            var val = parts[1].Trim();

            // Validate and set the value
            if (SetConfigValue(config, key, val))
            {
                hasChanges = true;
                if (!quiet)
                {
                    AnsiConsole.MarkupLine($"[green]✓[/] Set [blue]{key}[/] = [green]{val}[/]");
                }
            }
            else if (!quiet)
            {
                AnsiConsole.MarkupLine($"[red]Failed to set {key}[/]");
            }
        }

        if (hasChanges)
        {
            await SaveConfig(config);
            if (!quiet)
            {
                AnsiConsole.MarkupLine("[green]Configuration saved successfully[/]");
            }
        }

        return 0;
    }

    private int ListConfigValues(bool quiet)
    {
        if (!quiet)
        {
            AnsiConsole.Write(new Rule("[blue]Current Configuration[/]"));
            AnsiConsole.WriteLine();

            var table = new Table();
            table.Border(TableBorder.Rounded);
            table.AddColumn("[bold]Key[/]");
            table.AddColumn("[bold]Value[/]");
            table.AddColumn("[bold]Description[/]");

            AddConfigRow(table, "Language", _options.Value.Language, "Display language");
            AddConfigRow(table, "Verbose", _options.Value.Verbose.ToString(), "Enable verbose output");
            AddConfigRow(table, "Quiet", _options.Value.Quiet.ToString(), "Suppress non-error output");
            AddConfigRow(table, "Force", _options.Value.Force.ToString(), "Force operations without confirmation");
            AddConfigRow(table, "DryRun", _options.Value.DryRun.ToString(), "Perform dry run without actual changes");
            AddConfigRow(table, "Recursive", _options.Value.Recursive.ToString(), "Process directories recursively");
            AddConfigRow(table, "ParallelOperations", _options.Value.ParallelOperations.ToString(), "Number of parallel operations");
            AddConfigRow(table, "AutoConfirm", _options.Value.AutoConfirm.ToString(), "Auto-confirm operations");
            AddConfigRow(table, "ShowProgress", _options.Value.ShowProgress.ToString(), "Show progress indicators");
            AddConfigRow(table, "UseRecycleBin", _options.Value.UseRecycleBin.ToString(), "Use recycle bin instead of permanent deletion");
            AddConfigRow(table, "MinimumFileAge", _options.Value.MinimumFileAge.ToString(), "Minimum file age in days");
            AddConfigRow(table, "MinimumFileSize", _options.Value.MinimumFileSize.ToString(), "Minimum file size in bytes");
            AddConfigRow(table, "LogLevel", _options.Value.LogLevel, "Logging level");
            AddConfigRow(table, "EnableTelemetry", _options.Value.EnableTelemetry.ToString(), "Enable anonymous telemetry");

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();

            AnsiConsole.MarkupLine($"[dim]Configuration file: {_configFilePath}[/]");
        }

        return 0;
    }

    private async Task<int> ResetConfig(bool quiet)
    {
        if (!quiet)
        {
            if (!AnsiConsole.Confirm("Are you sure you want to reset all configuration to defaults?", false))
            {
                AnsiConsole.MarkupLine("[yellow]Reset cancelled[/]");
                return 0;
            }
        }

        try
        {
            if (File.Exists(_configFilePath))
            {
                File.Delete(_configFilePath);
            }

            if (!quiet)
            {
                AnsiConsole.MarkupLine("[green]✓ Configuration reset to defaults[/]");
            }

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting configuration");
            if (!quiet)
            {
                AnsiConsole.MarkupLine($"[red]Error resetting configuration: {ex.Message}[/]");
            }
            return 1;
        }
    }

    private void ShowConfigHelp()
    {
        var panel = new Panel(new Markup(
            "[bold]Configuration Management[/]\n\n" +
            "[yellow]Commands:[/]\n" +
            "  [blue]--get[/] <key>         Get a configuration value\n" +
            "  [blue]--set[/] key=value     Set a configuration value\n" +
            "  [blue]--list[/]              List all configuration values\n" +
            "  [blue]--reset[/]             Reset configuration to defaults\n\n" +
            "[yellow]Examples:[/]\n" +
            "  sweeper config [blue]--get[/] Language\n" +
            "  sweeper config [blue]--set[/] Language=tr-TR\n" +
            "  sweeper config [blue]--set[/] Verbose=true AutoConfirm=false\n" +
            "  sweeper config [blue]--list[/]"))
        {
            Header = new PanelHeader("Config Command Help", Justify.Center),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("blue"),
            Padding = new Padding(1)
        };

        AnsiConsole.Write(panel);
    }

    private void AddConfigRow(Table table, string key, string value, string description)
    {
        table.AddRow($"[yellow]{key}[/]", $"[green]{value}[/]", $"[dim]{description}[/]");
    }

    private async Task<Dictionary<string, object>> LoadOrCreateConfig()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_configFilePath)!);

            if (File.Exists(_configFilePath))
            {
                var json = await File.ReadAllTextAsync(_configFilePath);
                return JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading configuration");
        }

        return new Dictionary<string, object>();
    }

    private async Task SaveConfig(Dictionary<string, object> config)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_configFilePath)!);
            
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            
            var json = JsonSerializer.Serialize(config, options);
            await File.WriteAllTextAsync(_configFilePath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving configuration");
            throw;
        }
    }

    private bool SetConfigValue(Dictionary<string, object> config, string key, string value)
    {
        try
        {
            // Parse boolean values
            if (bool.TryParse(value, out var boolValue))
            {
                config[key] = boolValue;
                return true;
            }

            // Parse integer values
            if (int.TryParse(value, out var intValue))
            {
                config[key] = intValue;
                return true;
            }

            // Parse long values
            if (long.TryParse(value, out var longValue))
            {
                config[key] = longValue;
                return true;
            }

            // Default to string
            config[key] = value;
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting config value {Key}={Value}", key, value);
            return false;
        }
    }
}
