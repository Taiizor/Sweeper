# 🧹 Sweeper

A professional, cross-platform CLI tool for cleaning temporary files and reclaiming disk space on your system. Built with .NET 9 and designed with extensibility, performance, and safety in mind.

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey.svg)

## ✨ Features

- **Cross-Platform Support**: Works on Windows, Linux, and macOS
- **Smart Cleanup**: Intelligently identifies and removes temporary files
- **Safe Operations**: Option to use Recycle Bin instead of permanent deletion
- **Multiple Categories**: Clean system temp, user temp, browser cache, and more
- **Dry Run Mode**: Preview what will be deleted without actually removing files
- **Multi-Language Support**: Available in multiple languages (English default)
- **Beautiful CLI**: Modern, colorful interface with progress indicators
- **Configurable**: Extensive configuration options for customization
- **Extensible Architecture**: Easy to add new cleanup strategies

## 🚀 Quick Start

### Installation

#### From Source

```bash
# Clone the repository
git clone https://github.com/yourusername/sweeper.git
cd sweeper

# Build the project
dotnet build -c Release

# Run the tool
dotnet run --project src/Sweeper.CLI
```

#### Global Tool Installation

```bash
# Package as global tool
dotnet pack src/Sweeper.CLI -c Release

# Install globally
dotnet tool install --global --add-source ./src/Sweeper.CLI/nupkg sweeper
```

### Basic Usage

```bash
# Show help
sweeper --help

# Scan for temporary files
sweeper scan

# Clean temporary files (with confirmation)
sweeper clean

# Clean specific categories
sweeper clean --categories SystemTemp UserTemp BrowserCache

# Dry run (preview without deleting)
sweeper clean --dry-run

# Force cleanup without confirmation
sweeper clean --force

# Estimate space to be freed
sweeper estimate
```

## 📚 Commands

### `clean`
Clean temporary files from your system.

```bash
sweeper clean [options]

Options:
  -c, --categories     Specify cleanup categories
  -f, --force          Force cleanup without confirmation
  -d, --dry-run        Perform a dry run without deleting files
  -e, --exclude        Exclude specific paths or patterns
  -i, --include        Include specific paths or patterns
  -r, --recycle-bin    Move to recycle bin (default: true)
  -a, --min-age        Minimum file age in days
  -s, --min-size       Minimum file size in bytes
```

### `scan`
Scan for temporary files without deleting them.

```bash
sweeper scan [options]

Options:
  -c, --categories     Specify cleanup categories to scan
  -d, --details        Show detailed file information
  -s, --sort           Sort results by: size, date, name
  -n, --limit          Limit number of results (0 for all)
```

### `list`
List available cleanup categories.

```bash
sweeper list [options]

Options:
  -a, --all            Show all categories including disabled ones
  -d, --details        Show detailed information about each category
```

### `estimate`
Estimate space that can be freed.

```bash
sweeper estimate [options]

Options:
  -c, --categories     Specify cleanup categories to estimate
  -d, --details        Show breakdown by category
```

### `config`
Configure Sweeper settings.

```bash
sweeper config [options]

Options:
  -g, --get            Get a configuration value
  -s, --set            Set a configuration value (key=value)
  -l, --list           List all configuration values
  -r, --reset          Reset configuration to defaults
```

## 🗂️ Cleanup Categories

| Category | Description | Admin Required |
|----------|-------------|----------------|
| `SystemTemp` | System temporary files | Yes |
| `UserTemp` | User temporary files | No |
| `BrowserCache` | Web browser cache files | No |
| `ApplicationCache` | Application cache files | No |
| `LogFiles` | System and application logs | No |
| `Thumbnails` | Thumbnail cache | No |
| `RecycleBin` | Recycle bin/trash | No |
| `DownloadedPrograms` | Downloaded installers | No |
| `WindowsUpdate` | Windows Update cache | Yes |
| `PackageManagerCache` | NPM, NuGet, pip cache | No |
| `BuildArtifacts` | Build output files | No |

