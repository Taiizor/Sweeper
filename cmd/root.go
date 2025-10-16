package cmd

import (
	"fmt"
	"os"

	"github.com/Taiizor/Sweeper/internal/config"
	"github.com/Taiizor/Sweeper/internal/i18n"
	"github.com/spf13/cobra"
	"github.com/spf13/viper"
)

var (
	cfgFile   string
	cfg       *config.Config
	verbose   bool
	dryRun    bool
	force     bool
	lang      string
	translate i18n.Translator
)

// rootCmd represents the base command when called without any subcommands
var rootCmd = &cobra.Command{
	Use:   "sweeper",
	Short: "A powerful CLI tool for cleaning temporary files across different operating systems",
	Long: `Sweeper is a professional CLI tool designed to identify and remove temporary files
created by various applications and operating systems.

It provides a safe, efficient, and cross-platform solution for reclaiming disk space
by removing unnecessary temporary files while preserving important data.`,
	Version: "0.1.0",
}

// Execute adds all child commands to the root command and sets flags appropriately.
func Execute() error {
	return rootCmd.Execute()
}

func init() {
	cobra.OnInitialize(initConfig)

	// Global flags
	rootCmd.PersistentFlags().StringVar(&cfgFile, "config", "", "config file (default is $HOME/.sweeper.yaml)")
	rootCmd.PersistentFlags().BoolVarP(&verbose, "verbose", "v", false, "verbose output")
	rootCmd.PersistentFlags().BoolVar(&dryRun, "dry-run", false, "preview what would be deleted without actually deleting")
	rootCmd.PersistentFlags().BoolVarP(&force, "force", "f", false, "force deletion without confirmation")
	rootCmd.PersistentFlags().StringVar(&lang, "lang", "en", "language for messages (en, tr, es, etc.)")

	// Bind flags to viper
	viper.BindPFlag("verbose", rootCmd.PersistentFlags().Lookup("verbose"))
	viper.BindPFlag("dry_run", rootCmd.PersistentFlags().Lookup("dry-run"))
	viper.BindPFlag("force", rootCmd.PersistentFlags().Lookup("force"))
	viper.BindPFlag("language", rootCmd.PersistentFlags().Lookup("lang"))
}

// initConfig reads in config file and ENV variables if set.
func initConfig() {
	if cfgFile != "" {
		viper.SetConfigFile(cfgFile)
	} else {
		home, err := os.UserHomeDir()
		if err != nil {
			fmt.Println(err)
			os.Exit(1)
		}

		viper.AddConfigPath(home)
		viper.SetConfigType("yaml")
		viper.SetConfigName(".sweeper")
	}

	viper.AutomaticEnv()
	viper.SetEnvPrefix("SWEEPER")

	// Read config file if it exists
	if err := viper.ReadInConfig(); err == nil {
		if verbose {
			fmt.Println("Using config file:", viper.ConfigFileUsed())
		}
	}

	// Initialize configuration
	cfg = config.New()

	// Initialize translator
	translate = i18n.New(lang)
}
