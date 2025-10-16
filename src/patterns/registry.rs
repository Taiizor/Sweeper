use std::collections::HashMap;

use super::category::Category;
use super::pattern::{Pattern, PatternType, Platform, SafetyLevel};

/// Registry containing all built-in cleaning patterns
pub struct Registry {
    patterns: HashMap<String, Pattern>,
}

impl Registry {
    /// Create a new empty registry
    pub fn new() -> Self {
        Self {
            patterns: HashMap::new(),
        }
    }
    
    /// Get the number of patterns in the registry
    pub fn count(&self) -> usize {
        self.patterns.len()
    }
    
    /// Get a pattern by its ID
    pub fn get_pattern(&self, id: &str) -> Option<&Pattern> {
        self.patterns.get(id)
    }
    
    /// Get all patterns in a specific category
    pub fn get_patterns_by_category(&self, category: &Category) -> Vec<&Pattern> {
        self.patterns
            .values()
            .filter(|p| &p.category == category)
            .collect()
    }
    
    /// Register a new pattern
    fn register(&mut self, pattern: Pattern) {
        self.patterns.insert(pattern.id.clone(), pattern);
    }
    
    /// Initialize with default patterns
    fn init_default_patterns(&mut self) {
        // System patterns - Windows
        self.register(
            Pattern::new(
                "win_temp",
                "Windows Temp",
                "Windows temporary files",
                Category::System,
                "%TEMP%\\*",
            )
            .with_type(PatternType::EnvVar)
            .with_platforms(vec![Platform::Windows])
            .with_safety(SafetyLevel::VeryHigh),
        );
        
        self.register(
            Pattern::new(
                "win_prefetch",
                "Windows Prefetch",
                "Windows prefetch files",
                Category::System,
                "C:\\Windows\\Prefetch\\*",
            )
            .with_type(PatternType::Glob)
            .with_platforms(vec![Platform::Windows])
            .with_safety(SafetyLevel::High),
        );
        
        self.register(
            Pattern::new(
                "win_recent",
                "Recent Documents",
                "Windows recent documents list",
                Category::System,
                "%APPDATA%\\Microsoft\\Windows\\Recent\\*",
            )
            .with_type(PatternType::EnvVar)
            .with_platforms(vec![Platform::Windows])
            .with_safety(SafetyLevel::High),
        );
        
        // System patterns - Linux/Unix
        self.register(
            Pattern::new(
                "linux_tmp",
                "Linux Temp",
                "Linux temporary files",
                Category::System,
                "/tmp/*",
            )
            .with_type(PatternType::Glob)
            .with_platforms(vec![Platform::Linux, Platform::Unix])
            .with_safety(SafetyLevel::High),
        );
        
        self.register(
            Pattern::new(
                "linux_cache",
                "Linux Cache",
                "Linux user cache directory",
                Category::System,
                "~/.cache/*",
            )
            .with_type(PatternType::Glob)
            .with_platforms(vec![Platform::Linux])
            .with_safety(SafetyLevel::Medium),
        );
        
        // System patterns - macOS
        self.register(
            Pattern::new(
                "macos_cache",
                "macOS Cache",
                "macOS system caches",
                Category::System,
                "~/Library/Caches/*",
            )
            .with_type(PatternType::Glob)
            .with_platforms(vec![Platform::MacOS])
            .with_safety(SafetyLevel::High),
        );
        
        self.register(
            Pattern::new(
                "macos_logs",
                "macOS Logs",
                "macOS system logs",
                Category::Logs,
                "~/Library/Logs/*",
            )
            .with_type(PatternType::Glob)
            .with_platforms(vec![Platform::MacOS])
            .with_safety(SafetyLevel::High),
        );
        
        // Browser patterns - Chrome
        self.register(
            Pattern::new(
                "chrome_cache_win",
                "Chrome Cache (Windows)",
                "Google Chrome browser cache on Windows",
                Category::Browsers,
                "%LOCALAPPDATA%\\Google\\Chrome\\User Data\\Default\\Cache\\*",
            )
            .with_type(PatternType::EnvVar)
            .with_platforms(vec![Platform::Windows])
            .with_safety(SafetyLevel::High),
        );
        
        self.register(
            Pattern::new(
                "chrome_cache_linux",
                "Chrome Cache (Linux)",
                "Google Chrome browser cache on Linux",
                Category::Browsers,
                "~/.cache/google-chrome/Default/Cache/*",
            )
            .with_type(PatternType::Glob)
            .with_platforms(vec![Platform::Linux])
            .with_safety(SafetyLevel::High),
        );
        
        self.register(
            Pattern::new(
                "chrome_cache_mac",
                "Chrome Cache (macOS)",
                "Google Chrome browser cache on macOS",
                Category::Browsers,
                "~/Library/Caches/Google/Chrome/Default/Cache/*",
            )
            .with_type(PatternType::Glob)
            .with_platforms(vec![Platform::MacOS])
            .with_safety(SafetyLevel::High),
        );
        
        // Browser patterns - Firefox
        self.register(
            Pattern::new(
                "firefox_cache_win",
                "Firefox Cache (Windows)",
                "Mozilla Firefox browser cache on Windows",
                Category::Browsers,
                "%LOCALAPPDATA%\\Mozilla\\Firefox\\Profiles\\*.default*\\cache2\\*",
            )
            .with_type(PatternType::EnvVar)
            .with_platforms(vec![Platform::Windows])
            .with_safety(SafetyLevel::High),
        );
        
        // Development patterns - Node.js
        self.register(
            Pattern::new(
                "node_modules",
                "Node Modules",
                "Node.js dependencies",
                Category::Development,
                "**/node_modules",
            )
            .with_type(PatternType::Glob)
            .with_platforms(vec![Platform::All])
            .with_safety(SafetyLevel::Medium),
        );
        
        self.register(
            Pattern::new(
                "npm_cache",
                "NPM Cache",
                "NPM package cache",
                Category::PackageManagers,
                "~/.npm/_cacache/*",
            )
            .with_type(PatternType::Glob)
            .with_platforms(vec![Platform::All])
            .with_safety(SafetyLevel::High),
        );
        
        // Development patterns - Python
        self.register(
            Pattern::new(
                "python_cache",
                "Python Cache",
                "Python bytecode cache files",
                Category::Development,
                "**/__pycache__",
            )
            .with_type(PatternType::Glob)
            .with_platforms(vec![Platform::All])
            .with_safety(SafetyLevel::VeryHigh),
        );
        
        self.register(
            Pattern::new(
                "pip_cache",
                "Pip Cache",
                "Python pip package cache",
                Category::PackageManagers,
                "~/.cache/pip/*",
            )
            .with_type(PatternType::Glob)
            .with_platforms(vec![Platform::Linux, Platform::MacOS])
            .with_safety(SafetyLevel::High),
        );
        
        // Development patterns - Rust
        self.register(
            Pattern::new(
                "rust_target",
                "Rust Target",
                "Rust build artifacts",
                Category::BuildArtifacts,
                "**/target",
            )
            .with_type(PatternType::Glob)
            .with_platforms(vec![Platform::All])
            .with_safety(SafetyLevel::Medium),
        );
        
        self.register(
            Pattern::new(
                "cargo_cache",
                "Cargo Cache",
                "Rust cargo package cache",
                Category::PackageManagers,
                "~/.cargo/registry/cache/*",
            )
            .with_type(PatternType::Glob)
            .with_platforms(vec![Platform::All])
            .with_safety(SafetyLevel::High),
        );
        
        // Development patterns - Java
        self.register(
            Pattern::new(
                "maven_cache",
                "Maven Cache",
                "Maven dependency cache",
                Category::PackageManagers,
                "~/.m2/repository/*",
            )
            .with_type(PatternType::Glob)
            .with_platforms(vec![Platform::All])
            .with_safety(SafetyLevel::Medium),
        );
        
        self.register(
            Pattern::new(
                "gradle_cache",
                "Gradle Cache",
                "Gradle build cache",
                Category::PackageManagers,
                "~/.gradle/caches/*",
            )
            .with_type(PatternType::Glob)
            .with_platforms(vec![Platform::All])
            .with_safety(SafetyLevel::Medium),
        );
        
        // IDE patterns - VS Code
        self.register(
            Pattern::new(
                "vscode_cache",
                "VS Code Cache",
                "Visual Studio Code cache",
                Category::IDEs,
                "~/.config/Code/Cache/*",
            )
            .with_type(PatternType::Glob)
            .with_platforms(vec![Platform::Linux])
            .with_safety(SafetyLevel::High),
        );
        
        self.register(
            Pattern::new(
                "vscode_cache_win",
                "VS Code Cache (Windows)",
                "Visual Studio Code cache on Windows",
                Category::IDEs,
                "%APPDATA%\\Code\\Cache\\*",
            )
            .with_type(PatternType::EnvVar)
            .with_platforms(vec![Platform::Windows])
            .with_safety(SafetyLevel::High),
        );
        
        // IDE patterns - JetBrains
        self.register(
            Pattern::new(
                "jetbrains_cache",
                "JetBrains Cache",
                "JetBrains IDEs cache",
                Category::IDEs,
                "~/.cache/JetBrains/*",
            )
            .with_type(PatternType::Glob)
            .with_platforms(vec![Platform::Linux])
            .with_safety(SafetyLevel::Medium),
        );
        
        // Application patterns - Discord
        self.register(
            Pattern::new(
                "discord_cache_win",
                "Discord Cache (Windows)",
                "Discord application cache on Windows",
                Category::Applications,
                "%APPDATA%\\discord\\Cache\\*",
            )
            .with_type(PatternType::EnvVar)
            .with_platforms(vec![Platform::Windows])
            .with_safety(SafetyLevel::High),
        );
        
        // Application patterns - Slack
        self.register(
            Pattern::new(
                "slack_cache_win",
                "Slack Cache (Windows)",
                "Slack application cache on Windows",
                Category::Applications,
                "%APPDATA%\\Slack\\Cache\\*",
            )
            .with_type(PatternType::EnvVar)
            .with_platforms(vec![Platform::Windows])
            .with_safety(SafetyLevel::High),
        );
        
        // Thumbnails
        self.register(
            Pattern::new(
                "thumbnails_win",
                "Windows Thumbnails",
                "Windows thumbnail cache",
                Category::Thumbnails,
                "%LOCALAPPDATA%\\Microsoft\\Windows\\Explorer\\thumbcache_*.db",
            )
            .with_type(PatternType::EnvVar)
            .with_platforms(vec![Platform::Windows])
            .with_safety(SafetyLevel::High),
        );
        
        self.register(
            Pattern::new(
                "thumbnails_linux",
                "Linux Thumbnails",
                "Linux thumbnail cache",
                Category::Thumbnails,
                "~/.cache/thumbnails/*",
            )
            .with_type(PatternType::Glob)
            .with_platforms(vec![Platform::Linux])
            .with_safety(SafetyLevel::High),
        );
        
        // Downloads
        self.register(
            Pattern::new(
                "partial_downloads",
                "Partial Downloads",
                "Incomplete download files",
                Category::Downloads,
                "~/Downloads/*.part",
            )
            .with_type(PatternType::Glob)
            .with_platforms(vec![Platform::All])
            .with_safety(SafetyLevel::High),
        );
        
        // Backup files
        self.register(
            Pattern::new(
                "backup_files",
                "Backup Files",
                "Common backup file patterns",
                Category::Backups,
                "**/*.bak",
            )
            .with_type(PatternType::Glob)
            .with_platforms(vec![Platform::All])
            .with_safety(SafetyLevel::Medium),
        );
        
        self.register(
            Pattern::new(
                "vim_swap",
                "Vim Swap Files",
                "Vim editor swap files",
                Category::IDEs,
                "**/.*.swp",
            )
            .with_type(PatternType::Glob)
            .with_platforms(vec![Platform::All])
            .with_safety(SafetyLevel::High),
        );
    }
}

impl Default for Registry {
    fn default() -> Self {
        let mut registry = Self::new();
        registry.init_default_patterns();
        registry
    }
}
