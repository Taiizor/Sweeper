package main

import (
	"os"

	"github.com/Taiizor/Sweeper/cmd"
	"github.com/Taiizor/Sweeper/internal/config"
	"github.com/Taiizor/Sweeper/internal/logger"
)

func main() {
	// Initialize configuration
	cfg := config.New()

	// Initialize logger
	log := logger.New(cfg.LogLevel)
	defer log.Sync()

	// Execute root command
	if err := cmd.Execute(); err != nil {
		log.Error("Application failed", "error", err)
		os.Exit(1)
	}
}
