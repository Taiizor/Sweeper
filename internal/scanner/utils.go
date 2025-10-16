package scanner

import (
	"sort"
	"strings"

	"github.com/Taiizor/Sweeper/internal/models"
)

// SortItems sorts items by the specified field
func SortItems(items []models.Item, sortBy string) []models.Item {
	sorted := make([]models.Item, len(items))
	copy(sorted, items)

	switch strings.ToLower(sortBy) {
	case "size":
		sort.Slice(sorted, func(i, j int) bool {
			return sorted[i].Size > sorted[j].Size
		})
	case "name":
		sort.Slice(sorted, func(i, j int) bool {
			return sorted[i].Path < sorted[j].Path
		})
	case "date":
		sort.Slice(sorted, func(i, j int) bool {
			return sorted[i].ModTime.After(sorted[j].ModTime)
		})
	case "type":
		sort.Slice(sorted, func(i, j int) bool {
			return sorted[i].Type < sorted[j].Type
		})
	}

	return sorted
}

// GroupItems groups items by the specified field
func GroupItems(items []models.Item, groupBy string) map[string][]models.Item {
	grouped := make(map[string][]models.Item)

	for _, item := range items {
		var key string

		switch strings.ToLower(groupBy) {
		case "type":
			key = item.Type
		case "category":
			key = item.Category
		case "directory":
			key = getDirectory(item.Path)
		case "extension":
			key = getExtension(item.Path)
		default:
			key = "unknown"
		}

		grouped[key] = append(grouped[key], item)
	}

	return grouped
}

// getDirectory extracts the directory from a path
func getDirectory(path string) string {
	lastSlash := strings.LastIndex(path, "/")
	lastBackslash := strings.LastIndex(path, "\\")

	separator := lastSlash
	if lastBackslash > lastSlash {
		separator = lastBackslash
	}

	if separator == -1 {
		return "."
	}

	return path[:separator]
}

// getExtension extracts the file extension from a path
func getExtension(path string) string {
	lastDot := strings.LastIndex(path, ".")
	if lastDot == -1 {
		return "no_extension"
	}

	ext := path[lastDot:]
	if ext == "" {
		return "no_extension"
	}

	return ext
}
