package ui

import (
	"encoding/csv"
	"encoding/json"
	"fmt"
	"os"
	"strings"
	"text/tabwriter"
	"time"

	"github.com/Taiizor/Sweeper/internal/models"
	"github.com/fatih/color"
	"github.com/schollz/progressbar/v3"
	"gopkg.in/yaml.v3"
)

// Presenter handles UI presentation
type Presenter struct {
	verbose     bool
	progressBar *progressbar.ProgressBar
}

// NewPresenter creates a new presenter instance
func NewPresenter(verbose bool) *Presenter {
	return &Presenter{
		verbose: verbose,
	}
}

// ShowHeader displays the application header
func (p *Presenter) ShowHeader() {
	color.Cyan("╔══════════════════════════════════════════╗")
	color.Cyan("║       SWEEPER - File Cleaner v0.1.0     ║")
	color.Cyan("╚══════════════════════════════════════════╝")
	fmt.Println()
}

// ShowProgress displays a progress message
func (p *Presenter) ShowProgress(message string) {
	color.Yellow("⏳ " + message)
}

// ShowScanResults displays scan results
func (p *Presenter) ShowScanResults(items []models.Item) {
	fmt.Println()
	color.Green("📊 Scan Results:")
	fmt.Println(strings.Repeat("-", 50))

	// Group by category
	categories := make(map[string][]models.Item)
	var totalSize int64

	for _, item := range items {
		categories[item.Category] = append(categories[item.Category], item)
		totalSize += item.Size
	}

	// Display by category
	for category, catItems := range categories {
		var catSize int64
		for _, item := range catItems {
			catSize += item.Size
		}

		fmt.Printf("  %s: %d files (%s)\n",
			color.YellowString(category),
			len(catItems),
			FormatSize(catSize))
	}

	fmt.Println(strings.Repeat("-", 50))
	fmt.Printf("  Total: %s files (%s)\n",
		color.CyanString("%d", len(items)),
		color.CyanString(FormatSize(totalSize)))
	fmt.Println()
}

// ShowCleanResults displays cleaning results
func (p *Presenter) ShowCleanResults(result *models.CleanResult) {
	fmt.Println()
	color.Green("✅ Cleaning Complete!")
	fmt.Println(strings.Repeat("=", 50))

	// Success stats
	if result.SuccessCount > 0 {
		color.Green("  ✓ Successfully cleaned: %d files (%s)",
			result.SuccessCount,
			FormatSize(result.FreedSize))
	}

	// Failure stats
	if result.FailureCount > 0 {
		color.Red("  ✗ Failed to clean: %d files", result.FailureCount)

		if p.verbose && len(result.FailedItems) > 0 {
			fmt.Println("\n  Failed items:")
			for _, item := range result.FailedItems[:min(5, len(result.FailedItems))] {
				fmt.Printf("    - %s: %v\n", item.Path, item.Error)
			}
		}
	}

	// Duration
	fmt.Printf("\n  ⏱️  Duration: %s\n", result.Duration.Round(time.Second))
	fmt.Println(strings.Repeat("=", 50))
	fmt.Println()
}

// ShowTable displays items in a table format
func (p *Presenter) ShowTable(items []models.Item) {
	w := tabwriter.NewWriter(os.Stdout, 0, 0, 2, ' ', 0)
	fmt.Fprintln(w, "Path\tSize\tType\tModified")
	fmt.Fprintln(w, "----\t----\t----\t--------")

	for _, item := range items {
		path := item.Path
		if len(path) > 50 {
			path = "..." + path[len(path)-47:]
		}
		fmt.Fprintf(w, "%s\t%s\t%s\t%s\n",
			path,
			FormatSize(item.Size),
			item.Type,
			item.ModTime.Format("2006-01-02 15:04"))
	}

	w.Flush()
}

// ShowGroupedTable displays items grouped in a table format
func (p *Presenter) ShowGroupedTable(grouped map[string][]models.Item) {
	for group, items := range grouped {
		color.Yellow("\n[%s]", group)
		p.ShowTable(items)
	}
}

// ShowJSON displays items in JSON format
func (p *Presenter) ShowJSON(items []models.Item) {
	encoder := json.NewEncoder(os.Stdout)
	encoder.SetIndent("", "  ")
	encoder.Encode(items)
}

