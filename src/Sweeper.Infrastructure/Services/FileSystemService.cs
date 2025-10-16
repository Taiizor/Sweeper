using Microsoft.Extensions.Logging;
using Sweeper.Domain.Entities;
using Sweeper.Domain.Interfaces;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.RegularExpressions;

namespace Sweeper.Infrastructure.Services;

public class FileSystemService : IFileSystemService
{
    private readonly ILogger<FileSystemService> _logger;

    public FileSystemService(ILogger<FileSystemService> logger)
    {
        _logger = logger;
    }

    public bool DirectoryExists(string path)
    {
        try
        {
            return Directory.Exists(path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking directory existence: {Path}", path);
            return false;
        }
    }

    public bool FileExists(string path)
    {
        try
        {
            return File.Exists(path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file existence: {Path}", path);
            return false;
        }
    }

    public long GetFileSize(string path)
    {
        try
        {
            var info = new FileInfo(path);
            return info.Exists ? info.Length : 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file size: {Path}", path);
            return 0;
        }
    }

    public DateTime GetFileCreationTime(string path)
    {
        try
        {
            return File.GetCreationTime(path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file creation time: {Path}", path);
            return DateTime.MinValue;
        }
    }

    public DateTime GetFileLastWriteTime(string path)
    {
        try
        {
            return File.GetLastWriteTime(path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file last write time: {Path}", path);
            return DateTime.MinValue;
        }
    }

    public DateTime GetFileLastAccessTime(string path)
    {
        try
        {
            return File.GetLastAccessTime(path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file last access time: {Path}", path);
            return DateTime.MinValue;
        }
    }

    public FileAttributes GetFileAttributes(string path)
    {
        try
        {
            return File.GetAttributes(path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file attributes: {Path}", path);
            return FileAttributes.Normal;
        }
    }

    public IEnumerable<string> GetFiles(string path, string searchPattern, SearchOption searchOption)
    {
        try
        {
            if (!DirectoryExists(path))
                return Enumerable.Empty<string>();

            return Directory.EnumerateFiles(path, searchPattern, searchOption);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Access denied to path: {Path}", path);
            return Enumerable.Empty<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enumerating files: {Path}", path);
            return Enumerable.Empty<string>();
        }
    }

    public IEnumerable<string> GetDirectories(string path, string searchPattern, SearchOption searchOption)
    {
        try
        {
            if (!DirectoryExists(path))
                return Enumerable.Empty<string>();

            return Directory.EnumerateDirectories(path, searchPattern, searchOption);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Access denied to path: {Path}", path);
            return Enumerable.Empty<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enumerating directories: {Path}", path);
            return Enumerable.Empty<string>();
        }
    }

    public async Task<bool> DeleteFileAsync(string path)
    {
        try
        {
            if (!FileExists(path))
                return true;

            // Try to remove read-only attribute if present
            var attributes = File.GetAttributes(path);
            if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            {
                File.SetAttributes(path, attributes & ~FileAttributes.ReadOnly);
            }

            await Task.Run(() => File.Delete(path));
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {Path}", path);
            return false;
        }
    }

    public async Task<bool> DeleteDirectoryAsync(string path, bool recursive)
    {
        try
        {
            if (!DirectoryExists(path))
                return true;

            await Task.Run(() => Directory.Delete(path, recursive));
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting directory: {Path}", path);
            return false;
        }
    }

    public async Task<bool> MoveToRecycleBinAsync(string path)
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows-specific implementation using Shell32
                await Task.Run(() =>
                {
                    Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(path,
                        Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                        Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                });
                return true;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Move to .Trash folder on Linux
                var trashPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local/share/Trash/files");
                if (!Directory.Exists(trashPath))
                    Directory.CreateDirectory(trashPath);

                var fileName = Path.GetFileName(path);
                var destPath = Path.Combine(trashPath, fileName);
                
                // Handle duplicate names
                int counter = 1;
                while (File.Exists(destPath) || Directory.Exists(destPath))
                {
                    destPath = Path.Combine(trashPath, $"{Path.GetFileNameWithoutExtension(fileName)}_{counter}{Path.GetExtension(fileName)}");
                    counter++;
                }

                if (FileExists(path))
                    File.Move(path, destPath);
                else if (DirectoryExists(path))
                    Directory.Move(path, destPath);
                    
                return true;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // Move to Trash on macOS
                var trashPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".Trash");
                var fileName = Path.GetFileName(path);
                var destPath = Path.Combine(trashPath, fileName);
                
                // Handle duplicate names
                int counter = 1;
                while (File.Exists(destPath) || Directory.Exists(destPath))
                {
                    destPath = Path.Combine(trashPath, $"{Path.GetFileNameWithoutExtension(fileName)}_{counter}{Path.GetExtension(fileName)}");
                    counter++;
                }

                if (FileExists(path))
                    File.Move(path, destPath);
                else if (DirectoryExists(path))
                    Directory.Move(path, destPath);
                    
                return true;
            }
            else
            {
                // Fallback to permanent deletion
                return await DeleteFileAsync(path) || await DeleteDirectoryAsync(path, true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving to recycle bin: {Path}", path);
            return false;
        }
    }

    public bool HasWritePermission(string path)
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);

                if (Directory.Exists(path))
                {
                    var dirInfo = new DirectoryInfo(path);
                    var dirSecurity = dirInfo.GetAccessControl();
                    var rules = dirSecurity.GetAccessRules(true, true, typeof(SecurityIdentifier));

                    foreach (FileSystemAccessRule rule in rules)
                    {
                        if (identity.Groups?.Contains(rule.IdentityReference) == true || 
                            rule.IdentityReference.Value == identity.User?.Value)
                        {
                            if ((rule.FileSystemRights & FileSystemRights.Write) == FileSystemRights.Write)
                            {
                                if (rule.AccessControlType == AccessControlType.Allow)
                                    return true;
                                if (rule.AccessControlType == AccessControlType.Deny)
                                    return false;
                            }
                        }
                    }
                }
            }
            else
            {
                // Unix-like systems: try to create a temporary file
                if (Directory.Exists(path))
                {
                    var testFile = Path.Combine(path, $".sweeper_test_{Guid.NewGuid()}");
                    try
                    {
                        File.Create(testFile).Dispose();
                        File.Delete(testFile);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking write permission: {Path}", path);
            return false;
        }
    }

    public string ExpandEnvironmentVariables(string path)
    {
        return Environment.ExpandEnvironmentVariables(path);
    }

    public string GetTempPath()
    {
        return Path.GetTempPath();
    }

    public string GetUserProfilePath()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }

    public string GetProgramDataPath()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
    }

    public string GetSystemDirectory()
    {
        return Environment.SystemDirectory;
    }

    public async Task<List<FileItem>> ScanDirectoryAsync(string path, List<string> patterns, List<string> exclusions, CancellationToken cancellationToken = default)
    {
        var files = new List<FileItem>();

        if (!DirectoryExists(path) && !FileExists(path))
        {
            _logger.LogWarning("Path does not exist: {Path}", path);
            return files;
        }

        try
        {
            // If it's a file, process it directly
            if (FileExists(path))
            {
                var fileItem = CreateFileItem(path);
                if (fileItem != null && ShouldIncludeFile(path, patterns, exclusions))
                {
                    files.Add(fileItem);
                }
                return files;
            }

            // Process directory
            var searchPatterns = patterns?.Count > 0 ? patterns : new List<string> { "*" };

            foreach (var pattern in searchPatterns)
            {
                await Task.Run(() =>
                {
                    var matchedFiles = Directory.EnumerateFiles(path, pattern, SearchOption.AllDirectories);
                    
                    foreach (var file in matchedFiles)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (ShouldIncludeFile(file, patterns, exclusions))
                        {
                            var fileItem = CreateFileItem(file);
                            if (fileItem != null)
                            {
                                files.Add(fileItem);
                            }
                        }
                    }
                }, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Directory scan cancelled: {Path}", path);
            throw;
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Access denied scanning directory: {Path}", path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scanning directory: {Path}", path);
        }

        return files;
    }

    private FileItem? CreateFileItem(string path)
    {
        try
        {
            var info = new FileInfo(path);
            if (!info.Exists)
                return null;

            return new FileItem
            {
                Path = path,
                Size = info.Length,
                CreatedAt = info.CreationTimeUtc,
                ModifiedAt = info.LastWriteTimeUtc,
                LastAccessedAt = info.LastAccessTimeUtc,
                IsReadOnly = info.IsReadOnly,
                IsSystemFile = (info.Attributes & FileAttributes.System) == FileAttributes.System,
                IsHidden = (info.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating file item: {Path}", path);
            return new FileItem
            {
                Path = path,
                Error = ex.Message
            };
        }
    }

    private bool ShouldIncludeFile(string filePath, List<string>? patterns, List<string>? exclusions)
    {
        var fileName = Path.GetFileName(filePath);

        // Check exclusions first
        if (exclusions != null)
        {
            foreach (var exclusion in exclusions)
            {
                if (MatchesPattern(fileName, exclusion) || MatchesPattern(filePath, exclusion))
                    return false;
            }
        }

        // If no patterns specified, include all
        if (patterns == null || patterns.Count == 0)
            return true;

        // Check if file matches any pattern
        foreach (var pattern in patterns)
        {
            if (MatchesPattern(fileName, pattern))
                return true;
        }

        return false;
    }

    private bool MatchesPattern(string input, string pattern)
    {
        // Simple wildcard matching
        var regexPattern = Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".");
        return Regex.IsMatch(input, $"^{regexPattern}$", RegexOptions.IgnoreCase);
    }
}
