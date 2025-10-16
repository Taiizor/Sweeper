# Sweeper 🧹

A powerful, professional CLI tool for cleaning temporary files across different operating systems. Sweeper helps you reclaim disk space by safely removing unnecessary temporary files, caches, and build artifacts.

## ✨ Features

- **Cross-Platform Support**: Works on Windows, Linux, and macOS
- **Smart Detection**: Automatically finds temporary files from various sources
- **Safe Cleaning**: Protected files are preserved, with dry-run mode for preview
- **Multi-Language Support**: Available in multiple languages (English, Turkish, and more)
- **Parallel Processing**: Fast cleaning with concurrent operations
- **Customizable**: Flexible configuration and exclusion patterns
- **Beautiful UI**: Color-coded output with progress bars
- **Statistics**: Track your cleaning history and saved space

## 🚀 Quick Start

### Installation

```bash
# Clone the repository
git clone https://github.com/Taiizor/Sweeper.git
cd sweeper

# Build the application
go build -o sweeper

# Or install directly
go install github.com/Taiizor/Sweeper@latest
```

### Basic Usage

```bash
# Clean all default temporary files
sweeper clean

# Preview what would be deleted (dry-run)
sweeper clean --dry-run

# Clean specific targets
sweeper clean browser npm

# List files without deleting
sweeper list

# Show available targets
sweeper targets

# Display statistics
sweeper stats
```

## 🎯 Available Targets

| Target | Description |
|--------|-------------|
| `all` | Clean all available targets |
| `system` | System temporary files and caches |
| `browser` | Web browser cache (Chrome, Firefox, Edge, etc.) |
| `dev` | Development build artifacts |
| `npm` | Node.js/npm cache and node_modules |
| `pip` | Python/pip cache and virtual environments |
| `cargo` | Rust/Cargo build artifacts |
| `maven` | Maven build artifacts |
| `gradle` | Gradle build artifacts |
| `ide` | IDE cache (VS Code, IntelliJ, etc.) |
| `logs` | Application and system logs |
| `cache` | Various application caches |
| `docker` | Docker images and build cache |

## 🛠️ Configuration

Sweeper can be configured via command-line flags or a configuration file:

### Command-Line Flags

```bash
# Global flags
--config string    Config file (default: $HOME/.sweeper.yaml)
--verbose         Verbose output
--dry-run         Preview without deleting
--force           Force deletion without confirmation
--lang string     Language (en, tr, es, etc.)

# Clean command flags
--targets strings  Specific targets to clean
--exclude strings  Patterns to exclude
--max-size string  Maximum file size (e.g., 100MB)
--min-age int     Minimum age in days
--recursive       Recursively clean subdirectories
```

### Configuration File

Create a `.sweeper.yaml` file in your home directory:

```yaml
# Language settings
language: en

# Logging
log_level: info
log_file: ~/.local/share/sweeper/sweeper.log

# Cleaning settings
default_targets:
  - browser
  - system
auto_confirm: false
keep_recent_days: 7
safe_mode: true

# Exclusions
exclude_patterns:
  - "*.important"
  - "keep-*"

# Display
color_output: true
progress_bar: true
output_format: table
```

## 🏗️ Architecture

Sweeper is built with a modular, extensible architecture:

```
sweeper/
├── cmd/              # CLI commands
│   ├── root.go      # Root command
│   ├── clean.go     # Clean command
│   ├── list.go      # List command
│   └── stats.go     # Statistics command
├── internal/
│   ├── cleaner/     # File deletion logic
│   ├── scanner/     # File scanning logic
│   ├── patterns/    # Cleaning patterns
│   ├── models/      # Data models
│   ├── config/      # Configuration
│   ├── logger/      # Logging
│   ├── i18n/        # Internationalization
│   └── ui/          # User interface
└── main.go          # Entry point
```

## 🔧 Development

### Prerequisites

- Go 1.21 or higher
- Git

### Building from Source

```bash
# Clone the repository
git clone https://github.com/Taiizor/Sweeper.git
cd sweeper

# Download dependencies
go mod download

# Build the application
go build -o sweeper

# Run tests
go test ./...

# Run with race detector
go run -race main.go clean --dry-run
```

### Adding New Cleaning Patterns

To add support for new applications or file types:

1. Add patterns to the appropriate file in `internal/patterns/`
2. Update the pattern manager in `internal/patterns/manager.go`
3. Add translations to locale files in `internal/i18n/locales/`

Example pattern:

```go
{
    Name:        "my_app_cache",
    Category:    "cache",
    Description: "My Application cache files",
    OS:          []string{"windows", "linux"},
    Paths: []string{
        "~/.myapp/cache",
        "%APPDATA%\\MyApp\\Cache",
    },
    Extensions: []string{".tmp", ".cache"},
    Priority:   2,
}
```

## 🌐 Internationalization

Sweeper supports multiple languages. To add a new language:

1. Create a new JSON file in `internal/i18n/locales/`
2. Translate all message keys
3. Update the `loadLocales()` function in `internal/i18n/i18n.go`

## 📊 Performance

Sweeper is designed for performance:

- **Parallel Scanning**: Multiple workers scan directories concurrently
- **Efficient Deletion**: Batch operations with optimized I/O
- **Memory Efficient**: Streaming processing for large file sets
- **Progress Tracking**: Real-time feedback without performance impact

## 🔒 Safety Features

- **Dry-Run Mode**: Preview changes before execution
- **Protected Paths**: System-critical files are protected
- **Confirmation Prompts**: Requires user confirmation for destructive operations
- **Safe Mode**: Conservative cleaning by default
- **Exclusion Patterns**: Fine-grained control over what gets cleaned

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- [Cobra](https://github.com/spf13/cobra) - CLI framework
- [Viper](https://github.com/spf13/viper) - Configuration management
- [go-i18n](https://github.com/nicksnyder/go-i18n) - Internationalization
- [Zap](https://github.com/uber-go/zap) - Logging
- [Color](https://github.com/fatih/color) - Colorized output
- [Progressbar](https://github.com/schollz/progressbar) - Progress bars

## 📧 Contact

For questions and support, please open an issue on GitHub.

---

**⚠️ Disclaimer**: Always use the `--dry-run` flag first to preview what will be deleted. While Sweeper includes safety features, deleted files cannot be recovered.
