using Sweeper.Domain.Enums;

namespace Sweeper.Domain.Interfaces;

public interface IOperatingSystemService
{
    OperatingSystemType GetOperatingSystem();
    string GetOSVersion();
    string GetOSArchitecture();
    bool IsElevated();
    bool IsWindows();
    bool IsLinux();
    bool IsMacOS();
    bool IsFreeBSD();
    string GetUsername();
    string GetMachineName();
    long GetAvailableDiskSpace(string drive);
    long GetTotalDiskSpace(string drive);
}
