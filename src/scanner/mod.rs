use anyhow::{Context, Result};
use bytesize::ByteSize;
use indicatif::{ProgressBar, ProgressStyle};
use std::path::{Path, PathBuf};
use std::sync::atomic::{AtomicU64, Ordering};
use std::sync::Arc;
use tokio::sync::mpsc;
use walkdir::{DirEntry, WalkDir};

use crate::config::ScannerConfig;
use crate::patterns::{Pattern, PatternManager, PatternMatch};

/// Scanner for finding files matching cleaning patterns
pub struct Scanner {
    config: ScannerConfig,
    pattern_manager: PatternManager,
    total_size: Arc<AtomicU64>,
    total_files: Arc<AtomicU64>,
}

/// Result of a scan operation
#[derive(Debug, Clone)]
pub struct ScanResult {
    /// All matches found
    pub matches: Vec<PatternMatch>,
    
    /// Total size that can be freed
    pub total_size: u64,
    
    /// Total number of files
    pub total_files: usize,
    
    /// Total number of directories
    pub total_directories: usize,
    
    /// Scan duration
    pub duration: std::time::Duration,
    
    /// Errors encountered during scan
    pub errors: Vec<String>,
}

/// Options for scanning
#[derive(Debug, Clone)]
pub struct ScanOptions {
    /// Paths to scan
    pub paths: Vec<PathBuf>,
    
    /// Patterns to use (if empty, use all enabled patterns)
    pub patterns: Vec<String>,
    
    /// Show progress bar
    pub show_progress: bool,
    
    /// Dry run mode
    pub dry_run: bool,
}

impl Scanner {
    /// Create a new scanner
    pub fn new(config: ScannerConfig, pattern_manager: PatternManager) -> Self {
        Self {
            config,
            pattern_manager,
            total_size: Arc::new(AtomicU64::new(0)),
            total_files: Arc::new(AtomicU64::new(0)),
        }
    }
    
    /// Perform a scan with the given options
    pub async fn scan(&self, options: ScanOptions) -> Result<ScanResult> {
        let start_time = std::time::Instant::now();
        let mut matches = Vec::new();
        let mut errors = Vec::new();
        
        // Get patterns to use
        let patterns = if options.patterns.is_empty() {
            self.pattern_manager.get_enabled_patterns(&self.config.enabled_categories)
        } else {
            options.patterns
                .iter()
                .filter_map(|id| self.pattern_manager.get_pattern(id))
                .collect()
        };
        
        // Get scan paths
        let scan_paths = if options.paths.is_empty() {
            self.get_default_scan_paths()?
        } else {
            options.paths
        };
        
        // Create progress bar if needed
        let progress_bar = if options.show_progress {
            Some(self.create_progress_bar())
        } else {
            None
        };
        
        // Scan each path
        for path in scan_paths {
            if !path.exists() {
                errors.push(format!("Path does not exist: {:?}", path));
                continue;
            }
            
            match self.scan_path(&path, &patterns, &progress_bar).await {
                Ok(path_matches) => matches.extend(path_matches),
                Err(e) => errors.push(format!("Error scanning {:?}: {}", path, e)),
            }
        }
        
        if let Some(pb) = progress_bar {
            pb.finish_with_message("Scan complete");
        }
        
        let total_size = self.total_size.load(Ordering::Relaxed);
        let total_files = self.total_files.load(Ordering::Relaxed) as usize;
        let total_directories = matches.iter().filter(|m| m.is_directory).count();
        
        Ok(ScanResult {
            matches,
            total_size,
            total_files,
            total_directories,
            duration: start_time.elapsed(),
            errors,
        })
    }
    
    /// Scan a specific path for matches
    async fn scan_path(
        &self,
        path: &Path,
        patterns: &[&Pattern],
        progress_bar: &Option<ProgressBar>,
    ) -> Result<Vec<PatternMatch>> {
        let mut matches = Vec::new();
        
        // Use WalkDir for recursive traversal
        let walker = WalkDir::new(path)
            .max_depth(self.config.max_depth)
            .follow_links(self.config.follow_links)
            .into_iter()
            .filter_entry(|e| self.should_scan_entry(e));
        
        for entry in walker {
            match entry {
                Ok(entry) => {
                    let entry_path = entry.path();
                    
                    // Update progress bar
                    if let Some(pb) = progress_bar {
                        pb.set_message(format!("Scanning: {}", entry_path.display()));
                        pb.inc(1);
                    }
                    
                    // Check if entry matches any pattern
                    let entry_matches = self.pattern_manager.find_matches(entry_path, patterns.to_vec());
                    
                    for pattern_match in entry_matches {
                        // Check age if configured
                        if self.config.min_age_days > 0 {
                            if !self.is_old_enough(&pattern_match)? {
                                continue;
                            }
                        }
                        
                        // Update counters
                        self.total_size.fetch_add(pattern_match.size, Ordering::Relaxed);
                        if !pattern_match.is_directory {
                            self.total_files.fetch_add(1, Ordering::Relaxed);
                        }
                        
                        matches.push(pattern_match);
                    }
                }
                Err(e) => {
                    // Log error but continue scanning
                    tracing::warn!("Error scanning entry: {}", e);
                }
            }
        }
        
        Ok(matches)
    }
    
