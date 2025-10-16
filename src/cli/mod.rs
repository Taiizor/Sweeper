pub mod completions;
pub mod interactive;

use clap::{Parser, Subcommand, ValueEnum};
use std::path::PathBuf;

/// Sweeper - A powerful cross-platform CLI tool for cleaning temporary files and directories
#[derive(Parser, Debug)]
#[command(
    name = "sweeper",
    version,
    about,
    long_about = None,
    author = "Taiizor <https://github.com/Taiizor>"
)]
pub struct Cli {
    /// Set the verbosity level
    #[arg(short, long, action = clap::ArgAction::Count)]
    pub verbose: u8,
    
    /// Suppress all output except errors
    #[arg(short, long)]
    pub quiet: bool,
    
    /// Use a custom config file
    #[arg(short, long, value_name = "FILE")]
    pub config: Option<PathBuf>,
    
    /// Set the language (e.g., en, tr, de, fr)
    #[arg(short, long, value_name = "LANG")]
    pub language: Option<String>,
    
    #[command(subcommand)]
    pub command: Option<Commands>,
}

#[derive(Subcommand, Debug)]
pub enum Commands {
    /// Scan for temporary files and directories
    Scan {
        /// Target path to scan (defaults to system temp directories)
        #[arg(short, long)]
        path: Option<PathBuf>,
        
        /// Patterns to scan for (e.g., npm-cache, cargo-cache, temp-files)
        #[arg(short = 't', long, value_name = "PATTERN")]
        patterns: Vec<String>,
        
        /// Perform a dry run without making changes
        #[arg(short, long)]
        dry_run: bool,
    },
    
    /// Clean temporary files and directories
    Clean {
        /// Target path to clean (defaults to system temp directories)
        #[arg(short, long)]
        path: Option<PathBuf>,
        
        /// Patterns to clean (e.g., npm-cache, cargo-cache, temp-files)
        #[arg(short = 't', long, value_name = "PATTERN")]
        patterns: Vec<String>,
        
        /// Force deletion without confirmation
        #[arg(short, long)]
        force: bool,
        
        /// Move files to trash instead of permanent deletion
        #[arg(short = 'r', long)]
        trash: bool,
    },
    
    /// List available cleaning patterns
    List {
        /// Filter by category
        #[arg(short, long)]
        category: Option<String>,
    },
    
    /// Manage configuration
    Config {
        #[command(subcommand)]
        command: ConfigCommands,
    },
    
    /// Generate shell completions
    Completions {
        /// Shell to generate completions for
        #[arg(value_enum)]
        shell: Shell,
    },
}

#[derive(Subcommand, Debug)]
pub enum ConfigCommands {
    /// Show current configuration
    Show,
    
    /// Edit configuration
    Edit {
        /// Configuration key
        key: String,
        
        /// New value
        value: String,
    },
    
    /// Reset configuration to defaults
    Reset {
        /// Force reset without confirmation
        #[arg(short, long)]
        force: bool,
    },
    
    /// Export configuration to file
    Export {
        /// Output file path
        #[arg(short, long)]
        output: PathBuf,
    },
    
    /// Import configuration from file
    Import {
        /// Input file path
        #[arg(short, long)]
        input: PathBuf,
    },
}

#[derive(ValueEnum, Clone, Debug)]
pub enum Shell {
    Bash,
    Fish,
    Zsh,
    PowerShell,
    Elvish,
}
