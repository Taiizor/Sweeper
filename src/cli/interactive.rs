use anyhow::Result;
use colored::Colorize;
use std::io::{self, Write};

use crate::config::Config;
use crate::i18n::I18n;

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
    // Implementation will be added
    Ok(())
}

async fn clean_interactive(config: &Config, i18n: &I18n) -> Result<()> {
    println!("{}", i18n.get("clean_interactive").blue());
    // Implementation will be added
    Ok(())
}

fn list_interactive(config: &Config, i18n: &I18n) -> Result<()> {
    println!("{}", i18n.get("list_interactive").blue());
    // Implementation will be added
    Ok(())
}

fn config_interactive(config: &Config, i18n: &I18n) -> Result<()> {
    println!("{}", i18n.get("config_interactive").blue());
    // Implementation will be added
    Ok(())
}
