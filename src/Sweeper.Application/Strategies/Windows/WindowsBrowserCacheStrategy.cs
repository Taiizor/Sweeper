using Microsoft.Extensions.Logging;
using Sweeper.Application.Strategies.Base;
using Sweeper.Domain.Entities;
using Sweeper.Domain.Enums;
using Sweeper.Domain.Interfaces;

namespace Sweeper.Application.Strategies.Windows;

public class WindowsBrowserCacheStrategy : BaseCleanupStrategy
{
    public override OperatingSystemType SupportedOS => OperatingSystemType.Windows;
    public override CleanupCategory Category => CleanupCategory.BrowserCache;
    public override string Name => "Windows Browser Cache";
    public override string Description => "Clean browser cache and temporary internet files";
    public override int Priority => 90;

    public WindowsBrowserCacheStrategy(
        ILogger<WindowsBrowserCacheStrategy> logger,
        IFileSystemService fileSystemService,
        IOperatingSystemService osService)
        : base(logger, fileSystemService, osService)
    {
    }

    public override async Task<List<CleanupTarget>> GetTargetsAsync()
    {
        var targets = new List<CleanupTarget>();

        // Chrome
        targets.Add(new CleanupTarget
        {
            Name = "Google Chrome Cache",
            Description = "Chrome browser cache and temporary files",
            Category = CleanupCategory.BrowserCache,
            OperatingSystem = OperatingSystemType.Windows,
            Paths = new List<string> 
            { 
                @"%LOCALAPPDATA%\Google\Chrome\User Data\Default\Cache",
                @"%LOCALAPPDATA%\Google\Chrome\User Data\Default\Code Cache",
                @"%LOCALAPPDATA%\Google\Chrome\User Data\Default\GPUCache",
                @"%LOCALAPPDATA%\Google\Chrome\User Data\Default\Media Cache",
                @"%LOCALAPPDATA%\Google\Chrome\User Data\Default\Service Worker\CacheStorage",
                @"%LOCALAPPDATA%\Google\Chrome\User Data\Default\Service Worker\ScriptCache"
            },
            Patterns = new List<string> { "*" },
            Exclusions = new List<string>(),
            RequiresElevation = false,
            Priority = 100
        });

        // Firefox
        targets.Add(new CleanupTarget
        {
            Name = "Mozilla Firefox Cache",
            Description = "Firefox browser cache and temporary files",
            Category = CleanupCategory.BrowserCache,
            OperatingSystem = OperatingSystemType.Windows,
            Paths = new List<string> 
            { 
                @"%LOCALAPPDATA%\Mozilla\Firefox\Profiles\*.default-release\cache2",
                @"%LOCALAPPDATA%\Mozilla\Firefox\Profiles\*.default\cache2",
                @"%APPDATA%\Mozilla\Firefox\Profiles\*.default-release\cache2",
                @"%APPDATA%\Mozilla\Firefox\Profiles\*.default\cache2"
            },
            Patterns = new List<string> { "*" },
            Exclusions = new List<string>(),
            RequiresElevation = false,
            Priority = 100
        });

        // Edge (Chromium)
        targets.Add(new CleanupTarget
        {
            Name = "Microsoft Edge Cache",
            Description = "Edge browser cache and temporary files",
            Category = CleanupCategory.BrowserCache,
            OperatingSystem = OperatingSystemType.Windows,
            Paths = new List<string> 
            { 
                @"%LOCALAPPDATA%\Microsoft\Edge\User Data\Default\Cache",
                @"%LOCALAPPDATA%\Microsoft\Edge\User Data\Default\Code Cache",
                @"%LOCALAPPDATA%\Microsoft\Edge\User Data\Default\GPUCache",
                @"%LOCALAPPDATA%\Microsoft\Edge\User Data\Default\Media Cache",
                @"%LOCALAPPDATA%\Microsoft\Edge\User Data\Default\Service Worker\CacheStorage",
                @"%LOCALAPPDATA%\Microsoft\Edge\User Data\Default\Service Worker\ScriptCache"
            },
            Patterns = new List<string> { "*" },
            Exclusions = new List<string>(),
            RequiresElevation = false,
            Priority = 100
        });

        // Internet Explorer
        targets.Add(new CleanupTarget
        {
            Name = "Internet Explorer Cache",
            Description = "Internet Explorer cache and temporary internet files",
            Category = CleanupCategory.BrowserCache,
            OperatingSystem = OperatingSystemType.Windows,
            Paths = new List<string> 
            { 
                @"%LOCALAPPDATA%\Microsoft\Windows\INetCache",
                @"%LOCALAPPDATA%\Microsoft\Windows\WebCache",
                @"%USERPROFILE%\AppData\Local\Microsoft\Windows\Temporary Internet Files"
            },
            Patterns = new List<string> { "*" },
            Exclusions = new List<string> { "*.dat", "*.log" },
            RequiresElevation = false,
            Priority = 80
        });

        // Opera
        targets.Add(new CleanupTarget
        {
            Name = "Opera Browser Cache",
            Description = "Opera browser cache and temporary files",
            Category = CleanupCategory.BrowserCache,
            OperatingSystem = OperatingSystemType.Windows,
            Paths = new List<string> 
            { 
                @"%APPDATA%\Opera Software\Opera Stable\Cache",
                @"%APPDATA%\Opera Software\Opera Stable\Code Cache",
                @"%APPDATA%\Opera Software\Opera Stable\GPUCache",
                @"%APPDATA%\Opera Software\Opera Stable\Media Cache"
            },
            Patterns = new List<string> { "*" },
            Exclusions = new List<string>(),
            RequiresElevation = false,
            Priority = 70
        });

        // Brave
        targets.Add(new CleanupTarget
        {
            Name = "Brave Browser Cache",
            Description = "Brave browser cache and temporary files",
            Category = CleanupCategory.BrowserCache,
            OperatingSystem = OperatingSystemType.Windows,
            Paths = new List<string> 
            { 
                @"%LOCALAPPDATA%\BraveSoftware\Brave-Browser\User Data\Default\Cache",
                @"%LOCALAPPDATA%\BraveSoftware\Brave-Browser\User Data\Default\Code Cache",
                @"%LOCALAPPDATA%\BraveSoftware\Brave-Browser\User Data\Default\GPUCache"
            },
            Patterns = new List<string> { "*" },
            Exclusions = new List<string>(),
            RequiresElevation = false,
            Priority = 70
        });

        return targets;
    }
}
