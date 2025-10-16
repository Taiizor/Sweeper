namespace Sweeper.Domain.Entities;

public class ScanResult
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public CleanupTarget Target { get; set; } = null!;
    public List<FileItem> Files { get; set; } = new();
    public long TotalSize { get; set; }
    public int TotalFiles { get; set; }
    public DateTime ScannedAt { get; set; }
    public TimeSpan ScanDuration { get; set; }
    public bool HasErrors { get; set; }
    public List<string> Errors { get; set; } = new();

    public ScanResult()
    {
        ScannedAt = DateTime.UtcNow;
    }

    public ScanResult(CleanupTarget target) : this()
    {
        Target = target;
    }

    public void AddFile(FileItem file)
    {
        Files.Add(file);
        TotalSize += file.Size;
        TotalFiles++;
    }

    public void AddError(string error)
    {
        Errors.Add(error);
        HasErrors = true;
    }
}

public class FileItem
{
    public string Path { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public DateTime LastAccessedAt { get; set; }
    public bool IsReadOnly { get; set; }
    public bool IsSystemFile { get; set; }
    public bool IsHidden { get; set; }
    public string? Error { get; set; }

    public FileItem()
    {
    }

    public FileItem(string path, long size)
    {
        Path = path;
        Size = size;
    }
}
