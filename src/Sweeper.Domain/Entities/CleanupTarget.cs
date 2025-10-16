using Sweeper.Domain.Enums;

namespace Sweeper.Domain.Entities;

public class CleanupTarget
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public CleanupCategory Category { get; set; }
    public OperatingSystemType OperatingSystem { get; set; }
    public List<string> Paths { get; set; } = new();
    public List<string> Patterns { get; set; } = new();
    public List<string> Exclusions { get; set; } = new();
    public bool RequiresElevation { get; set; }
    public long EstimatedSize { get; set; }
    public DateTime LastScanned { get; set; }
    public bool IsEnabled { get; set; } = true;
    public int Priority { get; set; } = 0;

    public CleanupTarget()
    {
    }

    public CleanupTarget(string name, CleanupCategory category, OperatingSystemType os)
    {
        Name = name;
        Category = category;
        OperatingSystem = os;
    }
}
