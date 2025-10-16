pub mod logging;

use std::path::{Path, PathBuf};

/// Expand environment variables in a path string
pub fn expand_env_vars(path: &str) -> PathBuf {
    let expanded = shellexpand::full(path)
        .unwrap_or_else(|_| std::borrow::Cow::Borrowed(path));
    PathBuf::from(expanded.as_ref())
}

/// Get the size of a directory recursively
pub fn get_dir_size(path: &Path) -> std::io::Result<u64> {
    let mut total_size = 0u64;
    
    if path.is_file() {
        return Ok(path.metadata()?.len());
    }
    
    for entry in std::fs::read_dir(path)? {
        let entry = entry?;
        let metadata = entry.metadata()?;
        
        if metadata.is_dir() {
            total_size += get_dir_size(&entry.path())?;
        } else {
            total_size += metadata.len();
        }
    }
    
    Ok(total_size)
}

/// Check if a path is safe to delete
pub fn is_safe_to_delete(path: &Path) -> bool {
    // Never delete system critical paths
    let path_str = path.to_string_lossy().to_lowercase();
    
    // Windows critical paths
    if cfg!(windows) {
        if path_str.contains("windows") && !path_str.contains("temp") {
            return false;
        }
        if path_str.contains("program files") {
            return false;
        }
        if path_str.contains("system32") || path_str.contains("syswow64") {
            return false;
        }
    }
    
    // Unix/Linux critical paths
    if cfg!(unix) {
        let critical_paths = [
            "/bin", "/boot", "/dev", "/etc", "/lib", "/lib64",
            "/proc", "/root", "/sbin", "/sys", "/usr/bin",
            "/usr/sbin", "/usr/lib", "/usr/lib64"
        ];
        
        for critical in &critical_paths {
            if path.starts_with(critical) {
                return false;
            }
        }
    }
    
    true
}

/// Format a duration in a human-readable way
pub fn format_duration(duration: std::time::Duration) -> String {
    let secs = duration.as_secs();
    let millis = duration.as_millis();
    
    if secs == 0 {
        format!("{}ms", millis)
    } else if secs < 60 {
        format!("{:.2}s", duration.as_secs_f64())
    } else if secs < 3600 {
        let mins = secs / 60;
        let secs = secs % 60;
        format!("{}m {}s", mins, secs)
    } else {
        let hours = secs / 3600;
        let mins = (secs % 3600) / 60;
        format!("{}h {}m", hours, mins)
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    
    #[test]
    fn test_is_safe_to_delete() {
        assert!(!is_safe_to_delete(Path::new("/bin/bash")));
        assert!(!is_safe_to_delete(Path::new("C:\\Windows\\System32")));
        assert!(is_safe_to_delete(Path::new("/tmp/test")));
        assert!(is_safe_to_delete(Path::new("C:\\Temp\\test")));
    }
    
    #[test]
    fn test_format_duration() {
        use std::time::Duration;
        
        assert_eq!(format_duration(Duration::from_millis(500)), "500ms");
        assert_eq!(format_duration(Duration::from_secs(30)), "30.00s");
        assert_eq!(format_duration(Duration::from_secs(90)), "1m 30s");
        assert_eq!(format_duration(Duration::from_secs(3700)), "1h 1m");
    }
}
