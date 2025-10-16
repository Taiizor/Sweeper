package patterns

import "github.com/Taiizor/Sweeper/internal/models"

// getDevelopmentPatterns returns development-related temporary file patterns
func getDevelopmentPatterns() []models.Pattern {
	return []models.Pattern{
		// Build artifacts
		{
			Name:        "build_artifacts",
			Category:    "development",
			Description: "Common build artifacts and output directories",
			OS:          []string{"windows", "linux", "darwin"},
			Paths: []string{
				".",
			},
			Globs: []string{
				"build",
				"dist",
				"out",
				"bin",
				"obj",
				"target",
				"*.exe",
				"*.dll",
				"*.so",
				"*.dylib",
			},
			Priority: 2,
		},

		// Object files
		{
			Name:        "object_files",
			Category:    "development",
			Description: "Compiled object files",
			OS:          []string{"windows", "linux", "darwin"},
			Paths: []string{
				".",
			},
			Extensions: []string{".o", ".obj", ".a", ".lib", ".pdb", ".ilk"},
			Priority:   1,
		},

		// Debug symbols
		{
			Name:        "debug_symbols",
			Category:    "development",
			Description: "Debug symbol files",
			OS:          []string{"windows", "linux", "darwin"},
			Paths: []string{
				".",
			},
			Extensions: []string{".pdb", ".dSYM", ".map"},
			Priority:   3,
		},
	}
}

// getNpmPatterns returns npm-related temporary file patterns
func getNpmPatterns() []models.Pattern {
	return []models.Pattern{
		{
			Name:        "node_modules",
			Category:    "npm",
			Description: "Node.js dependencies",
			OS:          []string{"windows", "linux", "darwin"},
			Paths: []string{
				".",
			},
			Globs:    []string{"node_modules"},
			Priority: 1,
		},
		{
			Name:        "npm_cache",
			Category:    "npm",
			Description: "npm cache directory",
			OS:          []string{"windows"},
			Paths: []string{
				"%APPDATA%\\npm-cache",
				"%LOCALAPPDATA%\\npm-cache",
			},
			Priority: 2,
		},
		{
			Name:        "npm_cache_unix",
			Category:    "npm",
			Description: "npm cache directory",
			OS:          []string{"linux", "darwin"},
			Paths: []string{
				"~/.npm",
			},
			Priority: 2,
		},
		{
			Name:        "yarn_cache",
			Category:    "npm",
			Description: "Yarn cache directory",
			OS:          []string{"windows"},
			Paths: []string{
				"%LOCALAPPDATA%\\Yarn\\Cache",
			},
			Priority: 2,
		},
		{
			Name:        "yarn_cache_unix",
			Category:    "npm",
			Description: "Yarn cache directory",
			OS:          []string{"linux", "darwin"},
			Paths: []string{
				"~/.cache/yarn",
				"~/Library/Caches/Yarn",
			},
			Priority: 2,
		},
		{
			Name:        "pnpm_store",
			Category:    "npm",
			Description: "pnpm store directory",
			OS:          []string{"windows", "linux", "darwin"},
			Paths: []string{
				"~/.pnpm-store",
			},
			Priority: 2,
		},
	}
}

// getPipPatterns returns Python pip-related temporary file patterns
func getPipPatterns() []models.Pattern {
	return []models.Pattern{
		{
			Name:        "pip_cache",
			Category:    "pip",
			Description: "pip cache directory",
			OS:          []string{"windows"},
			Paths: []string{
				"%LOCALAPPDATA%\\pip\\Cache",
			},
			Priority: 1,
		},
		{
			Name:        "pip_cache_unix",
			Category:    "pip",
			Description: "pip cache directory",
			OS:          []string{"linux", "darwin"},
			Paths: []string{
				"~/.cache/pip",
				"~/Library/Caches/pip",
			},
			Priority: 1,
		},
		{
			Name:        "python_cache",
			Category:    "pip",
			Description: "Python bytecode cache",
			OS:          []string{"windows", "linux", "darwin"},
			Paths: []string{
				".",
			},
			Globs:    []string{"__pycache__", "*.pyc", "*.pyo"},
			Priority: 1,
		},
		{
			Name:        "virtualenv",
			Category:    "pip",
			Description: "Python virtual environments",
			OS:          []string{"windows", "linux", "darwin"},
			Paths: []string{
				".",
			},
			Globs:    []string{"venv", "env", ".venv", ".env", "virtualenv"},
			Priority: 2,
		},
		{
			Name:        "pytest_cache",
			Category:    "pip",
			Description: "Pytest cache",
			OS:          []string{"windows", "linux", "darwin"},
			Paths: []string{
				".",
			},
			Globs:    []string{".pytest_cache"},
			Priority: 3,
		},
		{
			Name:        "jupyter_checkpoints",
			Category:    "pip",
			Description: "Jupyter notebook checkpoints",
			OS:          []string{"windows", "linux", "darwin"},
			Paths: []string{
				".",
			},
			Globs:    []string{".ipynb_checkpoints"},
			Priority: 3,
		},
	}
}