    /// Check if an entry should be scanned
    fn should_scan_entry(&self, entry: &DirEntry) -> bool {
        let file_name = entry.file_name().to_string_lossy();
        
        // Check if it's a hidden file/directory
        if !self.config.scan_hidden && file_name.starts_with('.') {
            return false;
        }
        
        // Skip certain system directories
        if entry.file_type().is_dir() {
            match file_name.as_ref() {
                ".git" | ".svn" | ".hg" | "node_modules" => return false,
                _ => {}
            }
        }
        
        true
    }
    
    /// Check if a match is old enough based on configuration
    fn is_old_enough(&self, pattern_match: &PatternMatch) -> Result<bool> {
        if self.config.min_age_days == 0 {
            return Ok(true);
        }
        
        let min_age = std::time::Duration::from_secs(
            self.config.min_age_days as u64 * 24 * 60 * 60
        );
        
        if let Some(last_modified) = pattern_match.metadata.last_modified {
            let age = std::time::SystemTime::now()
                .duration_since(last_modified)
                .unwrap_or(std::time::Duration::ZERO);
            
            Ok(age >= min_age)
        } else {
            Ok(true) // If we can't determine age, include it
        }
    }
    
    /// Get default scan paths based on the operating system
    fn get_default_scan_paths(&self) -> Result<Vec<PathBuf>> {
        let mut paths = Vec::new();
        
        // Add system temp directory
        if let Ok(temp_dir) = std::env::temp_dir().canonicalize() {
            paths.push(temp_dir);
        }
        
        // Add user home directory for user-specific caches
        if let Some(home_dir) = dirs::home_dir() {
            paths.push(home_dir);
        }
        
        // Platform-specific paths
        #[cfg(target_os = "windows")]
        {
            // Windows specific paths
            if let Ok(local_app_data) = std::env::var("LOCALAPPDATA") {
                paths.push(PathBuf::from(local_app_data));
            }
            if let Ok(app_data) = std::env::var("APPDATA") {
                paths.push(PathBuf::from(app_data));
            }
        }
        
        #[cfg(target_os = "linux")]
        {
            // Linux specific paths
            if let Some(cache_dir) = dirs::cache_dir() {
                paths.push(cache_dir);
            }
        }
        
        #[cfg(target_os = "macos")]
        {
            // macOS specific paths
            if let Some(cache_dir) = dirs::cache_dir() {
                paths.push(cache_dir);
            }
            if let Some(home) = dirs::home_dir() {
                paths.push(home.join("Library").join("Caches"));
                paths.push(home.join("Library").join("Logs"));
            }
        }
        
        Ok(paths)
    }
    
    /// Create a progress bar for scanning
    fn create_progress_bar(&self) -> ProgressBar {
        let pb = ProgressBar::new_spinner();
        pb.set_style(
            ProgressStyle::default_spinner()
                .template("{spinner:.green} [{elapsed_precise}] {msg}")
                .unwrap()
                .tick_strings(&["⣾", "⣽", "⣻", "⢿", "⡿", "⣟", "⣯", "⣷"]),
        );
        pb.enable_steady_tick(std::time::Duration::from_millis(120));
        pb
    }
}

impl ScanResult {
    /// Get a summary of the scan results
    pub fn summary(&self) -> String {
        format!(
            "Found {} items ({} files, {} directories) - Total size: {}",
            self.matches.len(),
            self.total_files,
            self.total_directories,
            ByteSize(self.total_size)
        )
    }
    
    /// Group matches by category
    pub fn group_by_category(&self) -> std::collections::HashMap<String, Vec<&PatternMatch>> {
        let mut groups = std::collections::HashMap::new();
        
        for match_item in &self.matches {
            let category_name = match_item.pattern.category.to_string();
            groups
                .entry(category_name)
                .or_insert_with(Vec::new)
                .push(match_item);
        }
        
        groups
    }
    
    /// Group matches by safety level
    pub fn group_by_safety(&self) -> std::collections::HashMap<String, Vec<&PatternMatch>> {
        let mut groups = std::collections::HashMap::new();
        
        for match_item in &self.matches {
            let safety_level = format!("{:?}", match_item.pattern.safety);
            groups
                .entry(safety_level)
                .or_insert_with(Vec::new)
                .push(match_item);
        }
        
        groups
    }
}

impl Default for Scanner {
    fn default() -> Self {
        Self::new(
            ScannerConfig::default(),
            PatternManager::default(),
        )
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    
    #[tokio::test]
    async fn test_scanner_creation() {
        let scanner = Scanner::default();
        assert_eq!(scanner.total_files.load(Ordering::Relaxed), 0);
        assert_eq!(scanner.total_size.load(Ordering::Relaxed), 0);
    }
}
