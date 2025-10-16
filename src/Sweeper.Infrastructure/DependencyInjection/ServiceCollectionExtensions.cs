using Microsoft.Extensions.DependencyInjection;
using Sweeper.Domain.Interfaces;
using Sweeper.Infrastructure.Services;

namespace Sweeper.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Register core services
        services.AddSingleton<IOperatingSystemService, OperatingSystemService>();
        services.AddSingleton<IFileSystemService, FileSystemService>();
        services.AddSingleton<ILocalizationService, LocalizationService>();

        return services;
    }
}
