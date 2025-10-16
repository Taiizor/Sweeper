package cleaner

import (
	"fmt"
	"os"
	"path/filepath"
	"sync"
	"time"

	"github.com/Taiizor/Sweeper/internal/models"
)

// Config holds cleaner configuration
type Config struct {
	Force      bool
	Verbose    bool
	MaxWorkers int
}

// Cleaner is responsible for deleting temporary files
type Cleaner struct {
	config Config
	mu     sync.Mutex
	wg     sync.WaitGroup
}

// New creates a new Cleaner instance
func New(config Config) *Cleaner {
	if config.MaxWorkers <= 0 {
		config.MaxWorkers = 4 // Default to 4 workers
	}
	return &Cleaner{
		config: config,
	}
}

// Clean performs the cleaning operation on provided items
func (c *Cleaner) Clean(items []models.Item) (*models.CleanResult, error) {
	startTime := time.Now()
	result := &models.CleanResult{
		TotalItems:   len(items),
		SuccessItems: []models.Item{},
		FailedItems:  []models.Item{},
	}

	// Calculate total size
	for _, item := range items {
		result.TotalSize += item.Size
	}

	// Create worker pool
	itemChan := make(chan models.Item, len(items))
	resultChan := make(chan cleanItemResult, len(items))

	// Start workers
	for i := 0; i < c.config.MaxWorkers; i++ {
		c.wg.Add(1)
		go c.worker(itemChan, resultChan)
	}

	// Send items to workers
	for _, item := range items {
		itemChan <- item
	}
	close(itemChan)

	// Wait for workers to finish
	go func() {
		c.wg.Wait()
		close(resultChan)
	}()

	// Collect results
	for res := range resultChan {
		c.mu.Lock()
		if res.err != nil {
			res.item.Error = res.err
			result.FailedItems = append(result.FailedItems, res.item)
			result.FailureCount++
		} else {
			result.SuccessItems = append(result.SuccessItems, res.item)
			result.SuccessCount++
			result.FreedSize += res.item.Size
		}
		c.mu.Unlock()
	}

	result.Duration = time.Since(startTime)
	return result, nil
}

// cleanItemResult holds the result of cleaning a single item
type cleanItemResult struct {
	item models.Item
	err  error
}

// worker processes items from the channel
func (c *Cleaner) worker(items <-chan models.Item, results chan<- cleanItemResult) {
	defer c.wg.Done()

	for item := range items {
		err := c.cleanItem(item)
		results <- cleanItemResult{
			item: item,
			err:  err,
		}
	}
}

// cleanItem cleans a single item
func (c *Cleaner) cleanItem(item models.Item) error {
	// Check if item can be deleted
	if !item.CanBeDeleted && !c.config.Force {
		return fmt.Errorf("item is protected and cannot be deleted without force flag")
	}

	// Check if path exists
	if _, err := os.Stat(item.Path); os.IsNotExist(err) {
		return nil // Already deleted
	}

	// Delete the item
	if item.IsDirectory {
		return c.removeDirectory(item.Path)
	}
	return c.removeFile(item.Path)
}

// removeFile removes a single file
func (c *Cleaner) removeFile(path string) error {
	// Try to remove read-only attribute on Windows
	if err := makeWritable(path); err != nil && c.config.Verbose {
		fmt.Printf("Warning: Could not make %s writable: %v\n", path, err)
	}

	err := os.Remove(path)
	if err != nil && c.config.Force {
		// Force removal by changing permissions
		os.Chmod(path, 0777)
		err = os.Remove(path)
	}
	return err
}

// removeDirectory removes a directory and its contents
func (c *Cleaner) removeDirectory(path string) error {
	// First try simple removal
	err := os.RemoveAll(path)
	if err == nil {
		return nil
	}

	// If failed and force is enabled, try harder
	if c.config.Force {
		// Walk through directory and force permissions
		filepath.Walk(path, func(p string, info os.FileInfo, err error) error {
			if err == nil {
				os.Chmod(p, 0777)
			}
			return nil
		})

		// Try removal again
		err = os.RemoveAll(path)
	}

	return err
}

// makeWritable attempts to make a file writable (Windows-specific handling)
func makeWritable(path string) error {
	info, err := os.Stat(path)
	if err != nil {
		return err
	}

	// Make file writable
	mode := info.Mode()
	if mode&0200 == 0 {
		return os.Chmod(path, mode|0200)
	}

	return nil
}

// EstimateTime estimates the time required to clean the given items
func EstimateTime(items []models.Item) time.Duration {
	// Rough estimation: 100 files per second
	fileCount := len(items)
	seconds := fileCount / 100
	if seconds < 1 {
		seconds = 1
	}
	return time.Duration(seconds) * time.Second
}
