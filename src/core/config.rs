use anyhow::Result;
use colored::Colorize;
use std::path::PathBuf;

use crate::cli::ConfigCommands;
use crate::config::Config;
use crate::i18n::I18n;

/// Execute config command
pub fn execute(
    command: ConfigCommands,
    config: &Config,
    i18n: &I18n,
) -> Result<()> {
    match command {
        ConfigCommands::Show => show_config(config, i18n),
        ConfigCommands::Edit { key, value } => edit_config(key, value, config, i18n),
        ConfigCommands::Reset { force } => reset_config(force, config, i18n),
        ConfigCommands::Export { output } => export_config(output, config, i18n),
        ConfigCommands::Import { input } => import_config(input, i18n),
    }
}

/// Show current configuration
fn show_config(config: &Config, i18n: &I18n) -> Result<()> {
    println!("{}", i18n.get("current_config").blue().bold());
    println!("{}", "=".repeat(60).blue());
    
    println!("\n{}", i18n.get("general_settings").cyan().bold());
    println!("  {} {}", i18n.get("language").yellow(), config.language);
    println!("  {} {}", i18n.get("colored_output").yellow(), config.colored_output);
    println!("  {} {}", i18n.get("use_trash").yellow(), config.use_trash);
    println!("  {} {}", i18n.get("require_confirmation").yellow(), config.require_confirmation);
    
    println!("\n{}", i18n.get("pattern_settings").cyan().bold());
    if let Some(ref dir) = config.patterns.custom_patterns_dir {
        println!("  {} {:?}", i18n.get("custom_patterns_dir").yellow(), dir);
    }
    println!("  {} {:?}", i18n.get("enabled_categories").yellow(), config.patterns.enabled_categories);
    println!("  {} {:?}", i18n.get("excluded_patterns").yellow(), config.patterns.excluded_patterns);
    
    println!("\n{}", i18n.get("scanner_settings").cyan().bold());
    println!("  {} {}", i18n.get("max_depth").yellow(), config.scanner.max_depth);
    println!("  {} {}", i18n.get("follow_links").yellow(), config.scanner.follow_links);
    println!("  {} {}", i18n.get("scan_hidden").yellow(), config.scanner.scan_hidden);
    println!("  {} {}", i18n.get("threads").yellow(), config.scanner.threads);
    println!("  {} {} days", i18n.get("min_age").yellow(), config.scanner.min_age_days);
    
    println!("\n{}", i18n.get("logging_settings").cyan().bold());
    println!("  {} {}", i18n.get("log_level").yellow(), config.logging.level);
    if let Some(ref path) = config.logging.log_file {
        println!("  {} {:?}", i18n.get("log_file").yellow(), path);
    }
    println!("  {} {}", i18n.get("timestamps").yellow(), config.logging.timestamps);
    
    println!("\n{}", i18n.get("config_location").dim());
    println!("  {:?}", Config::config_path()?);
    
    Ok(())
}

/// Edit a configuration value
fn edit_config(key: String, value: String, config: &Config, i18n: &I18n) -> Result<()> {
    let mut new_config = config.clone();
    
    match key.as_str() {
        "language" => new_config.language = value,
        "colored_output" => new_config.colored_output = value.parse()?,
        "use_trash" => new_config.use_trash = value.parse()?,
        "require_confirmation" => new_config.require_confirmation = value.parse()?,
        "scanner.max_depth" => new_config.scanner.max_depth = value.parse()?,
        "scanner.follow_links" => new_config.scanner.follow_links = value.parse()?,
        "scanner.scan_hidden" => new_config.scanner.scan_hidden = value.parse()?,
        "scanner.threads" => new_config.scanner.threads = value.parse()?,
        "scanner.min_age_days" => new_config.scanner.min_age_days = value.parse()?,
        "logging.level" => new_config.logging.level = value,
        "logging.timestamps" => new_config.logging.timestamps = value.parse()?,
        _ => {
            println!("{}", i18n.get("invalid_config_key").red());
            return Ok(());
        }
    }
    
    new_config.save()?;
    println!("{}", i18n.get("config_updated").green());
    println!("  {} = {}", key.yellow(), value.bright_white());
    
    Ok(())
}

/// Reset configuration to defaults
fn reset_config(force: bool, config: &Config, i18n: &I18n) -> Result<()> {
    if !force {
        print!("{} [y/N]: ", i18n.get("confirm_reset").yellow());
        std::io::Write::flush(&mut std::io::stdout())?;
        
        let mut input = String::new();
        std::io::stdin().read_line(&mut input)?;
        
        if !input.trim().eq_ignore_ascii_case("y") {
            println!("{}", i18n.get("reset_cancelled").red());
            return Ok(());
        }
    }
    
    let default_config = Config::default();
    default_config.save()?;
    
    println!("{}", i18n.get("config_reset").green());
    Ok(())
}

/// Export configuration to file
fn export_config(output: PathBuf, config: &Config, i18n: &I18n) -> Result<()> {
    let contents = toml::to_string_pretty(config)?;
    std::fs::write(&output, contents)?;
    
    println!("{}", i18n.get("config_exported").green());
    println!("  {:?}", output);
    
    Ok(())
}

/// Import configuration from file
fn import_config(input: PathBuf, i18n: &I18n) -> Result<()> {
    let contents = std::fs::read_to_string(&input)?;
    let new_config: Config = toml::from_str(&contents)?;
    
    new_config.save()?;
    
    println!("{}", i18n.get("config_imported").green());
    println!("  {:?}", input);
    
    Ok(())
}
