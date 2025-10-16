use anyhow::Result;
use bytesize::ByteSize;
use colored::Colorize;
use std::path::PathBuf;

use crate::config::Config;
use crate::i18n::I18n;
use crate::patterns::PatternManager;
use crate::scanner::{ScanOptions, Scanner};

/// Execute scan command
pub async fn execute(
    path: Option<PathBuf>,
    patterns: Vec<String>,
    dry_run: bool,
    config: &Config,
    i18n: &I18n,
) -> Result<()> {
    println!("{}", i18n.get("scan_starting").blue().bold());
    
    // Create pattern manager and scanner
    let mut pattern_manager = PatternManager::new()?;
    if let Some(ref custom_dir) = config.patterns.custom_patterns_dir {
        pattern_manager.load_custom_patterns(custom_dir)?;
    }
    
    let scanner = Scanner::new(config.scanner.clone(), pattern_manager);
    
    // Prepare scan options
    let scan_options = ScanOptions {
        paths: path.map(|p| vec![p]).unwrap_or_default(),
        patterns,
        show_progress: true,
        dry_run,
    };
    
    // Perform scan
    let result = scanner.scan(scan_options).await?;
    
    // Display results
    display_results(&result, i18n)?;
    
    // Display summary
    println!("\n{}", "=".repeat(60).cyan());
    println!("{}", i18n.get("scan_summary").bold());
    println!("{}", "=".repeat(60).cyan());
    
    println!(
        "  {} {}",
        i18n.get("total_items").yellow(),
        result.matches.len().to_string().bright_white()
    );
    
    println!(
        "  {} {}",
        i18n.get("total_size").yellow(),
        ByteSize(result.total_size).to_string().bright_white()
    );
    
    println!(
        "  {} {}",
        i18n.get("scan_duration").yellow(),
        format!("{:.2}s", result.duration.as_secs_f64()).bright_white()
    );
    
    if !result.errors.is_empty() {
        println!("\n{}", i18n.get("scan_errors").red().bold());
        for error in &result.errors {
            println!("  ❌ {}", error);
        }
    }
    
    if dry_run {
        println!("\n{}", i18n.get("dry_run_notice").yellow().italic());
    }
    
    Ok(())
}

/// Display scan results grouped by category
fn display_results(result: &crate::scanner::ScanResult, i18n: &I18n) -> Result<()> {
    let grouped = result.group_by_category();
    
    for (category, matches) in grouped {
        if matches.is_empty() {
            continue;
        }
        
        println!("\n{} {}", "▶".cyan(), category.bold());
        println!("{}", "-".repeat(40).dim());
        
        let mut category_size = 0u64;
        let mut displayed = 0;
        const MAX_ITEMS: usize = 10;
        
        for pattern_match in matches.iter().take(MAX_ITEMS) {
            let size_str = ByteSize(pattern_match.size).to_string();
            let path_str = pattern_match.path.display().to_string();
            
            let icon = if pattern_match.is_directory {
                "📁"
            } else {
                "📄"
            };
            
            println!(
                "  {} {} ({})",
                icon,
                path_str.bright_blue(),
                size_str.green()
            );
            
            category_size += pattern_match.size;
            displayed += 1;
        }
        
        if matches.len() > MAX_ITEMS {
            println!(
                "  {} {} {}",
                "...".dim(),
                i18n.get("and_more").dim(),
                (matches.len() - MAX_ITEMS).to_string().dim()
            );
        }
        
        println!(
            "  {} {}",
            i18n.get("category_total").italic(),
            ByteSize(category_size).to_string().green().bold()
        );
    }
    
    Ok(())
}
