package patterns

import (
	"runtime"
	"strings"

	"github.com/Taiizor/Sweeper/internal/models"
)

// Manager manages cleaning patterns
type Manager struct {
	patterns map[string][]models.Pattern
	os       string
}

// NewManager creates a new patterns manager
func NewManager() *Manager {
	m := &Manager{
		patterns: make(map[string][]models.Pattern),
		os:       runtime.GOOS,
	}
	m.loadPatterns()
	return m
}

// GetPatterns returns patterns for the specified targets
func (m *Manager) GetPatterns(targets []string) []models.Pattern {
	var result []models.Pattern

	// Check if "all" is in targets
	for _, target := range targets {
		if strings.ToLower(target) == "all" {
			return m.getAllPatterns()
		}
	}

	// Collect patterns for specific targets
	seen := make(map[string]bool)
	for _, target := range targets {
		target = strings.ToLower(target)
		if patterns, ok := m.patterns[target]; ok {
			for _, pattern := range patterns {
				// Avoid duplicates
				if !seen[pattern.Name] {
					result = append(result, pattern)
					seen[pattern.Name] = true
				}
			}
		}
	}

	return result
}

// getAllPatterns returns all available patterns for the current OS
func (m *Manager) getAllPatterns() []models.Pattern {
	var result []models.Pattern
	seen := make(map[string]bool)

	for _, patterns := range m.patterns {
		for _, pattern := range patterns {
			// Check if pattern is compatible with current OS
			if m.isCompatibleOS(pattern) && !seen[pattern.Name] {
				result = append(result, pattern)
				seen[pattern.Name] = true
			}
		}
	}

	return result
}

// isCompatibleOS checks if a pattern is compatible with the current OS
func (m *Manager) isCompatibleOS(pattern models.Pattern) bool {
	if len(pattern.OS) == 0 {
		return true // No OS restriction means all OS
	}

	for _, os := range pattern.OS {
		if os == m.os || os == "all" {
			return true
		}
	}

	return false
}

// GetAvailableTargets returns list of available target names
func (m *Manager) GetAvailableTargets() []string {
	targets := []string{"all"}
	for target := range m.patterns {
		targets = append(targets, target)
	}
	return targets
}

// loadPatterns loads all cleaning patterns
func (m *Manager) loadPatterns() {
	// System patterns
	m.patterns["system"] = getSystemPatterns()

	// Browser patterns
	m.patterns["browser"] = getBrowserPatterns()

	// Development patterns
	m.patterns["dev"] = getDevelopmentPatterns()

	// Package manager patterns
	m.patterns["npm"] = getNpmPatterns()
	m.patterns["pip"] = getPipPatterns()
	m.patterns["cargo"] = getCargoPatterns()
	m.patterns["maven"] = getMavenPatterns()
	m.patterns["gradle"] = getGradlePatterns()

	// IDE patterns
	m.patterns["ide"] = getIDEPatterns()

	// Log patterns
	m.patterns["logs"] = getLogPatterns()

	// Cache patterns
	m.patterns["cache"] = getCachePatterns()

	// Docker patterns
	m.patterns["docker"] = getDockerPatterns()
}
