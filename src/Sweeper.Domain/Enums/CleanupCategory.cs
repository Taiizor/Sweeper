namespace Sweeper.Domain.Enums;

[Flags]
public enum CleanupCategory
{
    None = 0,
    SystemTemp = 1 << 0,
    UserTemp = 1 << 1,
    BrowserCache = 1 << 2,
    ApplicationCache = 1 << 3,
    LogFiles = 1 << 4,
    Thumbnails = 1 << 5,
    RecycleBin = 1 << 6,
    DownloadedPrograms = 1 << 7,
    WindowsUpdate = 1 << 8,
    PackageManagerCache = 1 << 9,
    BuildArtifacts = 1 << 10,
    All = ~None
}
