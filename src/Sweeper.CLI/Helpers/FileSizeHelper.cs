namespace Sweeper.CLI.Helpers;

public static class FileSizeHelper
{
    private static readonly string[] SizeSuffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };

    public static string FormatSize(long bytes)
    {
        if (bytes < 0)
            return "-" + FormatSize(-bytes);
        
        if (bytes == 0)
            return "0 B";

        int magnitude = (int)Math.Log(bytes, 1024);
        decimal adjustedSize = (decimal)bytes / (1L << (magnitude * 10));

        if (Math.Round(adjustedSize, 2) >= 1000)
        {
            magnitude += 1;
            adjustedSize /= 1024;
        }

        return string.Format("{0:n2} {1}", adjustedSize, SizeSuffixes[magnitude]);
    }

    public static string FormatSizeWithColor(long bytes)
    {
        var formattedSize = FormatSize(bytes);
        
        if (bytes > 1_073_741_824) // > 1 GB
            return $"[red]{formattedSize}[/]";
        else if (bytes > 104_857_600) // > 100 MB
            return $"[yellow]{formattedSize}[/]";
        else if (bytes > 10_485_760) // > 10 MB
            return $"[green]{formattedSize}[/]";
        else
            return $"[dim]{formattedSize}[/]";
    }

    public static long ParseSize(string sizeString)
    {
        if (string.IsNullOrWhiteSpace(sizeString))
            return 0;

        sizeString = sizeString.Trim().ToUpperInvariant();

        // Extract number and unit
        var match = System.Text.RegularExpressions.Regex.Match(sizeString, @"^([\d.]+)\s*([KMGTPE]?B?)$");
        
        if (!match.Success)
        {
            if (long.TryParse(sizeString, out var bytesOnly))
                return bytesOnly;
            
            throw new ArgumentException($"Invalid size format: {sizeString}");
        }

        if (!double.TryParse(match.Groups[1].Value, out var value))
            throw new ArgumentException($"Invalid numeric value: {match.Groups[1].Value}");

        var unit = match.Groups[2].Value;
        
        long multiplier = unit switch
        {
            "KB" or "K" => 1024,
            "MB" or "M" => 1024 * 1024,
            "GB" or "G" => 1024L * 1024 * 1024,
            "TB" or "T" => 1024L * 1024 * 1024 * 1024,
            "PB" or "P" => 1024L * 1024 * 1024 * 1024 * 1024,
            "EB" or "E" => 1024L * 1024 * 1024 * 1024 * 1024 * 1024,
            _ => 1
        };

        return (long)(value * multiplier);
    }
}
