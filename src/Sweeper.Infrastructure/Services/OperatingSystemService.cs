using Microsoft.Extensions.Logging;
using Sweeper.Domain.Enums;
using Sweeper.Domain.Interfaces;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Sweeper.Infrastructure.Services;

public class OperatingSystemService : IOperatingSystemService
{
    private readonly ILogger<OperatingSystemService> _logger;
    private readonly OperatingSystemType _osType;
    private readonly PlatformID _platformId;
    private readonly Version _version;

    public OperatingSystemService(ILogger<OperatingSystemService> logger)
    {
        _logger = logger;
        _platformId = Environment.OSVersion.Platform;
        _version = Environment.OSVersion.Version;
        _osType = DetectOperatingSystem();
    }

    public OperatingSystemType GetOperatingSystem()
    {
        return _osType;
    }

    public string GetOSVersion()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Windows version detection
            var major = _version.Major;
            var minor = _version.Minor;
            var build = _version.Build;

            if (major == 10 && build >= 22000)
                return $"Windows 11 ({_version})";
            else if (major == 10)
                return $"Windows 10 ({_version})";
            else if (major == 6 && minor == 3)
                return $"Windows 8.1 ({_version})";
            else if (major == 6 && minor == 2)
                return $"Windows 8 ({_version})";
            else if (major == 6 && minor == 1)
                return $"Windows 7 ({_version})";
            else
                return $"Windows {_version}";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return $"Linux {RuntimeInformation.OSDescription}";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return $"macOS {RuntimeInformation.OSDescription}";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
        {
            return $"FreeBSD {RuntimeInformation.OSDescription}";
        }

        return RuntimeInformation.OSDescription;
    }

    public string GetOSArchitecture()
    {
        return RuntimeInformation.OSArchitecture.ToString();
    }

    public bool IsElevated()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        else
        {
            // Unix-like systems: check if running as root (UID 0)
            return Environment.UserName == "root" || GetUserId() == 0;
        }
    }

    public bool IsWindows()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }

    public bool IsLinux()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    }

    public bool IsMacOS()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    }

    public bool IsFreeBSD()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD);
    }

    public string GetUsername()
    {
        return Environment.UserName;
    }

    public string GetMachineName()
    {
        return Environment.MachineName;
    }

    public long GetAvailableDiskSpace(string drive)
    {
        try
        {
            DriveInfo driveInfo;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // On Windows, ensure drive letter format (e.g., "C:\")
                if (!drive.EndsWith(":\\") && !drive.EndsWith(":/"))
                {
                    drive = drive.TrimEnd('\\', '/') + ":\\";
                }
                driveInfo = new DriveInfo(drive);
            }
            else
            {
                // On Unix-like systems, use root path if no specific path provided
                drive = string.IsNullOrEmpty(drive) ? "/" : drive;
                driveInfo = new DriveInfo(drive);
            }

            if (driveInfo.IsReady)
            {
                return driveInfo.AvailableFreeSpace;
            }

            _logger.LogWarning("Drive not ready: {Drive}", drive);
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available disk space for drive: {Drive}", drive);
            return 0;
        }
    }

    public long GetTotalDiskSpace(string drive)
    {
        try
        {
            DriveInfo driveInfo;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // On Windows, ensure drive letter format (e.g., "C:\")
                if (!drive.EndsWith(":\\") && !drive.EndsWith(":/"))
                {
                    drive = drive.TrimEnd('\\', '/') + ":\\";
                }
                driveInfo = new DriveInfo(drive);
            }
            else
            {
                // On Unix-like systems, use root path if no specific path provided
                drive = string.IsNullOrEmpty(drive) ? "/" : drive;
                driveInfo = new DriveInfo(drive);
            }

            if (driveInfo.IsReady)
            {
                return driveInfo.TotalSize;
            }

            _logger.LogWarning("Drive not ready: {Drive}", drive);
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total disk space for drive: {Drive}", drive);
            return 0;
        }
    }

    private OperatingSystemType DetectOperatingSystem()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return OperatingSystemType.Windows;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // Check if it's Android
            if (IsAndroid())
            {
                return OperatingSystemType.Android;
            }
            return OperatingSystemType.Linux;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return OperatingSystemType.MacOS;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
        {
            return OperatingSystemType.FreeBSD;
        }

        return OperatingSystemType.Unknown;
    }

    private bool IsAndroid()
    {
        try
        {
            // Check for Android-specific environment variables or paths
            var androidRoot = Environment.GetEnvironmentVariable("ANDROID_ROOT");
            if (!string.IsNullOrEmpty(androidRoot))
                return true;

            // Check for Android-specific directories
            if (Directory.Exists("/system/app") && Directory.Exists("/system/priv-app"))
                return true;

            // Check runtime description for Android
            var osDescription = RuntimeInformation.OSDescription.ToLower();
            return osDescription.Contains("android");
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error checking if platform is Android");
            return false;
        }
    }

    private int GetUserId()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return -1;

        try
        {
            // Try to get UID from environment
            var sudoUid = Environment.GetEnvironmentVariable("SUDO_UID");
            if (!string.IsNullOrEmpty(sudoUid) && int.TryParse(sudoUid, out var uid))
                return uid;

            // For Unix-like systems, we'd need to P/Invoke to get the actual UID
            // This is a simplified approach
            return Environment.UserName == "root" ? 0 : 1000;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error getting user ID");
            return -1;
        }
    }
}
