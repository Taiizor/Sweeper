use anyhow::Result;
use bytesize::ByteSize;
use colored::Colorize;
use indicatif::{ProgressBar, ProgressStyle};
use std::io::{self, Write};
use std::path::PathBuf;
use trash;

use crate::config::Config;
use crate::i18n::I18n;
use crate::patterns::{PatternManager, SafetyLevel};
use crate::scanner::{ScanOptions, Scanner};

/// Execute clean command
pub async fn execute(
    path: Option<PathBuf>,
    patterns: Vec<String>,
    force: bool,
    use_trash: bool,
    config: &Config,
    i18n: &I18n,
) -> Result<()> {
    println!("{}", i18n.get("clean_starting").blue().bold());
    
    // Create pattern manager and scanner
    let mut pattern_manager = PatternManager::new()?;
    if let Some(ref custom_dir) = config.patterns.custom_patterns_dir {
        pattern_manager.load_custom_patterns(custom_dir)?;
    }
    
    let scanner = Scanner::new(config.scanner.clone(), pattern_manager);
    
    // First, perform a scan to find what to clean
    let scan_options = ScanOptions {
        paths: path.map(|p| vec![p]).unwrap_or_default(),
        patterns,
        show_progress: true,
        dry_run: false,
    };
    
    println!("{}", i18n.get("scanning_for_cleanup").cyan());
    let scan_result = scanner.scan(scan_options).await?;
    
    if scan_result.matches.is_empty() {
        println!("{}", i18n.get("nothing_to_clean").green());
        return Ok(());
    }
    
    // Display what will be cleaned
    println!("\n{}", i18n.get("items_to_clean").yellow().bold());
    println!("{}", "=".repeat(60).yellow());
    
    let grouped = scan_result.group_by_safety();
    for (safety_level, matches) in &grouped {
        let safety_enum = matches[0].pattern.safety.clone();
        let color = match safety_enum {
            SafetyLevel::VeryHigh | SafetyLevel::High => "green",
            SafetyLevel::Medium => "yellow",
            SafetyLevel::Low | SafetyLevel::VeryLow => "red",
        };
        
        println!("\n  {} {}", "▶".cyan(), format!("Safety: {}", safety_level).color(color).bold());
        
        for (idx, pattern_match) in matches.iter().take(5).enumerate() {
            println!(
                "    {} {} ({})",
                if pattern_match.is_directory { "📁" } else { "📄" },
                pattern_match.path.display().to_string().bright_blue(),
                ByteSize(pattern_match.size).to_string().green()
            );
        }
        
        if matches.len() > 5 {
            println!("    ... {} {}", i18n.get("and"), matches.len() - 5);
        }
    }
    
    println!("\n{}", "=".repeat(60).yellow());
    println!(
        "  {} {}",
        i18n.get("total_to_clean").bold(),
        ByteSize(scan_result.total_size).to_string().bright_white().bold()
    );
    println!("{}", "=".repeat(60).yellow());
    
    // Ask for confirmation unless force flag is set
    if !force && config.require_confirmation {
        print!("\n{} [y/N]: ", i18n.get("confirm_clean").yellow());
        io::stdout().flush()?;
        
        let mut input = String::new();
        io::stdin().read_line(&mut input)?;
        
        if !input.trim().eq_ignore_ascii_case("y") {
            println!("{}", i18n.get("clean_cancelled").red());
            return Ok(());
        }
    }
    
    // Perform cleaning
    println!("\n{}", i18n.get("cleaning_started").green().bold());
    
    let use_trash = use_trash || config.use_trash;
    let cleaned = clean_items(&scan_result.matches, use_trash, i18n).await?;
    
    // Display results
    println!("\n{}", "=".repeat(60).green());
    println!("{}", i18n.get("clean_complete").green().bold());
    println!("{}", "=".repeat(60).green());
    
    println!(
        "  {} {}",
        i18n.get("items_cleaned").green(),
        cleaned.items_cleaned
    );
    
    println!(
        "  {} {}",
        i18n.get("space_freed").green(),
        ByteSize(cleaned.space_freed).to_string().bright_white().bold()
    );
    
    if !cleaned.errors.is_empty() {
        println!("\n{}", i18n.get("clean_errors").red().bold());
        for error in &cleaned.errors {
            println!("  ❌ {}", error);
        }
    }
    
    Ok(())
}

/// Clean the specified items
async fn clean_items(
    matches: &[crate::patterns::PatternMatch],
    use_trash: bool,
    i18n: &I18n,
) -> Result<CleanResult> {
    let pb = ProgressBar::new(matches.len() as u64);
    pb.set_style(
        ProgressStyle::default_bar()
            .template("{spinner:.green} [{elapsed_precise}] [{bar:40.cyan/blue}] {pos}/{len} {msg}")
            .unwrap()
            .progress_chars("#>-"),
    );
    
    let mut items_cleaned = 0;
    let mut space_freed = 0u64;
    let mut errors = Vec::new();
    
    for pattern_match in matches {
        pb.set_message(format!("Cleaning: {}", pattern_match.path.display()));
        
        let result = if use_trash {
            delete_to_trash(&pattern_match.path).await
        } else {
            delete_permanently(&pattern_match.path).await
        };
        
        match result {
            Ok(_) => {
                items_cleaned += 1;
                space_freed += pattern_match.size;
            }
            Err(e) => {
                errors.push(format!(
                    "Failed to clean {}: {}",
                    pattern_match.path.display(),
                    e
                ));
            }
        }
        
        pb.inc(1);
    }
    
    pb.finish_with_message(i18n.get("clean_finished"));
    
    Ok(CleanResult {
        items_cleaned,
        space_freed,
        errors,
    })
}

/// Delete item to trash/recycle bin
async fn delete_to_trash(path: &PathBuf) -> Result<()> {
    trash::delete(path)?;
    Ok(())
}

/// Delete item permanently
async fn delete_permanently(path: &PathBuf) -> Result<()> {
    if path.is_dir() {
        std::fs::remove_dir_all(path)?;
    } else {
        std::fs::remove_file(path)?;
    }
    Ok(())
}

/// Result of a clean operation
struct CleanResult {
    items_cleaned: usize,
    space_freed: u64,
    errors: Vec<String>,
}
