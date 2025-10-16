package logger

import (
	"os"
	"path/filepath"

	"go.uber.org/zap"
	"go.uber.org/zap/zapcore"
)

// Logger wraps zap logger
type Logger struct {
	*zap.Logger
}

// New creates a new logger instance
func New(level string) *Logger {
	config := zap.NewProductionConfig()
	
	// Set log level
	switch level {
	case "debug":
		config.Level = zap.NewAtomicLevelAt(zap.DebugLevel)
	case "info":
		config.Level = zap.NewAtomicLevelAt(zap.InfoLevel)
	case "warn":
		config.Level = zap.NewAtomicLevelAt(zap.WarnLevel)
	case "error":
		config.Level = zap.NewAtomicLevelAt(zap.ErrorLevel)
	default:
		config.Level = zap.NewAtomicLevelAt(zap.InfoLevel)
	}
	
	// Configure encoding
	config.Encoding = "console"
	config.EncoderConfig.TimeKey = "timestamp"
	config.EncoderConfig.EncodeTime = zapcore.ISO8601TimeEncoder
	config.EncoderConfig.EncodeLevel = zapcore.CapitalColorLevelEncoder
	
	// Build logger
	zapLogger, _ := config.Build()
	
	return &Logger{
		Logger: zapLogger,
	}
}

// NewFileLogger creates a logger that writes to both console and file
func NewFileLogger(level string, logFile string) *Logger {
	// Create log directory if it doesn't exist
	logDir := filepath.Dir(logFile)
	os.MkdirAll(logDir, 0755)
	
	// Create console encoder config
	consoleEncoderConfig := zapcore.EncoderConfig{
		TimeKey:        "timestamp",
		LevelKey:       "level",
		NameKey:        "logger",
		CallerKey:      "caller",
		MessageKey:     "message",
		StacktraceKey:  "stacktrace",
		LineEnding:     zapcore.DefaultLineEnding,
		EncodeLevel:    zapcore.CapitalColorLevelEncoder,
		EncodeTime:     zapcore.ISO8601TimeEncoder,
		EncodeDuration: zapcore.StringDurationEncoder,
		EncodeCaller:   zapcore.ShortCallerEncoder,
	}
	
	// Create file encoder config
	fileEncoderConfig := zapcore.EncoderConfig{
		TimeKey:        "timestamp",
		LevelKey:       "level",
		NameKey:        "logger",
		CallerKey:      "caller",
		MessageKey:     "message",
		StacktraceKey:  "stacktrace",
		LineEnding:     zapcore.DefaultLineEnding,
		EncodeLevel:    zapcore.CapitalLevelEncoder,
		EncodeTime:     zapcore.ISO8601TimeEncoder,
		EncodeDuration: zapcore.StringDurationEncoder,
		EncodeCaller:   zapcore.ShortCallerEncoder,
	}
	
	// Set log level
	var logLevel zapcore.Level
	switch level {
	case "debug":
		logLevel = zap.DebugLevel
	case "info":
		logLevel = zap.InfoLevel
	case "warn":
		logLevel = zap.WarnLevel
	case "error":
		logLevel = zap.ErrorLevel
	default:
		logLevel = zap.InfoLevel
	}
	
	// Create console core
	consoleCore := zapcore.NewCore(
		zapcore.NewConsoleEncoder(consoleEncoderConfig),
		zapcore.AddSync(os.Stdout),
		logLevel,
	)
	
	// Create file core
	file, _ := os.OpenFile(logFile, os.O_APPEND|os.O_CREATE|os.O_WRONLY, 0644)
	fileCore := zapcore.NewCore(
		zapcore.NewJSONEncoder(fileEncoderConfig),
		zapcore.AddSync(file),
		logLevel,
	)
	
	// Combine cores
	core := zapcore.NewTee(consoleCore, fileCore)
	
	// Build logger
	zapLogger := zap.New(core, zap.AddCaller(), zap.AddStacktrace(zap.ErrorLevel))
	
	return &Logger{
		Logger: zapLogger,
	}
}

// Debug logs a debug message
func (l *Logger) Debug(msg string, fields ...interface{}) {
	l.Sugar().Debugw(msg, fields...)
}

// Info logs an info message
func (l *Logger) Info(msg string, fields ...interface{}) {
	l.Sugar().Infow(msg, fields...)
}

// Warn logs a warning message
func (l *Logger) Warn(msg string, fields ...interface{}) {
	l.Sugar().Warnw(msg, fields...)
}

// Error logs an error message
func (l *Logger) Error(msg string, fields ...interface{}) {
	l.Sugar().Errorw(msg, fields...)
}

// Fatal logs a fatal message and exits
func (l *Logger) Fatal(msg string, fields ...interface{}) {
	l.Sugar().Fatalw(msg, fields...)
}