// getCargoPatterns returns Rust Cargo-related temporary file patterns
func getCargoPatterns() []models.Pattern {
	return []models.Pattern{
		{
			Name:        "cargo_target",
			Category:    "cargo",
			Description: "Cargo build output",
			OS:          []string{"windows", "linux", "darwin"},
			Paths: []string{
				".",
			},
			Globs:    []string{"target"},
			Priority: 1,
		},
		{
			Name:        "cargo_registry",
			Category:    "cargo",
			Description: "Cargo registry cache",
			OS:          []string{"windows"},
			Paths: []string{
				"%USERPROFILE%\\.cargo\\registry",
			},
			Priority: 2,
		},
		{
			Name:        "cargo_registry_unix",
			Category:    "cargo",
			Description: "Cargo registry cache",
			OS:          []string{"linux", "darwin"},
			Paths: []string{
				"~/.cargo/registry",
			},
			Priority: 2,
		},
		{
			Name:        "cargo_git",
			Category:    "cargo",
			Description: "Cargo git checkouts",
			OS:          []string{"windows", "linux", "darwin"},
			Paths: []string{
				"~/.cargo/git",
			},
			Priority: 3,
		},
	}
}

// getMavenPatterns returns Maven-related temporary file patterns
func getMavenPatterns() []models.Pattern {
	return []models.Pattern{
		{
			Name:        "maven_target",
			Category:    "maven",
			Description: "Maven build output",
			OS:          []string{"windows", "linux", "darwin"},
			Paths: []string{
				".",
			},
			Globs:    []string{"target"},
			Priority: 1,
		},
		{
			Name:        "maven_repository",
			Category:    "maven",
			Description: "Maven local repository",
			OS:          []string{"windows", "linux", "darwin"},
			Paths: []string{
				"~/.m2/repository",
			},
			Priority: 2,
		},
		{
			Name:        "maven_wrapper",
			Category:    "maven",
			Description: "Maven wrapper downloads",
			OS:          []string{"windows", "linux", "darwin"},
			Paths: []string{
				"~/.m2/wrapper",
			},
			Priority: 3,
		},
	}
}

// getGradlePatterns returns Gradle-related temporary file patterns
func getGradlePatterns() []models.Pattern {
	return []models.Pattern{
		{
			Name:        "gradle_build",
			Category:    "gradle",
			Description: "Gradle build output",
			OS:          []string{"windows", "linux", "darwin"},
			Paths: []string{
				".",
			},
			Globs:    []string{"build", ".gradle"},
			Priority: 1,
		},
		{
			Name:        "gradle_cache",
			Category:    "gradle",
			Description: "Gradle cache",
			OS:          []string{"windows", "linux", "darwin"},
			Paths: []string{
				"~/.gradle/caches",
			},
			Priority: 2,
		},
		{
			Name:        "gradle_wrapper",
			Category:    "gradle",
			Description: "Gradle wrapper downloads",
			OS:          []string{"windows", "linux", "darwin"},
			Paths: []string{
				"~/.gradle/wrapper",
			},
			Priority: 3,
		},
		{
			Name:        "gradle_daemon",
			Category:    "gradle",
			Description: "Gradle daemon logs",
			OS:          []string{"windows", "linux", "darwin"},
			Paths: []string{
				"~/.gradle/daemon",
			},
			Priority: 4,
		},
	}
}
