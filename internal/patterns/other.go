package patterns

import "github.com/Taiizor/Sweeper/internal/models"

// getLogPatterns returns log file patterns
func getLogPatterns() []models.Pattern {
	return []models.Pattern{
		{
			Name:        "application_logs",
			Category:    "logs",
			Description: "Application log files",
			OS:          []string{"windows", "linux", "darwin"},
			Paths: []string{
				".",
				"/var/log",
				"C:\\ProgramData",
			},
			Extensions: []string{".log", ".txt", ".out"},
			Priority:   2,
		},
		{
			Name:        "old_logs",
			Category:    "logs",
			Description: "Archived and rotated log files",
			OS:          []string{"windows", "linux", "darwin"},
			Paths: []string{
				".",
			},
			Globs:    []string{"*.log.*", "*.old", "*.bak", "*.1", "*.2", "*.gz"},
			Priority: 1,
		},
	}
}

// getCachePatterns returns general cache patterns
func getCachePatterns() []models.Pattern {
	return []models.Pattern{
		{
			Name:        "thumbnail_cache",
			Category:    "cache",
			Description: "Thumbnail cache files",
			OS:          []string{"windows", "linux", "darwin"},
			Paths: []string{
				"~/.cache/thumbnails",
				"~/Library/Caches",
				"%LOCALAPPDATA%\\Microsoft\\Windows\\Explorer",
			},
			Globs:    []string{"*.thumb", "thumbcache_*.db"},
			Priority: 2,
		},
		{
			Name:        "font_cache",
			Category:    "cache",
			Description: "Font cache files",
			OS:          []string{"windows", "linux", "darwin"},
			Paths: []string{
				"~/.cache/fontconfig",
				"~/Library/Caches/com.apple.fontd",
				"%LOCALAPPDATA%\\FontCache",
			},
			Priority: 3,
		},
		{
			Name:        "dns_cache",
			Category:    "cache",
			Description: "DNS cache",
			OS:          []string{"windows"},
			Paths: []string{
				"C:\\Windows\\System32\\config\\systemprofile\\AppData\\Local\\Microsoft\\Windows\\INetCache",
			},
			Priority: 3,
		},
	}
}

// getDockerPatterns returns Docker-related patterns
func getDockerPatterns() []models.Pattern {
	return []models.Pattern{
		{
			Name:        "docker_images",
			Category:    "docker",
			Description: "Unused Docker images",
			OS:          []string{"windows", "linux", "darwin"},
			Paths: []string{
				"/var/lib/docker",
				"C:\\ProgramData\\Docker",
				"~/Library/Containers/com.docker.docker",
			},
			Protected: true, // Requires Docker commands
			Priority:  3,
		},
		{
			Name:        "docker_volumes",
			Category:    "docker",
			Description: "Unused Docker volumes",
			OS:          []string{"windows", "linux", "darwin"},
			Paths: []string{
				"/var/lib/docker/volumes",
			},
			Protected: true, // Requires Docker commands
			Priority:  3,
		},
		{
			Name:        "docker_build_cache",
			Category:    "docker",
			Description: "Docker build cache",
			OS:          []string{"windows", "linux", "darwin"},
			Paths: []string{
				"~/.docker/buildcache",
			},
			Priority: 2,
		},
		{
			Name:        "podman_cache",
			Category:    "docker",
			Description: "Podman container cache",
			OS:          []string{"linux"},
			Paths: []string{
				"~/.local/share/containers/cache",
			},
			Priority: 2,
		},
	}
}
