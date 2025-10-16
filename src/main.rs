mod cli;
mod config;
mod core;
mod i18n;
mod patterns;
mod scanner;
mod utils;

use anyhow::Result;
use clap::Parser;
use tracing::info;

use crate::cli::{Cli, Commands};
use crate::config::Config;
use crate::i18n::I18n;

#[tokio::main]
async fn main() -> Result<()> {
    // Initialize tracing/logging
    utils::logging::init()?;
    
    // Parse CLI arguments
    let cli = Cli::parse();
    
    // Load configuration
    let config = Config::load()?;
    
    // Initialize internationalization
    let i18n = I18n::new(&config.language)?;
    
    // Process commands
    match cli.command {
        Some(Commands::Scan { path, patterns, dry_run }) => {
            info!("Starting scan operation");
            core::scan::execute(path, patterns, dry_run, &config, &i18n).await?;
        }
        Some(Commands::Clean { path, patterns, force, trash }) => {
            info!("Starting clean operation");
            core::clean::execute(path, patterns, force, trash, &config, &i18n).await?;
        }
        Some(Commands::List { category }) => {
            info!("Listing patterns");
            core::list::execute(category, &config, &i18n)?;
        }
        Some(Commands::Config { command }) => {
            info!("Managing configuration");
            core::config::execute(command, &config, &i18n)?;
        }
        Some(Commands::Completions { shell }) => {
            cli::completions::generate(shell);
        }
        None => {
            // Interactive mode or show help
            cli::interactive::run(&config, &i18n).await?;
        }
    }
    
    Ok(())
}
