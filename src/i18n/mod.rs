use anyhow::{Context, Result};
use fluent::{FluentBundle, FluentResource};
use fluent_bundle::resolver::errors::ResolverError;
use std::collections::HashMap;
use unic_langid::LanguageIdentifier;

/// Internationalization manager for multi-language support
pub struct I18n {
    current_language: String,
    bundles: HashMap<String, FluentBundle<FluentResource>>,
}

impl I18n {
    /// Create a new I18n instance with the specified language
    pub fn new(language: &str) -> Result<Self> {
        let mut i18n = Self {
            current_language: language.to_string(),
            bundles: HashMap::new(),
        };
        
        // Load built-in language resources
        i18n.load_builtin_languages()?;
        
        Ok(i18n)
    }
    
    /// Load all built-in language resources
    fn load_builtin_languages(&mut self) -> Result<()> {
        // English (default)
        self.load_language("en", include_str!("../../locales/en.ftl"))?;
        
        // Turkish
        self.load_language("tr", include_str!("../../locales/tr.ftl"))?;
        
        // German
        self.load_language("de", include_str!("../../locales/de.ftl"))?;
        
        // French
        self.load_language("fr", include_str!("../../locales/fr.ftl"))?;
        
        // Spanish
        self.load_language("es", include_str!("../../locales/es.ftl"))?;
        
        Ok(())
    }
    
    /// Load a language resource
    fn load_language(&mut self, lang_code: &str, ftl_content: &str) -> Result<()> {
        let lang_id: LanguageIdentifier = lang_code
            .parse()
            .with_context(|| format!("Invalid language code: {}", lang_code))?;
        
        let resource = FluentResource::try_new(ftl_content.to_string())
            .map_err(|(_, errors)| anyhow::anyhow!("Failed to parse FTL: {:?}", errors))?;
        
        let mut bundle = FluentBundle::new(vec![lang_id]);
        bundle
            .add_resource(resource)
            .map_err(|errors| anyhow::anyhow!("Failed to add resource: {:?}", errors))?;
        
        self.bundles.insert(lang_code.to_string(), bundle);
        
        Ok(())
    }
    
    /// Get a localized string
    pub fn get(&self, key: &str) -> String {
        self.get_with_args(key, &HashMap::new())
    }
    
    /// Get a localized string with arguments
    pub fn get_with_args(&self, key: &str, args: &HashMap<String, String>) -> String {
        // Try current language first
        if let Some(bundle) = self.bundles.get(&self.current_language) {
            if let Some(message) = self.format_message(bundle, key, args) {
                return message;
            }
        }
        
        // Fallback to English
        if self.current_language != "en" {
            if let Some(bundle) = self.bundles.get("en") {
                if let Some(message) = self.format_message(bundle, key, args) {
                    return message;
                }
            }
        }
        
        // If all else fails, return the key itself
        key.to_string()
    }
    
    /// Format a message with the given bundle
    fn format_message(
        &self,
        bundle: &FluentBundle<FluentResource>,
        key: &str,
        args: &HashMap<String, String>,
    ) -> Option<String> {
        let msg = bundle.get_message(key)?;
        let pattern = msg.value()?;
        
        let mut fluent_args = fluent::FluentArgs::new();
        for (k, v) in args {
            fluent_args.set(k.as_str(), v.as_str());
        }
        
        let mut errors = Vec::new();
        let value = bundle.format_pattern(pattern, Some(&fluent_args), &mut errors);
        
        if !errors.is_empty() {
            tracing::warn!("Errors formatting message '{}': {:?}", key, errors);
        }
        
        Some(value.to_string())
    }
    
    /// Change the current language
    pub fn set_language(&mut self, language: &str) -> Result<()> {
        if !self.bundles.contains_key(language) {
            return Err(anyhow::anyhow!("Language '{}' not available", language));
        }
        
        self.current_language = language.to_string();
        Ok(())
    }
    
    /// Get the current language
    pub fn current_language(&self) -> &str {
        &self.current_language
    }
    
    /// Get available languages
    pub fn available_languages(&self) -> Vec<String> {
        self.bundles.keys().cloned().collect()
    }
}

impl Default for I18n {
    fn default() -> Self {
        Self::new("en").expect("Failed to create default I18n")
    }
}
