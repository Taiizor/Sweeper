package patterns

import "github.com/Taiizor/Sweeper/internal/models"

// getIDEPatterns returns IDE-related temporary file patterns
func getIDEPatterns() []models.Pattern {
	return []models.Pattern{
		// Visual Studio Code
		{
			Name:        "vscode_cache",
			Category:    "ide",
			Description: "Visual Studio Code cache and temporary files",
			OS:          []string{"windows"},
			Paths: []string{
				"%APPDATA%\\Code\\Cache",
				"%APPDATA%\\Code\\CachedData",
				"%APPDATA%\\Code\\logs",
			},
			Priority: 1,
		},
		{
			Name:        "vscode_cache_unix",
			Category:    "ide",
			Description: "Visual Studio Code cache (Linux/macOS)",
			OS:          []string{"linux", "darwin"},
			Paths: []string{
				"~/.config/Code/Cache",
				"~/.config/Code/CachedData",
				"~/Library/Application Support/Code/Cache",
				"~/Library/Application Support/Code/CachedData",
			},
			Priority: 1,
		},
		{
			Name:        "vscode_workspaces",
			Category:    "ide",
			Description: "VS Code workspace storage",
			OS:          []string{"windows", "linux", "darwin"},
			Paths: []string{
				".",
			},
			Globs:    []string{".vscode", "*.code-workspace"},
			Priority: 3,
		},

		// Visual Studio
		{
			Name:        "visual_studio",
			Category:    "ide",
			Description: "Visual Studio temporary files",
			OS:          []string{"windows"},
			Paths: []string{
				".",
			},
			Globs:    []string{".vs", "*.suo", "*.user", "*.ncb", "*.sdf", "*.opensdf"},
			Priority: 1,
		},
		{
			Name:        "visual_studio_cache",
			Category:    "ide",
			Description: "Visual Studio build cache",
			OS:          []string{"windows"},
			Paths: []string{
				"%LOCALAPPDATA%\\Microsoft\\VisualStudio",
				"%TEMP%\\VisualStudioTestExplorerExtensions",
			},
			Priority: 2,
		},

		// JetBrains IDEs
		{
			Name:        "jetbrains_cache",
			Category:    "ide",
			Description: "JetBrains IDEs cache",
			OS:          []string{"windows"},
			Paths: []string{
				"%LOCALAPPDATA%\\JetBrains\\Toolbox\\apps",
				"%USERPROFILE%\\.IntelliJIdea*\\system\\caches",
				"%USERPROFILE%\\.WebStorm*\\system\\caches",
				"%USERPROFILE%\\.PyCharm*\\system\\caches",
				"%USERPROFILE%\\.GoLand*\\system\\caches",
				"%USERPROFILE%\\.Rider*\\system\\caches",
				"%USERPROFILE%\\.CLion*\\system\\caches",
			},
			Priority: 1,
		},
		{
			Name:        "jetbrains_cache_unix",
			Category:    "ide",
			Description: "JetBrains IDEs cache (Linux/macOS)",
			OS:          []string{"linux", "darwin"},
			Paths: []string{
				"~/.cache/JetBrains",
				"~/Library/Caches/JetBrains",
				"~/.IntelliJIdea*/system/caches",
				"~/.WebStorm*/system/caches",
				"~/.PyCharm*/system/caches",
				"~/.GoLand*/system/caches",
			},
			Priority: 1,
		},
		{
			Name:        "jetbrains_logs",
			Category:    "ide",
			Description: "JetBrains IDEs logs",
			OS:          []string{"windows", "linux", "darwin"},
			Paths: []string{
				".",
			},
			Globs:    []string{".idea", "*.iml", "*.ipr", "*.iws"},
			Priority: 2,
		},

		// Eclipse
		{
			Name:        "eclipse_workspace",
			Category:    "ide",
			Description: "Eclipse workspace metadata",
			OS:          []string{"windows", "linux", "darwin"},
			Paths: []string{
				".",
			},
			Globs:    []string{".metadata", ".settings", ".project", ".classpath"},
			Priority: 2,
		},

		// Android Studio
		{
			Name:        "android_studio",
			Category:    "ide",
			Description: "Android Studio build files",
			OS:          []string{"windows", "linux", "darwin"},
			Paths: []string{
				".",
			},
			Globs:    []string{".gradle", "build", "local.properties"},
			Priority: 1,
		},
		{
			Name:        "android_studio_cache",
			Category:    "ide",
			Description: "Android Studio cache",
			OS:          []string{"windows"},
			Paths: []string{
				"%USERPROFILE%\\.android\\cache",
				"%USERPROFILE%\\.android\\build-cache",
			},
			Priority: 2,
		},
		{
			Name:        "android_studio_cache_unix",
			Category:    "ide",
			Description: "Android Studio cache (Linux/macOS)",
			OS:          []string{"linux", "darwin"},
			Paths: []string{
				"~/.android/cache",
				"~/.android/build-cache",
			},
			Priority: 2,
		},

		// Xcode (macOS only)
		{
			Name:        "xcode_derived_data",
			Category:    "ide",
			Description: "Xcode derived data",
			OS:          []string{"darwin"},
			Paths: []string{
				"~/Library/Developer/Xcode/DerivedData",
			},
			Priority: 1,
		},
		{
			Name:        "xcode_archives",
			Category:    "ide",
			Description: "Xcode archives",
			OS:          []string{"darwin"},
			Paths: []string{
				"~/Library/Developer/Xcode/Archives",
			},
			Priority: 3,
		},

		// Sublime Text
		{
			Name:        "sublime_cache",
			Category:    "ide",
			Description: "Sublime Text cache",
			OS:          []string{"windows"},
			Paths: []string{
				"%APPDATA%\\Sublime Text 3\\Cache",
				"%APPDATA%\\Sublime Text\\Cache",
			},
			Priority: 3,
		},
		{
			Name:        "sublime_cache_unix",
			Category:    "ide",
			Description: "Sublime Text cache (Linux/macOS)",
			OS:          []string{"linux", "darwin"},
			Paths: []string{
				"~/.config/sublime-text-3/Cache",
				"~/.config/sublime-text/Cache",
				"~/Library/Application Support/Sublime Text 3/Cache",
			},
			Priority: 3,
		},

		// Atom
		{
			Name:        "atom_cache",
			Category:    "ide",
			Description: "Atom editor cache",
			OS:          []string{"windows"},
			Paths: []string{
				"%USERPROFILE%\\.atom\\compile-cache",
				"%USERPROFILE%\\.atom\\storage",
			},
			Priority: 3,
		},
		{
			Name:        "atom_cache_unix",
			Category:    "ide",
			Description: "Atom editor cache (Linux/macOS)",
			OS:          []string{"linux", "darwin"},
			Paths: []string{
				"~/.atom/compile-cache",
				"~/.atom/storage",
			},
			Priority: 3,
		},
	}
}
