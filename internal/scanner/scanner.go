package scanner

import (
	"fmt"
	"os"
	"path/filepath"
	"strings"
	"time"

	"github.com/Taiizor/Sweeper/internal/models"
	"github.com/Taiizor/Sweeper/internal/patterns"
)

// Config holds scanner configuration
type Config struct {
	Targets   []string
	Exclude   []string
	MaxSize   string
	MinAge    int
	Recursive bool
}

// Scanner is responsible for finding temporary files
type Scanner struct {
	config    Config
	patterns  *patterns.Manager
	items     []models.Item
	totalSize int64
}

// New creates a new Scanner instance
func New(config Config) *Scanner {
	return &Scanner{
		config:   config,
		patterns: patterns.NewManager(),
		items:    []models.Item{},
	}
}

// Scan performs the scanning operation
func (s *Scanner) Scan() ([]models.Item, error) {
	// Reset items
	s.items = []models.Item{}
	s.totalSize = 0

	// Get target patterns based on requested targets
	targetPatterns := s.patterns.GetPatterns(s.config.Targets)

	// Scan each pattern
	for _, pattern := range targetPatterns {
		if err := s.scanPattern(pattern); err != nil {
			// Log error but continue scanning
			fmt.Printf("Warning: Failed to scan %s: %v\n", pattern.Name, err)
		}
	}

	return s.items, nil
}

// scanPattern scans a specific pattern
func (s *Scanner) scanPattern(pattern models.Pattern) error {
	// Expand environment variables in paths
	expandedPaths := s.expandPaths(pattern.Paths)

	for _, path := range expandedPaths {
		// Check if path exists
		if _, err := os.Stat(path); os.IsNotExist(err) {
			continue
		}

		// Scan the path
		if err := s.scanPath(path, pattern); err != nil {
			return err
		}
	}

	return nil
}

// expandPaths expands environment variables and user home directory
func (s *Scanner) expandPaths(paths []string) []string {
	expanded := []string{}

	for _, path := range paths {
		// Expand environment variables
		path = os.ExpandEnv(path)

		// Expand ~ to user home directory
		if strings.HasPrefix(path, "~") {
			if home, err := os.UserHomeDir(); err == nil {
				path = filepath.Join(home, path[1:])
			}
		}

		expanded = append(expanded, path)
	}

	return expanded
}

// scanPath scans a specific path for temporary files
func (s *Scanner) scanPath(path string, pattern models.Pattern) error {
	// Check if path is excluded
	if s.isExcluded(path) {
		return nil
	}

	info, err := os.Stat(path)
	if err != nil {
		return err
	}

	// If it's a file, process it directly
	if !info.IsDir() {
		if s.shouldIncludeFile(path, info, pattern) {
			item := s.createItem(path, info, pattern)
			s.items = append(s.items, item)
			s.totalSize += item.Size
		}
		return nil
	}

	// If it's a directory, process recursively if needed
	if !s.config.Recursive && path != pattern.Paths[0] {
		return nil
	}

	// Walk through directory
	return filepath.Walk(path, func(filePath string, fileInfo os.FileInfo, err error) error {
		if err != nil {
			return nil // Skip errors, continue walking
		}

		// Skip if excluded
		if s.isExcluded(filePath) {
			if fileInfo.IsDir() {
				return filepath.SkipDir
			}
			return nil
		}

		// Check if file should be included
		if !fileInfo.IsDir() && s.shouldIncludeFile(filePath, fileInfo, pattern) {
			item := s.createItem(filePath, fileInfo, pattern)
			s.items = append(s.items, item)
			s.totalSize += item.Size
		}

		return nil
	})
}

// isExcluded checks if a path matches any exclusion pattern
func (s *Scanner) isExcluded(path string) bool {
	for _, excludePattern := range s.config.Exclude {
		matched, _ := filepath.Match(excludePattern, filepath.Base(path))
		if matched {
			return true
		}
	}
	return false
}

// shouldIncludeFile determines if a file should be included in results
func (s *Scanner) shouldIncludeFile(path string, info os.FileInfo, pattern models.Pattern) bool {
	// Check age if specified
	if s.config.MinAge > 0 {
		age := time.Since(info.ModTime()).Hours() / 24
		if age < float64(s.config.MinAge) {
			return false
		}
	}

	// Check size if specified
	if s.config.MaxSize != "" {
		maxSize := s.parseSize(s.config.MaxSize)
		if maxSize > 0 && info.Size() > maxSize {
			return false
		}
	}

	// Check if file matches pattern extensions
	if len(pattern.Extensions) > 0 {
		ext := strings.ToLower(filepath.Ext(path))
		matched := false
		for _, patternExt := range pattern.Extensions {
			if ext == patternExt {
				matched = true
				break
			}
		}
		if !matched {
			return false
		}
	}

	// Check if file matches pattern globs
	if len(pattern.Globs) > 0 {
		matched := false
		for _, glob := range pattern.Globs {
			if m, _ := filepath.Match(glob, filepath.Base(path)); m {
				matched = true
				break
			}
		}
		if !matched {
			return false
		}
	}

	return true
}

// createItem creates an Item from file info
func (s *Scanner) createItem(path string, info os.FileInfo, pattern models.Pattern) models.Item {
	return models.Item{
		Path:         path,
		Size:         info.Size(),
		ModTime:      info.ModTime(),
		Category:     pattern.Category,
		Type:         pattern.Name,
		Description:  pattern.Description,
		IsDirectory:  info.IsDir(),
		CanBeDeleted: !pattern.Protected,
	}
}

// parseSize parses size string (e.g., "100MB", "1GB") to bytes
func (s *Scanner) parseSize(sizeStr string) int64 {
	sizeStr = strings.ToUpper(strings.TrimSpace(sizeStr))

	multipliers := map[string]int64{
		"B":  1,
		"KB": 1024,
		"MB": 1024 * 1024,
		"GB": 1024 * 1024 * 1024,
		"TB": 1024 * 1024 * 1024 * 1024,
	}

	for suffix, multiplier := range multipliers {
		if strings.HasSuffix(sizeStr, suffix) {
			numStr := strings.TrimSuffix(sizeStr, suffix)
			var num float64
			fmt.Sscanf(numStr, "%f", &num)
			return int64(num * float64(multiplier))
		}
	}

	return 0
}

// GetTotalSize returns the total size of scanned items
func (s *Scanner) GetTotalSize() int64 {
	return s.totalSize
}

// GetItemCount returns the number of scanned items
func (s *Scanner) GetItemCount() int {
	return len(s.items)
}
