# 🧹 Sweeper

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Rust](https://img.shields.io/badge/rust-%23000000.svg?style=for-the-badge&logo=rust&logoColor=white)](https://www.rust-lang.org/)
[![Cross-Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20Linux%20%7C%20macOS-blue)](https://github.com/Taiizor/Sweeper)

A powerful, cross-platform CLI tool for cleaning temporary files and directories, written in Rust.

## ✨ Features

- 🌍 **Cross-Platform**: Works on Windows, Linux, and macOS
- 🎯 **Smart Detection**: Automatically detects common temporary files and caches
- 🛡️ **Safe Cleaning**: Multiple safety levels and trash/recycle bin support
- 🌐 **Multi-Language**: Supports multiple languages (English, Turkish, German, French, Spanish)
- ⚡ **Fast & Efficient**: Parallel scanning with progress indicators
- 🎨 **Customizable**: Add your own cleaning patterns
- 🔧 **Extensible**: Modular architecture for easy extension

## 📦 Installation

### From Source

```bash
# Clone the repository
git clone https://github.com/Taiizor/Sweeper.git
cd Sweeper

# Build the project
cargo build --release

# Install globally (optional)
cargo install --path .
```

### Using Cargo

```bash
cargo install sweeper
```

## 🚀 Quick Start

### Basic Commands

```bash
# Scan for temporary files
sweeper scan

# Clean temporary files (with confirmation)
sweeper clean

# List available cleaning patterns
sweeper list

# Show help
sweeper --help
```

### Advanced Usage

```bash
# Scan specific directory
sweeper scan --path /path/to/directory

# Clean with specific patterns
sweeper clean --patterns npm-cache,cargo-cache

# Force clean without confirmation
sweeper clean --force

# Use trash instead of permanent deletion
sweeper clean --trash

# Dry run (preview what would be deleted)
sweeper scan --dry-run

# Set language
sweeper --language tr
```

## 🎯 Supported Cleaning Patterns

### System
- Windows: Temp files, Prefetch, Recent documents
- Linux: /tmp, ~/.cache
- macOS: ~/Library/Caches, ~/Library/Logs

### Development
- Node.js: node_modules, npm cache
- Python: __pycache__, pip cache
- Rust: target directories, cargo cache
- Java: Maven cache, Gradle cache

### Browsers
- Chrome cache (all platforms)
- Firefox cache (all platforms)
- Edge cache
- Safari cache

### Applications
- VS Code cache
- JetBrains IDEs cache
- Discord cache
- Slack cache

### Package Managers
- npm, yarn, pnpm
- pip, pipenv, poetry
- cargo, rustup
- maven, gradle
- composer

## ⚙️ Configuration

Sweeper uses a configuration file located at:
- Windows: `%APPDATA%\sweeper\config.toml`
- Linux: `~/.config/sweeper/config.toml`
- macOS: `~/Library/Application Support/sweeper/config.toml`

### Example Configuration

```toml
language = "en"
colored_output = true
use_trash = true
require_confirmation = true

[patterns]
enabled_categories = ["system", "development", "browsers"]
excluded_patterns = []

[scanner]
max_depth = 10
follow_links = false
scan_hidden = true
threads = 8
min_age_days = 7

[logging]
level = "info"
timestamps = true
```

### Managing Configuration

```bash
# Show current configuration
sweeper config show

# Edit a configuration value
sweeper config edit language tr

# Reset to defaults
sweeper config reset

# Export configuration
sweeper config export --output my-config.toml

# Import configuration
sweeper config import --input my-config.toml
```

## 🎨 Custom Patterns

Create custom cleaning patterns in TOML or JSON format:

```toml
# ~/.config/sweeper/patterns/my-patterns.toml
[[patterns]]
id = "my-app-cache"
name = "My App Cache"
description = "Cache files for my application"
category = "applications"
pattern_type = "glob"
pattern = "~/.myapp/cache/*"
platforms = ["All"]
enabled_by_default = true
safety = "High"
```

## 🌐 Internationalization

Sweeper supports multiple languages:
- 🇬🇧 English (en) - Default
- 🇹🇷 Turkish (tr)
- 🇩🇪 German (de)
- 🇫🇷 French (fr)
- 🇪🇸 Spanish (es)

To change the language:
```bash
sweeper --language tr
# or
sweeper config edit language tr
```

## 🏗️ Architecture

Sweeper follows a modular architecture:

```
src/
├── cli/            # Command-line interface
├── config/         # Configuration management
├── core/           # Core business logic
├── i18n/           # Internationalization
├── patterns/       # Cleaning patterns & registry
├── scanner/        # File scanning engine
└── utils/          # Utility functions
```

## 🧪 Development

### Prerequisites
- Rust 1.70 or higher
- Cargo

### Building
```bash
# Debug build
cargo build

# Release build
cargo build --release

# Run tests
cargo test

# Run benchmarks
cargo bench

# Check code
cargo clippy

# Format code
cargo fmt
```

### Adding New Patterns

1. Edit `src/patterns/registry.rs`
2. Add your pattern to `init_default_patterns()`
3. Test thoroughly on target platforms

### Adding New Languages

1. Create a new `.ftl` file in `locales/`
2. Add the language to `src/i18n/mod.rs`
3. Translate all strings from `locales/en.ftl`

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## ⚠️ Safety Notice

Sweeper is designed with safety in mind:
- **Always** performs a scan before cleaning
- Uses trash/recycle bin by default
- Requires confirmation for destructive operations
- Never deletes system-critical files
- Respects file age settings (default: 7 days)

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Built with [Rust](https://www.rust-lang.org/)
- CLI powered by [clap](https://github.com/clap-rs/clap)
- Cross-platform paths by [dirs](https://github.com/dirs-dev/dirs-rs)
- Safe deletion via [trash](https://github.com/Byron/trash-rs)

## 📧 Contact

- GitHub: [@Taiizor](https://github.com/Taiizor)
- Repository: [https://github.com/Taiizor/Sweeper](https://github.com/Taiizor/Sweeper)

---

**⚡ Powered by Rust** | **🧹 Keep your system clean!**
