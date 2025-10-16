using Microsoft.Extensions.Logging;
using Sweeper.Application.Strategies.Base;
using Sweeper.Domain.Entities;
using Sweeper.Domain.Enums;
using Sweeper.Domain.Interfaces;

namespace Sweeper.Application.Strategies.Windows;

public class WindowsTempFilesStrategy : BaseCleanupStrategy
{
    public override OperatingSystemType SupportedOS => OperatingSystemType.Windows;
    public override CleanupCategory Category => CleanupCategory.SystemTemp | CleanupCategory.UserTemp;
    public override string Name => "Windows Temporary Files";
    public override string Description => "Clean Windows system and user temporary files";
    public override int Priority => 100;

    public WindowsTempFilesStrategy(
        ILogger<WindowsTempFilesStrategy> logger,
        IFileSystemService fileSystemService,
        IOperatingSystemService osService)
        : base(logger, fileSystemService, osService)
    {
    }

    public override async Task<List<CleanupTarget>> GetTargetsAsync()
    {
        var targets = new List<CleanupTarget>();

        // System Temp
        targets.Add(new CleanupTarget
        {
            Name = "System Temporary Files",
            Description = "Windows system temporary files",
            Category = CleanupCategory.SystemTemp,
            OperatingSystem = OperatingSystemType.Windows,
            Paths = new List<string> 
            { 
                @"%WINDIR%\Temp",
                @"%WINDIR%\Prefetch",
                @"%SYSTEMROOT%\Temp"
            },
            Patterns = new List<string> { "*" },
            Exclusions = new List<string> 
            { 
                "*.sys", 
                "*.dll",
                "*msi*.log"
            },
            RequiresElevation = true,
            Priority = 100
        });

        // User Temp
        targets.Add(new CleanupTarget
        {
            Name = "User Temporary Files",
            Description = "User temporary files",
            Category = CleanupCategory.UserTemp,
            OperatingSystem = OperatingSystemType.Windows,
            Paths = new List<string> 
            { 
                @"%TEMP%",
                @"%TMP%",
                @"%LOCALAPPDATA%\Temp",
                @"%USERPROFILE%\AppData\Local\Temp"
            },
            Patterns = new List<string> { "*" },
            Exclusions = new List<string> 
            { 
                "*.sys", 
                "*.dll",
                "*msi*.log",
                "FXSAPIDebugLogFile.txt"
            },
            RequiresElevation = false,
            Priority = 100
        });

        // Recent Documents
        targets.Add(new CleanupTarget
        {
            Name = "Recent Documents",
            Description = "Recently accessed documents shortcuts",
            Category = CleanupCategory.UserTemp,
            OperatingSystem = OperatingSystemType.Windows,
            Paths = new List<string> 
            { 
                @"%USERPROFILE%\Recent",
                @"%APPDATA%\Microsoft\Windows\Recent"
            },
            Patterns = new List<string> { "*.lnk" },
            Exclusions = new List<string>(),
            RequiresElevation = false,
            Priority = 50
        });

        return targets;
    }
}
