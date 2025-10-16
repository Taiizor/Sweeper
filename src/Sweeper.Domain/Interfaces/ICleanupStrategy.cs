using Sweeper.Domain.Entities;
using Sweeper.Domain.Enums;

namespace Sweeper.Domain.Interfaces;

public interface ICleanupStrategy
{
    OperatingSystemType SupportedOS { get; }
    CleanupCategory Category { get; }
    string Name { get; }
    string Description { get; }
    int Priority { get; }
    bool RequiresElevation { get; }
    
    Task<List<CleanupTarget>> GetTargetsAsync();
    Task<ScanResult> ScanAsync(CleanupTarget target, CancellationToken cancellationToken = default);
    Task<CleanupResult> CleanAsync(ScanResult scanResult, CancellationToken cancellationToken = default);
    Task<bool> ValidateAsync(CleanupTarget target);
}
