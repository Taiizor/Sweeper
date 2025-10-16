use anyhow::Result;
use colored::Colorize;
use std::io::{self, Write};
use std::path::PathBuf;

use crate::config::Config;
use crate::i18n::I18n;
use crate::core::{scan, clean, list, config as config_core};
use crate::cli::ConfigCommands;

pub async fn run(config: &Config, i18n: &I18n) -> Result<()> {
    println!("{}", i18n.get("welcome").cyan().bold());
    println!("{}", "=".repeat(50).cyan());
    
    loop {
        print!("{} ", i18n.get("prompt").green());
        io::stdout().flush()?;
        
        let mut input = String::new();
        io::stdin().read_line(&mut input)?;
        
        let input = input.trim();
        
        match input {
            "help" | "h" => show_help(i18n),
            "scan" | "s" => scan_interactive(config, i18n).await?,
            "clean" | "c" => clean_interactive(config, i18n).await?,
            "list" | "l" => list_interactive(config, i18n)?,
            "config" => config_interactive(config, i18n)?,
            "exit" | "quit" | "q" => {
                println!("{}", i18n.get("goodbye").yellow());
                break;
            }
            _ => println!("{}", i18n.get("invalid_command").red()),
        }
    }
    
    Ok(())
}

fn show_help(i18n: &I18n) {
    println!("\n{}", i18n.get("available_commands").bold());
    println!("  {} - {}", "scan (s)".cyan(), i18n.get("scan_description"));
    println!("  {} - {}", "clean (c)".cyan(), i18n.get("clean_description"));
    println!("  {} - {}", "list (l)".cyan(), i18n.get("list_description"));
    println!("  {} - {}", "config".cyan(), i18n.get("config_description"));
    println!("  {} - {}", "help (h)".cyan(), i18n.get("help_description"));
    println!("  {} - {}", "exit (q)".cyan(), i18n.get("exit_description"));
    println!();
}

async fn scan_interactive(config: &Config, i18n: &I18n) -> Result<()> {
    println!("{}", i18n.get("scan_interactive").blue());
    
    // Ask for path (optional)
    print!("Enter path to scan (press Enter for current directory): ");
    io::stdout().flush()?;
    let mut input = String::new();
    io::stdin().read_line(&mut input)?;
    
    let path = if input.trim().is_empty() {
        None
    } else {
        Some(PathBuf::from(input.trim()))
    };
    
    // Run scan with all patterns
    scan::execute(path, Vec::new(), false, config, i18n).await?;
    
    Ok(())
}

async fn clean_interactive(config: &Config, i18n: &I18n) -> Result<()> {
    println!("{}", i18n.get("clean_interactive").blue());
    
    // Ask for path (optional)
    print!("Enter path to clean (press Enter for current directory): ");
    io::stdout().flush()?;
    let mut input = String::new();
    io::stdin().read_line(&mut input)?;
    
    let path = if input.trim().is_empty() {
        None
    } else {
        Some(PathBuf::from(input.trim()))
    };
    
    // Run clean with all patterns
    clean::execute(path, Vec::new(), false, false, config, i18n).await?;
    
    Ok(())
}

fn list_interactive(config: &Config, i18n: &I18n) -> Result<()> {
    println!("{}", i18n.get("list_interactive").blue());
    
    // Ask for category filter (optional)
    print!("Enter category to filter (press Enter for all): ");
    io::stdout().flush()?;
    let mut input = String::new();
    io::stdin().read_line(&mut input)?;
    
    let category = if input.trim().is_empty() {
        None
    } else {
        Some(input.trim().to_string())
    };
    
    // List patterns
    list::execute(category, config, i18n)?;
    
    Ok(())
}

fn config_interactive(config: &Config, i18n: &I18n) -> Result<()> {
    println!("{}", i18n.get("config_interactive").blue());
    println!("\nConfig options:");
    println!("  1. Show current config");
    println!("  2. Edit config value");
    println!("  3. Reset to defaults");
    println!("  4. Back to main menu\n");
    
    print!("Choose option [1-4]: ");
    io::stdout().flush()?;
    let mut input = String::new();
    io::stdin().read_line(&mut input)?;
    
    match input.trim() {
        "1" => {
            config_core::execute(ConfigCommands::Show, config, i18n)?;
        }
        "2" => {
            print!("Enter key to edit: ");
            io::stdout().flush()?;
            let mut key = String::new();
            io::stdin().read_line(&mut key)?;
            
            print!("Enter new value: ");
            io::stdout().flush()?;
            let mut value = String::new();
            io::stdin().read_line(&mut value)?;
            
            config_core::execute(
                ConfigCommands::Edit {
                    key: key.trim().to_string(),
                    value: value.trim().to_string(),
                },
                config,
                i18n,
            )?;
        }
        "3" => {
            print!("Are you sure you want to reset config? [y/N]: ");
            io::stdout().flush()?;
            let mut confirm = String::new();
            io::stdin().read_line(&mut confirm)?;
            
            if confirm.trim().eq_ignore_ascii_case("y") {
                config_core::execute(
                    ConfigCommands::Reset { force: true },
                    config,
                    i18n,
                )?;
            }
        }
        "4" => {
            // Back to main menu
        }
        _ => {
            println!("Invalid option");
        }
    }
    
    Ok(())
}
