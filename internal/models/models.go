package models

import (
	"time"
)

// Item represents a file or directory that can be cleaned
type Item struct {
	Path         string    `json:"path" yaml:"path"`
	Size         int64     `json:"size" yaml:"size"`
	ModTime      time.Time `json:"modified" yaml:"modified"`
	Category     string    `json:"category" yaml:"category"`
	Type         string    `json:"type" yaml:"type"`
	Description  string    `json:"description" yaml:"description"`
	IsDirectory  bool      `json:"is_directory" yaml:"is_directory"`
	CanBeDeleted bool      `json:"can_be_deleted" yaml:"can_be_deleted"`
	Error        error     `json:"error,omitempty" yaml:"error,omitempty"`
}

// Pattern represents a cleaning pattern for specific file types
type Pattern struct {
	Name        string   `json:"name" yaml:"name"`
	Category    string   `json:"category" yaml:"category"`
	Description string   `json:"description" yaml:"description"`
	OS          []string `json:"os" yaml:"os"`
	Paths       []string `json:"paths" yaml:"paths"`
	Extensions  []string `json:"extensions,omitempty" yaml:"extensions,omitempty"`
	Globs       []string `json:"globs,omitempty" yaml:"globs,omitempty"`
	Protected   bool     `json:"protected" yaml:"protected"`
	Priority    int      `json:"priority" yaml:"priority"`
}

// CleanResult represents the result of a cleaning operation
type CleanResult struct {
	TotalItems    int           `json:"total_items" yaml:"total_items"`
	SuccessCount  int           `json:"success_count" yaml:"success_count"`
	FailureCount  int           `json:"failure_count" yaml:"failure_count"`
	TotalSize     int64         `json:"total_size" yaml:"total_size"`
	FreedSize     int64         `json:"freed_size" yaml:"freed_size"`
	Duration      time.Duration `json:"duration" yaml:"duration"`
	FailedItems   []Item        `json:"failed_items,omitempty" yaml:"failed_items,omitempty"`
	SuccessItems  []Item        `json:"success_items,omitempty" yaml:"success_items,omitempty"`
}

// Statistics represents cleaning statistics
type Statistics struct {
	LastRun       time.Time `json:"last_run" yaml:"last_run"`
	TotalRuns     int       `json:"total_runs" yaml:"total_runs"`
	TotalCleaned  int64     `json:"total_cleaned" yaml:"total_cleaned"`
	TotalFreed    int64     `json:"total_freed" yaml:"total_freed"`
	AverageSize   int64     `json:"average_size" yaml:"average_size"`
	MostCommon    []string  `json:"most_common" yaml:"most_common"`
}

// Target represents a cleaning target
type Target struct {
	Name        string   `json:"name" yaml:"name"`
	DisplayName string   `json:"display_name" yaml:"display_name"`
	Description string   `json:"description" yaml:"description"`
	Patterns    []string `json:"patterns" yaml:"patterns"`
	Enabled     bool     `json:"enabled" yaml:"enabled"`
}
