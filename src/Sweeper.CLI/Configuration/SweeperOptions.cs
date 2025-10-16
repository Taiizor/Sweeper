namespace Sweeper.CLI.Configuration;

public class SweeperOptions
{
    public string Language { get; set; } = "en-US";
    public bool Verbose { get; set; } = false;
    public bool Quiet { get; set; } = false;
    public bool Force { get; set; } = false;
    public bool DryRun { get; set; } = false;
    public bool Recursive { get; set; } = true;
    public int ParallelOperations { get; set; } = Environment.ProcessorCount;
    public List<string> DefaultCategories { get; set; } = new();
    public List<string> ExcludePaths { get; set; } = new();
    public List<string> IncludePaths { get; set; } = new();
    public bool AutoConfirm { get; set; } = false;
    public bool ShowProgress { get; set; } = true;
    public bool UseRecycleBin { get; set; } = true;
    public long MinimumFileAge { get; set; } = 0; // in days
    public long MinimumFileSize { get; set; } = 0; // in bytes
    public string LogLevel { get; set; } = "Information";
    public string LogPath { get; set; } = "";
    public bool EnableTelemetry { get; set; } = false;
}
