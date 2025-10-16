pub mod registry;
pub mod pattern;
pub mod category;

use anyhow::Result;
use serde::{Deserialize, Serialize};
use std::path::Path;

pub use pattern::{Pattern, PatternMatch};
pub use category::Category;
pub use registry::Registry;

/// Pattern manager that handles all cleaning patterns
pub struct PatternManager {
    registry: Registry,
    custom_patterns: Vec<Pattern>,
}

impl PatternManager {
    /// Create a new pattern manager with default patterns
    pub fn new() -> Result<Self> {
        let registry = Registry::default();
        Ok(Self {
            registry,
            custom_patterns: Vec::new(),
        })
    }
    
    /// Load custom patterns from directory
    pub fn load_custom_patterns(&mut self, dir: &Path) -> Result<()> {
        if !dir.exists() {
            return Ok(());
        }
        
        let pattern_files = std::fs::read_dir(dir)?
            .filter_map(|entry| entry.ok())
            .filter(|entry| {
                entry.path().extension()
                    .and_then(|ext| ext.to_str())
                    .map(|ext| ext == "toml" || ext == "json")
                    .unwrap_or(false)
            });
        
        for entry in pattern_files {
            let path = entry.path();
            let content = std::fs::read_to_string(&path)?;
            
            let patterns: Vec<Pattern> = if path.extension().unwrap() == "json" {
                serde_json::from_str(&content)?
            } else {
                toml::from_str(&content)?
            };
            
            self.custom_patterns.extend(patterns);
        }
        
        Ok(())
    }
    
    /// Get all patterns for a specific category
    pub fn get_patterns_by_category(&self, category: &Category) -> Vec<&Pattern> {
        let mut patterns = Vec::new();
        
        // Get built-in patterns
        patterns.extend(self.registry.get_patterns_by_category(category));
        
        // Add custom patterns
        for pattern in &self.custom_patterns {
            if &pattern.category == category {
                patterns.push(pattern);
            }
        }
        
        patterns
    }
    
    /// Get all enabled patterns
    pub fn get_enabled_patterns(&self, enabled_categories: &[String]) -> Vec<&Pattern> {
        let mut patterns = Vec::new();
        
        for category_name in enabled_categories {
            if let Ok(category) = Category::from_string(category_name) {
                patterns.extend(self.get_patterns_by_category(&category));
            }
        }
        
        patterns
    }
    
    /// Get all available patterns
    pub fn get_all_patterns(&self) -> Vec<&Pattern> {
        let mut patterns = Vec::new();
        
        // Get all built-in patterns
        patterns.extend(self.registry.get_all_patterns());
        
        // Add all custom patterns
        for pattern in &self.custom_patterns {
            patterns.push(pattern);
        }
        
        patterns
    }
    
    /// Find patterns that match a given path
    pub fn find_matches(&self, path: &Path, patterns: Vec<&Pattern>) -> Vec<PatternMatch> {
        let mut matches = Vec::new();
        
        for pattern in patterns {
            if let Some(pattern_match) = pattern.matches(path) {
                matches.push(pattern_match);
            }
        }
        
        matches
    }
    
    /// Get pattern by ID
    pub fn get_pattern(&self, id: &str) -> Option<&Pattern> {
        self.registry.get_pattern(id)
            .or_else(|| self.custom_patterns.iter().find(|p| p.id == id))
    }
    
    /// List all available categories
    pub fn list_categories(&self) -> Vec<Category> {
        Category::all()
    }
    
    /// Get statistics about patterns
    pub fn get_stats(&self) -> PatternStats {
        PatternStats {
            total_patterns: self.registry.count() + self.custom_patterns.len(),
            builtin_patterns: self.registry.count(),
            custom_patterns: self.custom_patterns.len(),
            categories: self.list_categories().len(),
        }
    }
}

#[derive(Debug, Serialize, Deserialize)]
pub struct PatternStats {
    pub total_patterns: usize,
    pub builtin_patterns: usize,
    pub custom_patterns: usize,
    pub categories: usize,
}

impl Default for PatternManager {
    fn default() -> Self {
        Self::new().expect("Failed to create pattern manager")
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    
    #[test]
    fn test_pattern_manager_creation() {
        let manager = PatternManager::new().unwrap();
        assert!(manager.registry.count() > 0);
    }
    
    #[test]
    fn test_get_patterns_by_category() {
        let manager = PatternManager::new().unwrap();
        let system_patterns = manager.get_patterns_by_category(&Category::System);
        assert!(!system_patterns.is_empty());
    }
}
