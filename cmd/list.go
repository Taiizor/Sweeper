package cmd

import (
	"fmt"

	"github.com/Taiizor/Sweeper/internal/scanner"
	"github.com/Taiizor/Sweeper/internal/ui"
	"github.com/spf13/cobra"
)

var (
	format  string
	sortBy  string
	groupBy string
	limit   int
)

// listCmd represents the list command
var listCmd = &cobra.Command{
	Use:   "list [targets...]",
	Short: "List temporary files without deleting them",
	Long: `List temporary files that would be cleaned without actually deleting them.
This command is useful for discovering what files Sweeper considers as temporary.`,
	Example: `  # List all temporary files
  sweeper list
  
  # List only browser cache files
  sweeper list browser
  
  # List files sorted by size
  sweeper list --sort=size
  
  # List files grouped by type
  sweeper list --group=type
  
  # Export list to JSON format
  sweeper list --format=json`,
	RunE: runList,
}

func init() {
	rootCmd.AddCommand(listCmd)

	listCmd.Flags().StringVar(&format, "format", "table", "output format (table, json, csv, yaml)")
	listCmd.Flags().StringVar(&sortBy, "sort", "size", "sort by field (size, name, date, type)")
	listCmd.Flags().StringVar(&groupBy, "group", "", "group by field (type, directory, extension)")
	listCmd.Flags().IntVar(&limit, "limit", 0, "limit number of results (0 for unlimited)")
	listCmd.Flags().StringSliceVarP(&targets, "targets", "t", []string{"all"}, "specific targets to list")
}

func runList(cmd *cobra.Command, args []string) error {
	// Merge args with targets flag
	if len(args) > 0 {
		targets = args
	}

	// Create UI presenter
	presenter := ui.NewPresenter(verbose)

	// Welcome message
	presenter.ShowHeader()

	// Initialize scanner
	scannerConfig := scanner.Config{
		Targets:   targets,
		Exclude:   exclude,
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
		presenter.Info("No temporary files found")
		return nil
	}

	// Apply sorting
	if sortBy != "" {
		items = scanner.SortItems(items, sortBy)
	}

	// Apply limit
	if limit > 0 && len(items) > limit {
		items = items[:limit]
	}

	// Display results based on format
	switch format {
	case "json":
		presenter.ShowJSON(items)
	case "csv":
		presenter.ShowCSV(items)
	case "yaml":
		presenter.ShowYAML(items)
	default:
		if groupBy != "" {
			grouped := scanner.GroupItems(items, groupBy)
			presenter.ShowGroupedTable(grouped)
		} else {
			presenter.ShowTable(items)
		}
	}

	// Show summary
	presenter.ShowSummary(items)

	return nil
}
