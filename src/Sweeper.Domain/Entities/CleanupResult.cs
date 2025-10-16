namespace Sweeper.Domain.Entities;

public class CleanupResult
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public ScanResult ScanResult { get; set; } = null!;
    public List<CleanedFile> CleanedFiles { get; set; } = new();
    public List<FailedFile> FailedFiles { get; set; } = new();
    public long TotalFreedSpace { get; set; }
    public int TotalFilesDeleted { get; set; }
    public int TotalFilesFailed { get; set; }
    public DateTime CleanedAt { get; set; }
    public TimeSpan CleanupDuration { get; set; }
    public bool IsCompleted { get; set; }
    public bool HasErrors => FailedFiles.Count > 0;

    public CleanupResult()
    {
        CleanedAt = DateTime.UtcNow;
    }

    public CleanupResult(ScanResult scanResult) : this()
    {
        ScanResult = scanResult;
    }

    public void AddCleanedFile(string path, long size)
    {
        CleanedFiles.Add(new CleanedFile(path, size));
        TotalFreedSpace += size;
        TotalFilesDeleted++;
    }

    public void AddFailedFile(string path, long size, string error)
    {
        FailedFiles.Add(new FailedFile(path, size, error));
        TotalFilesFailed++;
    }
}

public class CleanedFile
{
    public string Path { get; set; }
    public long Size { get; set; }
    public DateTime DeletedAt { get; set; }

    public CleanedFile(string path, long size)
    {
        Path = path;
        Size = size;
        DeletedAt = DateTime.UtcNow;
    }
}

public class FailedFile
{
    public string Path { get; set; }
    public long Size { get; set; }
    public string Error { get; set; }
    public DateTime FailedAt { get; set; }

    public FailedFile(string path, long size, string error)
    {
        Path = path;
        Size = size;
        Error = error;
        FailedAt = DateTime.UtcNow;
    }
}