// ShowCSV displays items in CSV format
func (p *Presenter) ShowCSV(items []models.Item) {
	writer := csv.NewWriter(os.Stdout)
	defer writer.Flush()

	// Write header
	writer.Write([]string{"Path", "Size", "Type", "Category", "Modified"})

	// Write data
	for _, item := range items {
		writer.Write([]string{
			item.Path,
			fmt.Sprintf("%d", item.Size),
			item.Type,
			item.Category,
			item.ModTime.Format(time.RFC3339),
		})
	}
}

// ShowYAML displays items in YAML format
func (p *Presenter) ShowYAML(items []models.Item) {
	encoder := yaml.NewEncoder(os.Stdout)
	encoder.Encode(items)
}

// ShowSummary displays a summary of items
func (p *Presenter) ShowSummary(items []models.Item) {
	var totalSize int64
	categories := make(map[string]int)

	for _, item := range items {
		totalSize += item.Size
		categories[item.Category]++
	}

	fmt.Println()
	color.Cyan("Summary:")
	fmt.Printf("  Total files: %d\n", len(items))
	fmt.Printf("  Total size: %s\n", FormatSize(totalSize))
	fmt.Printf("  Categories: %d\n", len(categories))
}

// ConfirmAction asks for user confirmation
func (p *Presenter) ConfirmAction(message string) bool {
	fmt.Print(color.YellowString("\n%s [y/N]: ", message))

	var response string
	fmt.Scanln(&response)

	response = strings.ToLower(strings.TrimSpace(response))
	return response == "y" || response == "yes"
}

// Info displays an info message
func (p *Presenter) Info(message string) {
	color.Blue("ℹ️  " + message)
}

// Success displays a success message
func (p *Presenter) Success(message string) {
	color.Green("✅ " + message)
}

// Warning displays a warning message
func (p *Presenter) Warning(message string) {
	color.Yellow("⚠️  " + message)
}

// Error displays an error message
func (p *Presenter) Error(message string) {
	color.Red("❌ " + message)
}

// StartProgress starts a progress bar
func (p *Presenter) StartProgress(total int, description string) {
	p.progressBar = progressbar.NewOptions(total,
		progressbar.OptionEnableColorCodes(true),
		progressbar.OptionSetDescription(description),
		progressbar.OptionShowCount(),
		progressbar.OptionShowIts(),
		progressbar.OptionSetItsString("files"),
		progressbar.OptionSetTheme(progressbar.Theme{
			Saucer:        "[green]█[reset]",
			SaucerPadding: " ",
			BarStart:      "[",
			BarEnd:        "]",
		}))
}

// UpdateProgress updates the progress bar
func (p *Presenter) UpdateProgress(delta int) {
	if p.progressBar != nil {
		p.progressBar.Add(delta)
	}
}

// FinishProgress finishes the progress bar
func (p *Presenter) FinishProgress() {
	if p.progressBar != nil {
		p.progressBar.Finish()
		fmt.Println()
		p.progressBar = nil
	}
}

// FormatSize formats a size in bytes to a human-readable format
func FormatSize(bytes int64) string {
	const unit = 1024
	if bytes < unit {
		return fmt.Sprintf("%d B", bytes)
	}

	div, exp := int64(unit), 0
	for n := bytes / unit; n >= unit; n /= unit {
		div *= unit
		exp++
	}

	units := []string{"KB", "MB", "GB", "TB"}
	return fmt.Sprintf("%.2f %s", float64(bytes)/float64(div), units[exp])
}

// FormatDuration formats a duration to a human-readable format
func FormatDuration(d time.Duration) string {
	if d < time.Second {
		return fmt.Sprintf("%dms", d.Milliseconds())
	}
	if d < time.Minute {
		return fmt.Sprintf("%.1fs", d.Seconds())
	}
	if d < time.Hour {
		return fmt.Sprintf("%.1fm", d.Minutes())
	}
	return fmt.Sprintf("%.1fh", d.Hours())
}

// min returns the minimum of two integers
func min(a, b int) int {
	if a < b {
		return a
	}
	return b
}