## ⚙️ Configuration

Configuration file location:
- Windows: `%LOCALAPPDATA%\Sweeper\config.json`
- Linux/macOS: `~/.local/share/Sweeper/config.json`

### Configuration Options

```json
{
  "Language": "en-US",
  "Verbose": false,
  "Quiet": false,
  "Force": false,
  "DryRun": false,
  "Recursive": true,
  "ParallelOperations": 4,
  "AutoConfirm": false,
  "ShowProgress": true,
  "UseRecycleBin": true,
  "MinimumFileAge": 0,
  "MinimumFileSize": 0,
  "LogLevel": "Information"
}
```

### Setting Configuration

```bash
# Set language to Turkish
sweeper config --set Language=tr-TR

# Enable verbose mode
sweeper config --set Verbose=true

# Set multiple values
sweeper config --set AutoConfirm=true ShowProgress=false
```

## 🌍 Supported Languages

- English (en-US) - Default
- Turkish (tr-TR)
- German (de-DE) - Coming soon
- French (fr-FR) - Coming soon
- Spanish (es-ES) - Coming soon
- Japanese (ja-JP) - Coming soon
- Chinese (zh-CN) - Coming soon
- Russian (ru-RU) - Coming soon

## 🏗️ Architecture

Sweeper follows Clean Architecture principles with the following layers:

```
Sweeper/
├── src/
│   ├── Sweeper.Domain/         # Core business logic
│   ├── Sweeper.Application/    # Application services
│   ├── Sweeper.Infrastructure/ # External services
│   └── Sweeper.CLI/           # CLI interface
└── tests/
    ├── Sweeper.Domain.Tests/
    ├── Sweeper.Application.Tests/
    └── Sweeper.Infrastructure.Tests/
```

### Key Design Patterns

- **Strategy Pattern**: Different cleanup strategies for various file types
- **Repository Pattern**: Abstraction over file system operations
- **Dependency Injection**: Loose coupling and testability
- **Command Pattern**: CLI command handling

## 🔧 Development

### Prerequisites

- .NET 9 SDK
- Visual Studio 2022 / VS Code / Rider

### Building

```bash
# Build solution
dotnet build

# Run tests
dotnet test

# Run with watch mode
dotnet watch run --project src/Sweeper.CLI
```

### Adding New Cleanup Strategies

1. Create a new strategy class inheriting from `BaseCleanupStrategy`
2. Implement required properties and methods
3. Register the strategy in DI container
4. Test thoroughly

Example:
```csharp
public class MyCustomStrategy : BaseCleanupStrategy
{
    public override OperatingSystemType SupportedOS => OperatingSystemType.Windows;
    public override CleanupCategory Category => CleanupCategory.ApplicationCache;
    public override string Name => "My Custom Cleanup";
    
    public override async Task<List<CleanupTarget>> GetTargetsAsync()
    {
        // Implementation
    }
}
```

## 📝 Logging

Logs are stored in:
- Windows: `%LOCALAPPDATA%\Sweeper\logs\`
- Linux/macOS: `~/.local/share/Sweeper/logs/`

Log levels: `Trace`, `Debug`, `Information`, `Warning`, `Error`, `Fatal`

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Built with [Spectre.Console](https://spectreconsole.net/) for beautiful CLI output
- Uses [System.CommandLine](https://github.com/dotnet/command-line-api) for command parsing
- Logging with [Serilog](https://serilog.net/)

## ⚠️ Disclaimer

Always review files before deletion. While Sweeper includes safety features like dry-run mode and recycle bin support, deleted files may be unrecoverable. Use at your own risk.

## 📞 Support

- Create an [Issue](https://github.com/yourusername/sweeper/issues) for bug reports
- Start a [Discussion](https://github.com/yourusername/sweeper/discussions) for questions
- Check the [Wiki](https://github.com/yourusername/sweeper/wiki) for detailed documentation

---

Made with ❤️ using .NET 9
