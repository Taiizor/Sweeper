use anyhow::{Context, Result};
use serde::{Deserialize, Serialize};
use std::path::PathBuf;

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct Config {
    /// Default language for the CLI
    #[serde(default = "default_language")]
    pub language: String,
    
    /// Whether to use colored output
    #[serde(default = "default_colored_output")]
    pub colored_output: bool,
    
    /// Default trash behavior
    #[serde(default = "default_use_trash")]
    pub use_trash: bool,
    
    /// Require confirmation for destructive operations
    #[serde(default = "default_require_confirmation")]
    pub require_confirmation: bool,
    
    /// Patterns configuration
    #[serde(default)]
    pub patterns: PatternsConfig,
    
    /// Scanner configuration
    #[serde(default)]
    pub scanner: ScannerConfig,
    
    /// Logging configuration
    #[serde(default)]
    pub logging: LoggingConfig,
}

#[derive(Debug, Clone, Serialize, Deserialize, Default)]
pub struct PatternsConfig {
    /// Custom patterns directory
    pub custom_patterns_dir: Option<PathBuf>,
    
    /// Enabled pattern categories
    #[serde(default = "default_enabled_categories")]
    pub enabled_categories: Vec<String>,
    
    /// Excluded patterns
    #[serde(default)]
    pub excluded_patterns: Vec<String>,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct ScannerConfig {
    /// Maximum depth for recursive scanning
    #[serde(default = "default_max_depth")]
    pub max_depth: usize,
    
    /// Follow symbolic links
    #[serde(default = "default_follow_links")]
    pub follow_links: bool,
    
    /// Scan hidden files and directories
    #[serde(default = "default_scan_hidden")]
    pub scan_hidden: bool,
    
    /// Parallel scanning threads
    #[serde(default = "default_threads")]
    pub threads: usize,
    
    /// Minimum file age in days before considering for cleanup
    #[serde(default = "default_min_age_days")]
    pub min_age_days: u32,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct LoggingConfig {
    /// Log level (trace, debug, info, warn, error)
    #[serde(default = "default_log_level")]
    pub level: String,
    
    /// Log file path
    pub log_file: Option<PathBuf>,
    
    /// Enable timestamps in logs
    #[serde(default = "default_log_timestamps")]
    pub timestamps: bool,
}

impl Config {
    /// Load configuration from file or create default
    pub fn load() -> Result<Self> {
        let config_path = Self::config_path()?;
        
        if config_path.exists() {
            let contents = std::fs::read_to_string(&config_path)
                .with_context(|| format!("Failed to read config file: {:?}", config_path))?;
            
            toml::from_str(&contents)
                .with_context(|| format!("Failed to parse config file: {:?}", config_path))
        } else {
            let config = Self::default();
            config.save()?;
            Ok(config)
        }
    }
    
    /// Save configuration to file
    pub fn save(&self) -> Result<()> {
        let config_path = Self::config_path()?;
        
        // Create config directory if it doesn't exist
        if let Some(parent) = config_path.parent() {
            std::fs::create_dir_all(parent)
                .with_context(|| format!("Failed to create config directory: {:?}", parent))?;
        }
        
        let contents = toml::to_string_pretty(self)
            .context("Failed to serialize config")?;
        
        std::fs::write(&config_path, contents)
            .with_context(|| format!("Failed to write config file: {:?}", config_path))?;
        
        Ok(())
    }
    
    /// Get the configuration file path
    pub fn config_path() -> Result<PathBuf> {
        let config_dir = dirs::config_dir()
            .context("Failed to get config directory")?;
        
        Ok(config_dir.join("sweeper").join("config.toml"))
    }
    
    /// Load from a custom path
    pub fn load_from(path: &PathBuf) -> Result<Self> {
        let contents = std::fs::read_to_string(path)
            .with_context(|| format!("Failed to read config file: {:?}", path))?;
        
        toml::from_str(&contents)
            .with_context(|| format!("Failed to parse config file: {:?}", path))
    }
}

impl Default for Config {
    fn default() -> Self {
        Self {
            language: default_language(),
            colored_output: default_colored_output(),
            use_trash: default_use_trash(),
            require_confirmation: default_require_confirmation(),
            patterns: PatternsConfig::default(),
            scanner: ScannerConfig::default(),
            logging: LoggingConfig::default(),
        }
    }
}

impl Default for ScannerConfig {
    fn default() -> Self {
        Self {
            max_depth: default_max_depth(),
            follow_links: default_follow_links(),
            scan_hidden: default_scan_hidden(),
            threads: default_threads(),
            min_age_days: default_min_age_days(),
        }
    }
}

impl Default for LoggingConfig {
    fn default() -> Self {
        Self {
            level: default_log_level(),
            log_file: None,
            timestamps: default_log_timestamps(),
        }
    }
}

// Default value functions
fn default_language() -> String { "en".to_string() }
fn default_colored_output() -> bool { true }
fn default_use_trash() -> bool { true }
fn default_require_confirmation() -> bool { true }
fn default_enabled_categories() -> Vec<String> {
    vec![
        "system".to_string(),
        "development".to_string(),
        "applications".to_string(),
    ]
}
fn default_max_depth() -> usize { 10 }
fn default_follow_links() -> bool { false }
fn default_scan_hidden() -> bool { true }
fn default_threads() -> usize { num_cpus::get() }
fn default_min_age_days() -> u32 { 7 }
fn default_log_level() -> String { "info".to_string() }
fn default_log_timestamps() -> bool { true }
