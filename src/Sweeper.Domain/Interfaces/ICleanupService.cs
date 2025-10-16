using Sweeper.Domain.Entities;
using Sweeper.Domain.Enums;

namespace Sweeper.Domain.Interfaces;

public interface ICleanupService
{
    Task<ScanResult> ScanAsync(CleanupTarget target, CancellationToken cancellationToken = default);
    Task<ScanResult> ScanAsync(CleanupCategory categories, CancellationToken cancellationToken = default);
    Task<List<ScanResult>> ScanAllAsync(CancellationToken cancellationToken = default);
    Task<CleanupResult> CleanAsync(ScanResult scanResult, CancellationToken cancellationToken = default);
    Task<CleanupResult> CleanAsync(CleanupCategory categories, CancellationToken cancellationToken = default);
    Task<long> EstimateSizeAsync(CleanupCategory categories, CancellationToken cancellationToken = default);
    Task<bool> ValidatePermissionsAsync(CleanupTarget target);
}
