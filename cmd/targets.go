package cmd

import (
	"fmt"
	"strings"

	"github.com/Taiizor/Sweeper/internal/patterns"
	"github.com/fatih/color"
	"github.com/spf13/cobra"
)

// targetsCmd represents the targets command
var targetsCmd = &cobra.Command{
	Use:   "targets",
	Short: "Show available cleaning targets",
	Long:  `Display all available cleaning targets that can be used with the clean command.`,
	RunE:  runTargets,
}

func init() {
	rootCmd.AddCommand(targetsCmd)
}

func runTargets(cmd *cobra.Command, args []string) error {
	manager := patterns.NewManager()

	color.Cyan("Available Cleaning Targets:")
	fmt.Println(strings.Repeat("═", 60))

	targets := []struct {
		name        string
		description string
	}{
		{"all", "Clean all available targets"},
		{"system", "System temporary files and caches"},
		{"browser", "Web browser cache and temporary files"},
		{"dev", "Development build artifacts and object files"},
		{"npm", "Node.js/npm cache and node_modules"},
		{"pip", "Python/pip cache and virtual environments"},
		{"cargo", "Rust/Cargo build artifacts and cache"},
		{"maven", "Maven build artifacts and local repository"},
		{"gradle", "Gradle build artifacts and cache"},
		{"ide", "IDE cache and temporary files"},
		{"logs", "Application and system log files"},
		{"cache", "Various application caches"},
		{"docker", "Docker images, volumes and build cache"},
	}

	for _, target := range targets {
		fmt.Printf("  %s\n", color.GreenString("%-15s", target.name))
		fmt.Printf("    %s\n", target.description)
	}

	fmt.Println(strings.Repeat("═", 60))
	fmt.Println()
	color.Yellow("Usage examples:")
	fmt.Println("  sweeper clean                  # Clean default targets")
	fmt.Println("  sweeper clean all              # Clean all targets")
	fmt.Println("  sweeper clean browser npm      # Clean specific targets")
	fmt.Println("  sweeper list browser           # List files without cleaning")
	fmt.Println()

	return nil
}
