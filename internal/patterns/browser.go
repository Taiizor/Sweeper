package patterns

import "github.com/Taiizor/Sweeper/internal/models"

// getBrowserPatterns returns browser cache and temporary file patterns
func getBrowserPatterns() []models.Pattern {
	return []models.Pattern{
		// Google Chrome
		{
			Name:        "chrome_cache",
			Category:    "browser",
			Description: "Google Chrome cache and temporary files",
			OS:          []string{"windows"},
			Paths: []string{
				"%LOCALAPPDATA%\\Google\\Chrome\\User Data\\Default\\Cache",
				"%LOCALAPPDATA%\\Google\\Chrome\\User Data\\Default\\Code Cache",
				"%LOCALAPPDATA%\\Google\\Chrome\\User Data\\Default\\GPUCache",
				"%LOCALAPPDATA%\\Google\\Chrome\\User Data\\Default\\Service Worker\\CacheStorage",
			},
			Priority: 1,
		},
		{
			Name:        "chrome_cache_linux",
			Category:    "browser",
			Description: "Google Chrome cache (Linux)",
			OS:          []string{"linux"},
			Paths: []string{
				"~/.cache/google-chrome",
				"~/.config/google-chrome/Default/Cache",
				"~/.config/google-chrome/Default/Code Cache",
			},
			Priority: 1,
		},
		{
			Name:        "chrome_cache_mac",
			Category:    "browser",
			Description: "Google Chrome cache (macOS)",
			OS:          []string{"darwin"},
			Paths: []string{
				"~/Library/Caches/Google/Chrome",
				"~/Library/Application Support/Google/Chrome/Default/Cache",
			},
			Priority: 1,
		},

		// Mozilla Firefox
		{
			Name:        "firefox_cache",
			Category:    "browser",
			Description: "Mozilla Firefox cache and temporary files",
			OS:          []string{"windows"},
			Paths: []string{
				"%LOCALAPPDATA%\\Mozilla\\Firefox\\Profiles\\*.default-release\\cache2",
				"%LOCALAPPDATA%\\Mozilla\\Firefox\\Profiles\\*.default\\cache2",
				"%APPDATA%\\Mozilla\\Firefox\\Profiles\\*.default-release\\cache2",
			},
			Priority: 1,
		},
		{
			Name:        "firefox_cache_linux",
			Category:    "browser",
			Description: "Mozilla Firefox cache (Linux)",
			OS:          []string{"linux"},
			Paths: []string{
				"~/.cache/mozilla/firefox/*.default-release/cache2",
				"~/.cache/mozilla/firefox/*.default/cache2",
				"~/.mozilla/firefox/*.default-release/cache2",
			},
			Priority: 1,
		},
		{
			Name:        "firefox_cache_mac",
			Category:    "browser",
			Description: "Mozilla Firefox cache (macOS)",
			OS:          []string{"darwin"},
			Paths: []string{
				"~/Library/Caches/Firefox/Profiles/*.default-release/cache2",
				"~/Library/Application Support/Firefox/Profiles/*.default-release/cache2",
			},
			Priority: 1,
		},

		// Microsoft Edge
		{
			Name:        "edge_cache",
			Category:    "browser",
			Description: "Microsoft Edge cache and temporary files",
			OS:          []string{"windows"},
			Paths: []string{
				"%LOCALAPPDATA%\\Microsoft\\Edge\\User Data\\Default\\Cache",
				"%LOCALAPPDATA%\\Microsoft\\Edge\\User Data\\Default\\Code Cache",
				"%LOCALAPPDATA%\\Microsoft\\Edge\\User Data\\Default\\GPUCache",
				"%LOCALAPPDATA%\\Microsoft\\Edge\\User Data\\Default\\Service Worker\\CacheStorage",
			},
			Priority: 1,
		},
		{
			Name:        "edge_cache_linux",
			Category:    "browser",
			Description: "Microsoft Edge cache (Linux)",
			OS:          []string{"linux"},
			Paths: []string{
				"~/.cache/microsoft-edge",
				"~/.config/microsoft-edge/Default/Cache",
			},
			Priority: 1,
		},
		{
			Name:        "edge_cache_mac",
			Category:    "browser",
			Description: "Microsoft Edge cache (macOS)",
			OS:          []string{"darwin"},
			Paths: []string{
				"~/Library/Caches/Microsoft Edge",
				"~/Library/Application Support/Microsoft Edge/Default/Cache",
			},
			Priority: 1,
		},

		// Opera
		{
			Name:        "opera_cache",
			Category:    "browser",
			Description: "Opera browser cache",
			OS:          []string{"windows"},
			Paths: []string{
				"%APPDATA%\\Opera Software\\Opera Stable\\Cache",
				"%LOCALAPPDATA%\\Opera Software\\Opera Stable\\Cache",
			},
			Priority: 2,
		},

		// Brave
		{
			Name:        "brave_cache",
			Category:    "browser",
			Description: "Brave browser cache",
			OS:          []string{"windows"},
			Paths: []string{
				"%LOCALAPPDATA%\\BraveSoftware\\Brave-Browser\\User Data\\Default\\Cache",
				"%LOCALAPPDATA%\\BraveSoftware\\Brave-Browser\\User Data\\Default\\Code Cache",
			},
			Priority: 2,
		},
		{
			Name:        "brave_cache_linux",
			Category:    "browser",
			Description: "Brave browser cache (Linux)",
			OS:          []string{"linux"},
			Paths: []string{
				"~/.cache/BraveSoftware/Brave-Browser",
				"~/.config/BraveSoftware/Brave-Browser/Default/Cache",
			},
			Priority: 2,
		},
		{
			Name:        "brave_cache_mac",
			Category:    "browser",
			Description: "Brave browser cache (macOS)",
			OS:          []string{"darwin"},
			Paths: []string{
				"~/Library/Caches/com.brave.Browser",
				"~/Library/Application Support/BraveSoftware/Brave-Browser/Default/Cache",
			},
			Priority: 2,
		},

		// Safari (macOS only)
		{
			Name:        "safari_cache",
			Category:    "browser",
			Description: "Safari browser cache",
			OS:          []string{"darwin"},
			Paths: []string{
				"~/Library/Caches/com.apple.Safari",
				"~/Library/Caches/com.apple.SafariTechnologyPreview",
				"~/Library/Safari/LocalStorage",
			},
			Priority: 1,
		},
	}
}
