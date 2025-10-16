using Sweeper.Domain.Entities;

namespace Sweeper.Domain.Interfaces;

public interface IFileSystemService
{
    bool DirectoryExists(string path);
    bool FileExists(string path);
    long GetFileSize(string path);
    DateTime GetFileCreationTime(string path);
    DateTime GetFileLastWriteTime(string path);
    DateTime GetFileLastAccessTime(string path);
    FileAttributes GetFileAttributes(string path);
    IEnumerable<string> GetFiles(string path, string searchPattern, SearchOption searchOption);
    IEnumerable<string> GetDirectories(string path, string searchPattern, SearchOption searchOption);
    Task<bool> DeleteFileAsync(string path);
    Task<bool> DeleteDirectoryAsync(string path, bool recursive);
    Task<bool> MoveToRecycleBinAsync(string path);
    bool HasWritePermission(string path);
    string ExpandEnvironmentVariables(string path);
    string GetTempPath();
    string GetUserProfilePath();
    string GetProgramDataPath();
    string GetSystemDirectory();
    Task<List<FileItem>> ScanDirectoryAsync(string path, List<string> patterns, List<string> exclusions, CancellationToken cancellationToken = default);
}
