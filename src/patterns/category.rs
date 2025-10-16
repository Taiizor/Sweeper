use serde::{Deserialize, Serialize};
use std::fmt;

/// Categories for organizing cleaning patterns
#[derive(Debug, Clone, PartialEq, Eq, Hash, Serialize, Deserialize)]
#[serde(rename_all = "snake_case")]
pub enum Category {
    /// System temporary files
    System,
    
    /// Development tools and caches
    Development,
    
    /// Application caches and data
    Applications,
    
    /// Browser caches and data
    Browsers,
    
    /// Package manager caches
    PackageManagers,
    
    /// Log files
    Logs,
    
    /// Build artifacts
    BuildArtifacts,
    
    /// IDE and editor caches
    IDEs,
    
    /// Downloads and temporary downloads
    Downloads,
    
    /// Thumbnails and image caches
    Thumbnails,
    
    /// Backup files
    Backups,
    
    /// Custom user-defined category
    Custom(String),
}

impl Category {
    /// Get all predefined categories
    pub fn all() -> Vec<Self> {
        vec![
            Self::System,
            Self::Development,
            Self::Applications,
            Self::Browsers,
            Self::PackageManagers,
            Self::Logs,
            Self::BuildArtifacts,
            Self::IDEs,
            Self::Downloads,
            Self::Thumbnails,
            Self::Backups,
        ]
    }
    
    /// Create a category from a string
    pub fn from_string(s: &str) -> anyhow::Result<Self> {
        match s.to_lowercase().as_str() {
            "system" => Ok(Self::System),
            "development" => Ok(Self::Development),
            "applications" => Ok(Self::Applications),
            "browsers" => Ok(Self::Browsers),
            "package_managers" | "packagemanagers" => Ok(Self::PackageManagers),
            "logs" => Ok(Self::Logs),
            "build_artifacts" | "buildartifacts" => Ok(Self::BuildArtifacts),
            "ides" => Ok(Self::IDEs),
            "downloads" => Ok(Self::Downloads),
            "thumbnails" => Ok(Self::Thumbnails),
            "backups" => Ok(Self::Backups),
            custom => Ok(Self::Custom(custom.to_string())),
        }
    }
    
    /// Get the display name for this category
    pub fn display_name(&self) -> &str {
        match self {
            Self::System => "System Temporary Files",
            Self::Development => "Development Tools",
            Self::Applications => "Application Caches",
            Self::Browsers => "Browser Data",
            Self::PackageManagers => "Package Manager Caches",
            Self::Logs => "Log Files",
            Self::BuildArtifacts => "Build Artifacts",
            Self::IDEs => "IDE Caches",
            Self::Downloads => "Downloads",
            Self::Thumbnails => "Thumbnails",
            Self::Backups => "Backup Files",
            Self::Custom(name) => name,
        }
    }
    
    /// Get a description for this category
    pub fn description(&self) -> &str {
        match self {
            Self::System => "Operating system temporary files and caches",
            Self::Development => "Development tools caches and temporary files",
            Self::Applications => "Application-specific caches and temporary data",
            Self::Browsers => "Web browser caches, cookies, and history",
            Self::PackageManagers => "Package manager download caches and metadata",
            Self::Logs => "System and application log files",
            Self::BuildArtifacts => "Compiled binaries and build outputs",
            Self::IDEs => "IDE and text editor caches and temporary files",
            Self::Downloads => "Downloaded files and incomplete downloads",
            Self::Thumbnails => "Image and video thumbnail caches",
            Self::Backups => "Backup files and old versions",
            Self::Custom(_) => "Custom user-defined category",
        }
    }
    
    /// Get an icon representation for this category
    pub fn icon(&self) -> &str {
        match self {
            Self::System => "🖥️",
            Self::Development => "⚙️",
            Self::Applications => "📱",
            Self::Browsers => "🌐",
            Self::PackageManagers => "📦",
            Self::Logs => "📝",
            Self::BuildArtifacts => "🔨",
            Self::IDEs => "💻",
            Self::Downloads => "⬇️",
            Self::Thumbnails => "🖼️",
            Self::Backups => "💾",
            Self::Custom(_) => "📂",
        }
    }
    
    /// Check if this category is considered high-priority
    pub fn is_high_priority(&self) -> bool {
        matches!(
            self,
            Self::System | Self::Browsers | Self::PackageManagers | Self::Development
        )
    }
}

impl fmt::Display for Category {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(f, "{}", self.display_name())
    }
}

impl Default for Category {
    fn default() -> Self {
        Self::System
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    
    #[test]
    fn test_category_from_string() {
        assert_eq!(Category::from_string("system").unwrap(), Category::System);
        assert_eq!(Category::from_string("SYSTEM").unwrap(), Category::System);
        assert_eq!(
            Category::from_string("package_managers").unwrap(),
            Category::PackageManagers
        );
        
        if let Category::Custom(name) = Category::from_string("my_custom").unwrap() {
            assert_eq!(name, "my_custom");
        } else {
            panic!("Expected Custom category");
        }
    }
    
    #[test]
    fn test_all_categories() {
        let all = Category::all();
        assert!(all.contains(&Category::System));
        assert!(all.contains(&Category::Development));
        assert_eq!(all.len(), 11);
    }
}
