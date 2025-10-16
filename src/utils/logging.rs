use anyhow::Result;
use tracing_subscriber::{fmt, prelude::*, EnvFilter};

/// Initialize the logging system
pub fn init() -> Result<()> {
    // Create environment filter
    let env_filter = EnvFilter::try_from_default_env()
        .unwrap_or_else(|_| EnvFilter::new("info"));
    
    // Create formatter layer
    let fmt_layer = fmt::layer()
        .with_target(false)
        .with_thread_ids(false)
        .with_thread_names(false)
        .with_ansi(true)
        .with_file(false)
        .with_line_number(false);
    
    // Build the subscriber
    tracing_subscriber::registry()
        .with(env_filter)
        .with(fmt_layer)
        .init();
    
    Ok(())
}

/// Initialize logging with custom configuration
pub fn init_with_config(level: &str, log_file: Option<&std::path::Path>, timestamps: bool) -> Result<()> {
    // Create environment filter
    let env_filter = EnvFilter::try_from_default_env()
        .unwrap_or_else(|_| EnvFilter::new(level));
    
    // Create formatter layer
    let fmt_layer = fmt::layer()
        .with_target(false)
        .with_thread_ids(false)
        .with_thread_names(false)
        .with_ansi(true)
        .with_file(false)
        .with_line_number(false);
    
    if timestamps {
        // Add timestamps if configured
        let fmt_layer = fmt_layer.with_timer(fmt::time::SystemTime::default());
        
        if let Some(log_path) = log_file {
            // Also log to file if configured
            let file = std::fs::OpenOptions::new()
                .create(true)
                .append(true)
                .open(log_path)?;
            
            let file_layer = fmt::layer()
                .with_ansi(false)
                .with_writer(file);
            
            tracing_subscriber::registry()
                .with(env_filter)
                .with(fmt_layer)
                .with(file_layer)
                .init();
        } else {
            tracing_subscriber::registry()
                .with(env_filter)
                .with(fmt_layer)
                .init();
        }
    } else {
        tracing_subscriber::registry()
            .with(env_filter)
            .with(fmt_layer)
            .init();
    }
    
    Ok(())
}
