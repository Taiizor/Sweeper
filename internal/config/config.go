package config

import (
	"os"
	"path/filepath"
	"runtime"

	"github.com/spf13/viper"
)

// Config holds the application configuration
type Config struct {
	// General settings
	Language    string `mapstructure:"language"`
	LogLevel    string `mapstructure:"log_level"`
	LogFile     string `mapstructure:"log_file"`
	MaxWorkers  int    `mapstructure:"max_workers"`
	
	// Cleaning settings
	DefaultTargets []string `mapstructure:"default_targets"`
	AutoConfirm    bool     `mapstructure:"auto_confirm"`
	KeepRecent     int      `mapstructure:"keep_recent_days"`
	MinFileAge     int      `mapstructure:"min_file_age"`
	MaxFileSize    string   `mapstructure:"max_file_size"`
	
	// Safety settings
	SafeMode       bool     `mapstructure:"safe_mode"`
	ProtectedPaths []string `mapstructure:"protected_paths"`
	ExcludePatterns []string `mapstructure:"exclude_patterns"`
	
	// Display settings
	ColorOutput    bool   `mapstructure:"color_output"`
	ProgressBar    bool   `mapstructure:"progress_bar"`
	OutputFormat   string `mapstructure:"output_format"`
	
	// Statistics
	EnableStats    bool   `mapstructure:"enable_stats"`
	StatsFile      string `mapstructure:"stats_file"`
	
	// Paths
	ConfigDir      string
	DataDir        string
	CacheDir       string
}

// New creates a new configuration with defaults
func New() *Config {
	cfg := &Config{
		Language:       getDefaultLanguage(),
		LogLevel:       "info",
		MaxWorkers:     runtime.NumCPU(),
		DefaultTargets: []string{"browser", "system"},
		ColorOutput:    true,
		ProgressBar:    true,
		OutputFormat:   "table",
		SafeMode:       true,
		EnableStats:    true,
	}

	// Set up paths
	cfg.setupPaths()
	
	// Load from viper if available
	cfg.loadFromViper()
	
	return cfg
}

// setupPaths sets up application directories
func (c *Config) setupPaths() {
	homeDir, err := os.UserHomeDir()
	if err != nil {
		homeDir = "."
	}

	// Set config directory
	if runtime.GOOS == "windows" {
		c.ConfigDir = filepath.Join(os.Getenv("APPDATA"), "sweeper")
	} else {
		c.ConfigDir = filepath.Join(homeDir, ".config", "sweeper")
	}

	// Set data directory
	if runtime.GOOS == "windows" {
		c.DataDir = filepath.Join(os.Getenv("LOCALAPPDATA"), "sweeper")
	} else if runtime.GOOS == "darwin" {
		c.DataDir = filepath.Join(homeDir, "Library", "Application Support", "sweeper")
	} else {
		c.DataDir = filepath.Join(homeDir, ".local", "share", "sweeper")
	}

	// Set cache directory
	if runtime.GOOS == "windows" {
		c.CacheDir = filepath.Join(os.Getenv("LOCALAPPDATA"), "sweeper", "cache")
	} else if runtime.GOOS == "darwin" {
		c.CacheDir = filepath.Join(homeDir, "Library", "Caches", "sweeper")
	} else {
		c.CacheDir = filepath.Join(homeDir, ".cache", "sweeper")
	}

	// Set default paths
	c.LogFile = filepath.Join(c.DataDir, "sweeper.log")
	c.StatsFile = filepath.Join(c.DataDir, "stats.json")

	// Create directories if they don't exist
	os.MkdirAll(c.ConfigDir, 0755)
	os.MkdirAll(c.DataDir, 0755)
	os.MkdirAll(c.CacheDir, 0755)
}

// loadFromViper loads configuration from viper
func (c *Config) loadFromViper() {
	// Language
	if viper.IsSet("language") {
		c.Language = viper.GetString("language")
	}
	
	// Log settings
	if viper.IsSet("log_level") {
		c.LogLevel = viper.GetString("log_level")
	}
	if viper.IsSet("log_file") {
		c.LogFile = viper.GetString("log_file")
	}
	
	// Worker settings
	if viper.IsSet("max_workers") {
		c.MaxWorkers = viper.GetInt("max_workers")
	}
	
	// Cleaning settings
	if viper.IsSet("default_targets") {
		c.DefaultTargets = viper.GetStringSlice("default_targets")
	}
	if viper.IsSet("auto_confirm") {
		c.AutoConfirm = viper.GetBool("auto_confirm")
	}
	if viper.IsSet("keep_recent_days") {
		c.KeepRecent = viper.GetInt("keep_recent_days")
	}
	
	// Safety settings
	if viper.IsSet("safe_mode") {
		c.SafeMode = viper.GetBool("safe_mode")
	}
	if viper.IsSet("protected_paths") {
		c.ProtectedPaths = viper.GetStringSlice("protected_paths")
	}
	if viper.IsSet("exclude_patterns") {
		c.ExcludePatterns = viper.GetStringSlice("exclude_patterns")
	}
	
	// Display settings
	if viper.IsSet("color_output") {
		c.ColorOutput = viper.GetBool("color_output")
	}
	if viper.IsSet("progress_bar") {
		c.ProgressBar = viper.GetBool("progress_bar")
	}
	if viper.IsSet("output_format") {
		c.OutputFormat = viper.GetString("output_format")
	}
	
	// Statistics
	if viper.IsSet("enable_stats") {
		c.EnableStats = viper.GetBool("enable_stats")
	}
}

// getDefaultLanguage returns the default language based on system locale
func getDefaultLanguage() string {
	// Check environment variables
	lang := os.Getenv("LANG")
	if lang == "" {
		lang = os.Getenv("LANGUAGE")
	}
	
	// Parse language code
	if len(lang) >= 2 {
		switch lang[:2] {
		case "tr":
			return "tr"
		case "es":
			return "es"
		case "fr":
			return "fr"
		case "de":
			return "de"
		case "ja":
			return "ja"
		case "zh":
			return "zh"
		case "ru":
			return "ru"
		default:
			return "en"
		}
	}
	
	return "en"
}

// Save saves the configuration to file
func (c *Config) Save() error {
	viper.Set("language", c.Language)
	viper.Set("log_level", c.LogLevel)
	viper.Set("log_file", c.LogFile)
	viper.Set("max_workers", c.MaxWorkers)
	viper.Set("default_targets", c.DefaultTargets)
	viper.Set("auto_confirm", c.AutoConfirm)
	viper.Set("keep_recent_days", c.KeepRecent)
	viper.Set("safe_mode", c.SafeMode)
	viper.Set("protected_paths", c.ProtectedPaths)
	viper.Set("exclude_patterns", c.ExcludePatterns)
	viper.Set("color_output", c.ColorOutput)
	viper.Set("progress_bar", c.ProgressBar)
	viper.Set("output_format", c.OutputFormat)
	viper.Set("enable_stats", c.EnableStats)

	configFile := filepath.Join(c.ConfigDir, "config.yaml")
	return viper.WriteConfigAs(configFile)
}

// Validate validates the configuration
func (c *Config) Validate() error {
	if c.MaxWorkers <= 0 {
		c.MaxWorkers = 1
	}
	
	if c.OutputFormat == "" {
		c.OutputFormat = "table"
	}
	
	return nil
}
