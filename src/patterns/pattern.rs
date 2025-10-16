use glob::Pattern as GlobPattern;
use serde::{Deserialize, Serialize};
use std::path::{Path, PathBuf};

use super::category::Category;

/// Represents a cleaning pattern for temporary files
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct Pattern {
    /// Unique identifier for the pattern
    pub id: String,
    
    /// Human-readable name
    pub name: String,
    
    /// Description of what this pattern cleans
    pub description: String,
    
    /// Category this pattern belongs to
    pub category: Category,
    
    /// Type of pattern matching
    pub pattern_type: PatternType,
    
    /// The actual pattern or path
    pub pattern: String,
    
    /// Operating systems this pattern applies to
    pub platforms: Vec<Platform>,
    
    /// Whether this pattern is enabled by default
    pub enabled_by_default: bool,
    
    /// Safety level of this pattern
    pub safety: SafetyLevel,
    
    /// Estimated disk space that can be freed (optional)
    pub estimated_size: Option<String>,
}

/// Type of pattern matching
#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
#[serde(rename_all = "snake_case")]
pub enum PatternType {
    /// Exact path match
    Exact,
    /// Glob pattern match
    Glob,
    /// Regular expression match
    Regex,
    /// Environment variable based path
    EnvVar,
    /// Special system directory
    SystemDir,
}

/// Supported platforms
#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
#[serde(rename_all = "snake_case")]
pub enum Platform {
    Windows,
    Linux,
    MacOS,
    Unix,  // Linux and MacOS
    All,   // All platforms
}

/// Safety level for cleaning operations
#[derive(Debug, Clone, Serialize, Deserialize, PartialEq, Ord, PartialOrd, Eq)]
#[serde(rename_all = "snake_case")]
pub enum SafetyLevel {
    /// Very safe to delete
    VeryHigh,
    /// Generally safe to delete
    High,
    /// Safe with user confirmation
    Medium,
    /// Requires careful consideration
    Low,
    /// Potentially dangerous
    VeryLow,
}

/// Represents a match found by a pattern
#[derive(Debug, Clone)]
pub struct PatternMatch {
    /// The pattern that matched
    pub pattern: Pattern,
    
    /// The path that was matched
    pub path: PathBuf,
    
    /// Size of the matched item
    pub size: u64,
    
    /// Whether it's a directory
    pub is_directory: bool,
    
    /// Additional metadata
    pub metadata: MatchMetadata,
}

/// Additional metadata for a pattern match
#[derive(Debug, Clone)]
pub struct MatchMetadata {
    /// Last modified time
    pub last_modified: Option<std::time::SystemTime>,
    
    /// Last accessed time
    pub last_accessed: Option<std::time::SystemTime>,
    
    /// Number of items (for directories)
    pub item_count: Option<usize>,
}

impl Pattern {
    /// Create a new pattern
    pub fn new(
        id: impl Into<String>,
        name: impl Into<String>,
        description: impl Into<String>,
        category: Category,
        pattern: impl Into<String>,
    ) -> Self {
        Self {
            id: id.into(),
            name: name.into(),
            description: description.into(),
            category,
            pattern_type: PatternType::Glob,
            pattern: pattern.into(),
            platforms: vec![Platform::All],
            enabled_by_default: true,
            safety: SafetyLevel::High,
            estimated_size: None,
        }
    }
    
    /// Check if this pattern applies to the current platform
    pub fn is_platform_compatible(&self) -> bool {
        if self.platforms.contains(&Platform::All) {
            return true;
        }
        
        #[cfg(target_os = "windows")]
        {
            self.platforms.contains(&Platform::Windows)
        }
        
        #[cfg(target_os = "linux")]
        {
            self.platforms.contains(&Platform::Linux) || 
            self.platforms.contains(&Platform::Unix)
        }
        
        #[cfg(target_os = "macos")]
        {
            self.platforms.contains(&Platform::MacOS) || 
            self.platforms.contains(&Platform::Unix)
        }
        
        #[cfg(not(any(target_os = "windows", target_os = "linux", target_os = "macos")))]
        {
            false
        }
    }
    
    /// Check if a path matches this pattern
    pub fn matches(&self, path: &Path) -> Option<PatternMatch> {
        if !self.is_platform_compatible() {
            return None;
        }
        
        let matches = match self.pattern_type {
            PatternType::Exact => self.match_exact(path),
            PatternType::Glob => self.match_glob(path),
            PatternType::Regex => self.match_regex(path),
            PatternType::EnvVar => self.match_env_var(path),
            PatternType::SystemDir => self.match_system_dir(path),
        };
        
        if matches {
            Some(self.create_match(path))
        } else {
            None
        }
    }
    
    fn match_exact(&self, path: &Path) -> bool {
        path == Path::new(&self.pattern)
    }
    
    fn match_glob(&self, path: &Path) -> bool {
        if let Ok(glob_pattern) = GlobPattern::new(&self.pattern) {
            glob_pattern.matches_path(path)
        } else {
            false
        }
    }
    
    fn match_regex(&self, _path: &Path) -> bool {
        // Implementation for regex matching
        false // Placeholder
    }
    
    fn match_env_var(&self, _path: &Path) -> bool {
        // Implementation for environment variable based matching
        false // Placeholder
    }
    
    fn match_system_dir(&self, _path: &Path) -> bool {
        // Implementation for system directory matching
        false // Placeholder
    }
    
    fn create_match(&self, path: &Path) -> PatternMatch {
        let metadata = std::fs::metadata(path);
        
        let (size, is_directory, last_modified, last_accessed) = if let Ok(meta) = metadata {
            (
                meta.len(),
                meta.is_dir(),
                meta.modified().ok(),
                meta.accessed().ok(),
            )
        } else {
            (0, false, None, None)
        };
        
        let item_count = if is_directory {
            std::fs::read_dir(path)
                .ok()
                .map(|entries| entries.count())
        } else {
            None
        };
        
        PatternMatch {
            pattern: self.clone(),
            path: path.to_path_buf(),
            size,
            is_directory,
            metadata: MatchMetadata {
                last_modified,
                last_accessed,
                item_count,
            },
        }
    }
    
    /// Builder method to set the pattern type
    pub fn with_type(mut self, pattern_type: PatternType) -> Self {
        self.pattern_type = pattern_type;
        self
    }
    
    /// Builder method to set the platforms
    pub fn with_platforms(mut self, platforms: Vec<Platform>) -> Self {
        self.platforms = platforms;
        self
    }
    
    /// Builder method to set the safety level
    pub fn with_safety(mut self, safety: SafetyLevel) -> Self {
        self.safety = safety;
        self
    }
    
    /// Builder method to set whether enabled by default
    pub fn with_enabled(mut self, enabled: bool) -> Self {
        self.enabled_by_default = enabled;
        self
    }
}
