package cmd

import (
	"encoding/json"
	"fmt"
	"os"
	"path/filepath"

	"github.com/Taiizor/Sweeper/internal/models"
	"github.com/Taiizor/Sweeper/internal/ui"
	"github.com/fatih/color"
	"github.com/spf13/cobra"
)

// statsCmd represents the stats command
var statsCmd = &cobra.Command{
	Use:   "stats",
	Short: "Show cleaning statistics",
	Long: `Display statistics about previous cleaning operations including
total space freed, most frequently cleaned locations, and cleaning history.`,
	RunE: runStats,
}

func init() {
	rootCmd.AddCommand(statsCmd)
}

func runStats(cmd *cobra.Command, args []string) error {
	// Create UI presenter
	presenter := ui.NewPresenter(verbose)

	// Load statistics
	stats, err := loadStatistics()
	if err != nil {
		if os.IsNotExist(err) {
			color.Yellow("No statistics available yet. Run 'sweeper clean' to generate statistics.")
			return nil
		}
		return fmt.Errorf("failed to load statistics: %w", err)
	}

	// Display statistics
	displayStatistics(stats, presenter)

	return nil
}

func loadStatistics() (*models.Statistics, error) {
	statsFile := filepath.Join(cfg.DataDir, "stats.json")

	data, err := os.ReadFile(statsFile)
	if err != nil {
		return nil, err
	}

	var stats models.Statistics
	if err := json.Unmarshal(data, &stats); err != nil {
		return nil, err
	}

	return &stats, nil
}

func displayStatistics(stats *models.Statistics, presenter *ui.Presenter) {
	presenter.ShowHeader()

	color.Cyan("📊 Cleaning Statistics")
	fmt.Println("═══════════════════════════════════════════")

	fmt.Printf("  Last Run:        %s\n", stats.LastRun.Format("2006-01-02 15:04:05"))
	fmt.Printf("  Total Runs:      %d\n", stats.TotalRuns)
	fmt.Printf("  Total Cleaned:   %d files\n", stats.TotalCleaned)
	fmt.Printf("  Total Freed:     %s\n", ui.FormatSize(stats.TotalFreed))
	fmt.Printf("  Average Size:    %s per run\n", ui.FormatSize(stats.AverageSize))

	if len(stats.MostCommon) > 0 {
		fmt.Println("\n  Most Common Targets:")
		for i, target := range stats.MostCommon {
			if i >= 5 {
				break
			}
			fmt.Printf("    %d. %s\n", i+1, target)
		}
	}

	fmt.Println("═══════════════════════════════════════════")
}

func saveStatistics(result *models.CleanResult) error {
	statsFile := filepath.Join(cfg.DataDir, "stats.json")

	// Load existing statistics
	var stats models.Statistics
	if data, err := os.ReadFile(statsFile); err == nil {
		json.Unmarshal(data, &stats)
	}

	// Update statistics
	stats.LastRun = result.Duration
	stats.TotalRuns++
	stats.TotalCleaned += int64(result.SuccessCount)
	stats.TotalFreed += result.FreedSize

	if stats.TotalRuns > 0 {
		stats.AverageSize = stats.TotalFreed / int64(stats.TotalRuns)
	}

	// Save statistics
	data, err := json.MarshalIndent(stats, "", "  ")
	if err != nil {
		return err
	}

	return os.WriteFile(statsFile, data, 0644)
}
