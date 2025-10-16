using System.CommandLine;
using Sweeper.Domain.Enums;

namespace Sweeper.CLI.Commands;

public static class CommandBuilder
{
    public static void BuildCommands(RootCommand rootCommand)
    {
        // Add global options
        var verboseOption = new Option<bool>(
            aliases: new[] { "--verbose", "-v" },
            description: "Enable verbose output");

        var quietOption = new Option<bool>(
            aliases: new[] { "--quiet", "-q" },
            description: "Suppress non-error output");

        var languageOption = new Option<string>(
            aliases: new[] { "--language", "-l" },
            getDefaultValue: () => "en-US",
            description: "Set display language (e.g., en-US, tr-TR)");

        rootCommand.AddGlobalOption(verboseOption);
        rootCommand.AddGlobalOption(quietOption);
        rootCommand.AddGlobalOption(languageOption);

        // Add commands
        rootCommand.AddCommand(BuildCleanCommand());
        rootCommand.AddCommand(BuildScanCommand());
        rootCommand.AddCommand(BuildListCommand());
        rootCommand.AddCommand(BuildEstimateCommand());
        rootCommand.AddCommand(BuildConfigCommand());
    }

    private static Command BuildCleanCommand()
    {
        var command = new Command("clean", "Clean temporary files from your system");

        var categoriesOption = new Option<string[]>(
            aliases: new[] { "--categories", "-c" },
            description: "Specify cleanup categories to clean")
        {
            AllowMultipleArgumentsPerToken = true
        };

        var forceOption = new Option<bool>(
            aliases: new[] { "--force", "-f" },
            description: "Force cleanup without confirmation");

        var dryRunOption = new Option<bool>(
            aliases: new[] { "--dry-run", "-d" },
            description: "Perform a dry run without deleting files");

        var excludeOption = new Option<string[]>(
            aliases: new[] { "--exclude", "-e" },
            description: "Exclude specific paths or patterns")
        {
            AllowMultipleArgumentsPerToken = true
        };

        var includeOption = new Option<string[]>(
            aliases: new[] { "--include", "-i" },
            description: "Include specific paths or patterns")
        {
            AllowMultipleArgumentsPerToken = true
        };

        var recycleBinOption = new Option<bool>(
            aliases: new[] { "--recycle-bin", "-r" },
            getDefaultValue: () => true,
            description: "Move files to recycle bin instead of permanent deletion");

        var minAgeOption = new Option<int>(
            aliases: new[] { "--min-age", "-a" },
            getDefaultValue: () => 0,
            description: "Minimum file age in days");

        var minSizeOption = new Option<long>(
            aliases: new[] { "--min-size", "-s" },
            getDefaultValue: () => 0,
            description: "Minimum file size in bytes");

        command.AddOption(categoriesOption);
        command.AddOption(forceOption);
        command.AddOption(dryRunOption);
        command.AddOption(excludeOption);
        command.AddOption(includeOption);
        command.AddOption(recycleBinOption);
        command.AddOption(minAgeOption);
        command.AddOption(minSizeOption);

        command.SetHandler<CleanCommandHandler>();

        return command;
    }

    private static Command BuildScanCommand()
    {
        var command = new Command("scan", "Scan for temporary files without deleting them");

        var categoriesOption = new Option<string[]>(
            aliases: new[] { "--categories", "-c" },
            description: "Specify cleanup categories to scan")
        {
            AllowMultipleArgumentsPerToken = true
        };

        var detailsOption = new Option<bool>(
            aliases: new[] { "--details", "-d" },
            description: "Show detailed file information");

        var sortOption = new Option<string>(
            aliases: new[] { "--sort", "-s" },
            getDefaultValue: () => "size",
            description: "Sort results by: size, date, name");

        var limitOption = new Option<int>(
            aliases: new[] { "--limit", "-n" },
            getDefaultValue: () => 0,
            description: "Limit number of results (0 for all)");

        command.AddOption(categoriesOption);
        command.AddOption(detailsOption);
        command.AddOption(sortOption);
        command.AddOption(limitOption);

        command.SetHandler<ScanCommandHandler>();

        return command;
    }

    private static Command BuildListCommand()
    {
        var command = new Command("list", "List available cleanup categories");

        var allOption = new Option<bool>(
            aliases: new[] { "--all", "-a" },
            description: "Show all categories including disabled ones");

        var detailsOption = new Option<bool>(
            aliases: new[] { "--details", "-d" },
            description: "Show detailed information about each category");

        command.AddOption(allOption);
        command.AddOption(detailsOption);

        command.SetHandler<ListCommandHandler>();

        return command;
    }

    private static Command BuildEstimateCommand()
    {
        var command = new Command("estimate", "Estimate space that can be freed");

        var categoriesOption = new Option<string[]>(
            aliases: new[] { "--categories", "-c" },
            description: "Specify cleanup categories to estimate")
        {
            AllowMultipleArgumentsPerToken = true
        };

        var detailsOption = new Option<bool>(
            aliases: new[] { "--details", "-d" },
            description: "Show breakdown by category");

        command.AddOption(categoriesOption);
        command.AddOption(detailsOption);

        command.SetHandler<EstimateCommandHandler>();

        return command;
    }

    private static Command BuildConfigCommand()
    {
        var command = new Command("config", "Configure Sweeper settings");

        var getOption = new Option<string>(
            aliases: new[] { "--get", "-g" },
            description: "Get a configuration value");

        var setOption = new Option<string[]>(
            aliases: new[] { "--set", "-s" },
            description: "Set a configuration value (key=value)")
        {
            AllowMultipleArgumentsPerToken = true
        };

        var listOption = new Option<bool>(
            aliases: new[] { "--list", "-l" },
            description: "List all configuration values");

        var resetOption = new Option<bool>(
            aliases: new[] { "--reset", "-r" },
            description: "Reset configuration to defaults");

        command.AddOption(getOption);
        command.AddOption(setOption);
        command.AddOption(listOption);
        command.AddOption(resetOption);

        command.SetHandler<ConfigCommandHandler>();

        return command;
    }
}
