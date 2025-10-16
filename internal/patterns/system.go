package patterns

import "github.com/Taiizor/Sweeper/internal/models"

// getSystemPatterns returns system-level temporary file patterns
func getSystemPatterns() []models.Pattern {
	return []models.Pattern{
		// Windows temporary files
		{
			Name:        "windows_temp",
			Category:    "system",
			Description: "Windows temporary files",
			OS:          []string{"windows"},
			Paths: []string{
				"$TEMP",
				"$TMP",
				"%LOCALAPPDATA%\\Temp",
				"C:\\Windows\\Temp",
				"C:\\Windows\\Prefetch",
			},
			Extensions: []string{".tmp", ".temp", ".log", ".old", ".bak"},
			Priority:   1,
		},
		{
			Name:        "windows_update",
			Category:    "system",
			Description: "Windows Update temporary files",
			OS:          []string{"windows"},
			Paths: []string{
				"C:\\Windows\\SoftwareDistribution\\Download",
			},
			Priority: 2,
		},
		{
			Name:        "windows_thumbnails",
			Category:    "system",
			Description: "Windows thumbnail cache",
			OS:          []string{"windows"},
			Paths: []string{
				"%LOCALAPPDATA%\\Microsoft\\Windows\\Explorer",
			},
			Globs:    []string{"thumbcache_*.db", "iconcache_*.db"},
			Priority: 3,
		},
		{
			Name:        "windows_recycle_bin",
			Category:    "system",
			Description: "Windows Recycle Bin",
			OS:          []string{"windows"},
			Paths: []string{
				"$RECYCLE.BIN",
				"C:\\$Recycle.Bin",
			},
			Protected: true, // Requires special handling
			Priority:  5,
		},

		// Linux/Unix temporary files
		{
			Name:        "linux_temp",
			Category:    "system",
			Description: "Linux temporary files",
			OS:          []string{"linux", "darwin"},
			Paths: []string{
				"/tmp",
				"/var/tmp",
				"/var/cache/apt/archives",
			},
			Extensions: []string{".tmp", ".temp", ".swp", ".swo", ".swn"},
			Priority:   1,
		},
		{
			Name:        "linux_logs",
			Category:    "system",
			Description: "Old system logs",
			OS:          []string{"linux"},
			Paths: []string{
				"/var/log",
			},
			Globs:    []string{"*.log.*", "*.old", "*.1", "*.2", "*.gz"},
			Priority: 3,
		},

		// macOS specific
		{
			Name:        "macos_cache",
			Category:    "system",
			Description: "macOS system cache",
			OS:          []string{"darwin"},
			Paths: []string{
				"~/Library/Caches",
				"/Library/Caches",
				"/System/Library/Caches",
			},
			Priority: 2,
		},
		{
			Name:        "macos_logs",
			Category:    "system",
			Description: "macOS system logs",
			OS:          []string{"darwin"},
			Paths: []string{
				"~/Library/Logs",
				"/Library/Logs",
				"/var/log",
			},
			Extensions: []string{".log", ".asl"},
			Priority:   3,
		},
		{
			Name:        "macos_trash",
			Category:    "system",
			Description: "macOS Trash",
			OS:          []string{"darwin"},
			Paths: []string{
				"~/.Trash",
			},
			Protected: true,
			Priority:  5,
		},

		// Cross-platform
		{
			Name:        "crash_dumps",
			Category:    "system",
			Description: "Application crash dumps",
			OS:          []string{"windows", "linux", "darwin"},
			Paths: []string{
				"~/",
				"$TEMP",
			},
			Globs:    []string{"*.dmp", "core.*", "*.crash"},
			Priority: 4,
		},
	}
}
