using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Sweeper.Application.Services;
using Sweeper.Application.Strategies.Windows;
using Sweeper.CLI.Commands;
using Sweeper.CLI.Configuration;
using Sweeper.Domain.Interfaces;
using Sweeper.Infrastructure.DependencyInjection;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;

namespace Sweeper.CLI;

class Program
{
    static async Task<int> Main(string[] args)
    {
        // Configure Serilog
        ConfigureLogging();

        try
        {
            // Create root command
            var rootCommand = new RootCommand("Sweeper - A professional CLI tool for cleaning temporary files");
            
            // Configure the command line builder with dependency injection
            var parser = new CommandLineBuilder(rootCommand)
                .UseHost(_ => Host.CreateDefaultBuilder(args), builder =>
                {
                    builder.ConfigureAppConfiguration((context, config) =>
                    {
                        config.SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                            .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true)
                            .AddEnvironmentVariables(prefix: "SWEEPER_")
                            .AddCommandLine(args);
                    })
                    .ConfigureServices((context, services) =>
                    {
                        ConfigureServices(services, context.Configuration);
                    })
                    .UseSerilog()
                    .UseCommandHandler<RootCommandHandler>(rootCommand);
                })
                .UseDefaults()
                .UseExceptionHandler((exception, context) =>
                {
                    var logger = context.BindingContext.GetService<ILogger<Program>>();
                    logger?.LogError(exception, "An unhandled exception occurred");
                    
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine($"Error: {exception.Message}");
                    
                    if (context.ParseResult.CommandResult.Command.Name != "root" && 
                        context.ParseResult.Tokens.Any(t => t.Value == "--verbose" || t.Value == "-v"))
                    {
                        Console.Error.WriteLine(exception.StackTrace);
                    }
                    
                    Console.ResetColor();
                })
                .Build();

            // Build commands
            CommandBuilder.BuildCommands(rootCommand);

            // Parse and invoke
            return await parser.InvokeAsync(args);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"Fatal error: {ex.Message}");
            Console.ResetColor();
            return -1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    private static void ConfigureLogging()
    {
        var logDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Sweeper",
            "logs");

        Directory.CreateDirectory(logDirectory);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("System", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithThreadId()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                restrictedToMinimumLevel: LogEventLevel.Information)
            .WriteTo.File(
                Path.Combine(logDirectory, "sweeper-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] ({ThreadId}) {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Add configuration
        services.AddSingleton(configuration);
        services.Configure<SweeperOptions>(configuration.GetSection("Sweeper"));

        // Add logging
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog();
        });

        // Add infrastructure services
        services.AddInfrastructureServices();

        // Add application services
        services.AddScoped<ICleanupService, CleanupService>();

        // Add cleanup strategies
        services.AddScoped<ICleanupStrategy, WindowsTempFilesStrategy>();
        // Add more strategies as they are implemented

        // Add command handlers
        services.AddTransient<RootCommandHandler>();
        services.AddTransient<CleanCommandHandler>();
        services.AddTransient<ScanCommandHandler>();
        services.AddTransient<ListCommandHandler>();
        services.AddTransient<EstimateCommandHandler>();
        services.AddTransient<ConfigCommandHandler>();
    }
}
