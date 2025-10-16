use anyhow::Result;
use colored::Colorize;

use crate::config::Config;
use crate::i18n::I18n;
use crate::patterns::{Category, PatternManager, pattern::SafetyLevel};

/// Execute list command
pub fn execute(
    category: Option<String>,
    config: &Config,
    i18n: &I18n,
) -> Result<()> {
    // Create pattern manager
    let mut pattern_manager = PatternManager::new()?;
    if let Some(ref custom_dir) = config.patterns.custom_patterns_dir {
        pattern_manager.load_custom_patterns(custom_dir)?;
    }
    
    println!("{}", i18n.get("available_patterns").blue().bold());
    println!("{}", "=".repeat(60).blue());
    
    // Get categories to display
    let categories = if let Some(cat_name) = category {
        // Show specific category
        vec![Category::from_string(&cat_name)?]
    } else {
        // Show all categories
        pattern_manager.list_categories()
    };
    
    // Display patterns by category
    for cat in categories {
        let patterns = pattern_manager.get_patterns_by_category(&cat);
        
        if patterns.is_empty() {
            continue;
        }
        
        println!("\n{} {} {}", 
            cat.icon(),
            cat.display_name().cyan().bold(),
            format!("({})", patterns.len()).dimmed()
        );
        println!("  {}", cat.description().italic());
        println!("  {}", "-".repeat(50).dimmed());
        
        for pattern in patterns {
            let platform_str = format!("{:?}", pattern.platforms)
                .replace("All", "🌍")
                .replace("Windows", "🪟")
                .replace("Linux", "🐧")
                .replace("MacOS", "🍎")
                .replace("Unix", "🖥️");
            
            let safety_color = match pattern.safety {
                SafetyLevel::VeryHigh => "green",
                SafetyLevel::High => "green",
                SafetyLevel::Medium => "yellow",
                SafetyLevel::Low => "red",
                SafetyLevel::VeryLow => "red",
            };
            
            println!(
                "  {} {} {}",
                "►".dimmed(),
                pattern.name.bright_white(),
                platform_str
            );
            
            println!(
                "    {} {}",
                i18n.get("pattern_id").dimmed(),
                pattern.id.yellow()
            );
            
            println!(
                "    {} {}",
                i18n.get("description").dimmed(),
                pattern.description
            );
            
            println!(
                "    {} {}",
                i18n.get("pattern").dimmed(),
                pattern.pattern.bright_blue()
            );
            
            println!(
                "    {} {}",
                i18n.get("safety").dimmed(),
                format!("{:?}", pattern.safety).color(safety_color)
            );
            
            if let Some(ref size) = pattern.estimated_size {
                println!(
                    "    {} {}",
                    i18n.get("estimated_size").dimmed(),
                    size.green()
                );
            }
            
            println!();
        }
    }
    
    // Display statistics
    let stats = pattern_manager.get_stats();
    println!("{}", "=".repeat(60).blue());
    println!("{}", i18n.get("pattern_statistics").bold());
    println!(
        "  {} {}",
        i18n.get("total_patterns").yellow(),
        stats.total_patterns
    );
    println!(
        "  {} {}",
        i18n.get("builtin_patterns").yellow(),
        stats.builtin_patterns
    );
    println!(
        "  {} {}",
        i18n.get("custom_patterns").yellow(),
        stats.custom_patterns
    );
    println!(
        "  {} {}",
        i18n.get("categories").yellow(),
        stats.categories
    );
    
    Ok(())
}
