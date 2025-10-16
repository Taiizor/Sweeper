package cmd

import (
	"fmt"

	"github.com/Taiizor/Sweeper/internal/cleaner"
	"github.com/Taiizor/Sweeper/internal/scanner"
	"github.com/Taiizor/Sweeper/internal/ui"
	"github.com/fatih/color"
	"github.com/spf13/cobra"
)

var (
	targets   []string
	exclude   []string
	maxSize   string
	minAge    int
	recursive bool
)

// cleanCmd represents the clean command
var cleanCmd = &cobra.Command{
	Use:   "clean [targets...]",
	Short: "Clean temporary files from the system",
	Long: `Clean temporary files from various locations on the system.
	
You can specify specific targets (browser, system, npm, etc.) or clean all known locations.
Use --dry-run to preview what would be deleted without actually deleting anything.`,
	Example: `  # Clean all temporary files
  sweeper clean
  
  # Clean only browser cache
  sweeper clean browser
  
  # Clean multiple targets
  sweeper clean browser npm system
  
  # Preview what would be deleted
  sweeper clean --dry-run
  
  # Clean with specific exclusions
  sweeper clean --exclude="*.log,important*"`,
	RunE: runClean,
}

func init() {
	rootCmd.AddCommand(cleanCmd)

	cleanCmd.Flags().StringSliceVarP(&targets, "targets", "t", []string{"all"}, "specific targets to clean (browser, system, npm, etc.)")
	cleanCmd.Flags().StringSliceVarP(&exclude, "exclude", "e", []string{}, "patterns to exclude from cleaning")
	cleanCmd.Flags().StringVar(&maxSize, "max-size", "", "maximum file size to consider for deletion (e.g., 100MB, 1GB)")
	cleanCmd.Flags().IntVar(&minAge, "min-age", 0, "minimum age in days for files to be considered for deletion")
	cleanCmd.Flags().BoolVarP(&recursive, "recursive", "r", true, "recursively clean subdirectories")
}

func runClean(cmd *cobra.Command, args []string) error {
	// Merge args with targets flag
	if len(args) > 0 {
		targets = args
	}

	// Create UI presenter
	presenter := ui.NewPresenter(verbose)

	// Welcome message
	presenter.ShowHeader()

	if dryRun {
		color.Yellow("🔍 Running in DRY-RUN mode - no files will be deleted\n")
	}

	// Initialize scanner with configuration
	scannerConfig := scanner.Config{
		Targets:   targets,
		Exclude:   exclude,
		MaxSize:   maxSize,
		MinAge:    minAge,
		Recursive: recursive,
	}

	s := scanner.New(scannerConfig)

	// Scan for temporary files
	presenter.ShowProgress("Scanning for temporary files...")
	items, err := s.Scan()
	if err != nil {
		return fmt.Errorf("failed to scan: %w", err)
	}

	if len(items) == 0 {
		color.Green("✨ No temporary files found to clean!")
		return nil
	}

	// Display scan results
	presenter.ShowScanResults(items)

	// Ask for confirmation unless force flag is set
	if !dryRun && !force {
		if !presenter.ConfirmAction("Do you want to proceed with deletion?") {
			color.Yellow("❌ Operation cancelled by user")
			return nil
		}
	}

	// Clean files if not in dry-run mode
	if !dryRun {
		cleanerConfig := cleaner.Config{
			Force:   force,
			Verbose: verbose,
		}

		c := cleaner.New(cleanerConfig)

		presenter.ShowProgress("Cleaning temporary files...")
		results, err := c.Clean(items)
		if err != nil {
			return fmt.Errorf("failed to clean: %w", err)
		}

		// Show results
		presenter.ShowCleanResults(results)
	}

	return nil
}
