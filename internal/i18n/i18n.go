package i18n

import (
	"embed"
	"encoding/json"
	"fmt"

	"github.com/nicksnyder/go-i18n/v2/i18n"
	"golang.org/x/text/language"
)

//go:embed locales/*.json
var localeFS embed.FS

// Translator handles internationalization
type Translator interface {
	T(messageID string, args ...interface{}) string
	Tf(messageID string, data map[string]interface{}) string
	GetLanguage() string
	SetLanguage(lang string) error
	GetAvailableLanguages() []string
}

// translator implements the Translator interface
type translator struct {
	bundle    *i18n.Bundle
	localizer *i18n.Localizer
	language  string
}

// New creates a new translator instance
func New(lang string) Translator {
	bundle := i18n.NewBundle(language.English)
	bundle.RegisterUnmarshalFunc("json", json.Unmarshal)

	// Load embedded locale files
	loadLocales(bundle)

	// Parse language tag
	tag, err := language.Parse(lang)
	if err != nil {
		tag = language.English
	}

	localizer := i18n.NewLocalizer(bundle, tag.String())

	return &translator{
		bundle:    bundle,
		localizer: localizer,
		language:  tag.String(),
	}
}

// loadLocales loads all embedded locale files
func loadLocales(bundle *i18n.Bundle) {
	locales := []string{
		"locales/en.json",
		"locales/tr.json",
		"locales/es.json",
		"locales/fr.json",
		"locales/de.json",
		"locales/ja.json",
		"locales/zh.json",
		"locales/ru.json",
	}

	for _, locale := range locales {
		data, err := localeFS.ReadFile(locale)
		if err == nil {
			bundle.ParseMessageFileBytes(data, locale)
		}
	}
}

// T translates a message with optional arguments
func (t *translator) T(messageID string, args ...interface{}) string {
	msg, err := t.localizer.Localize(&i18n.LocalizeConfig{
		MessageID: messageID,
	})

	if err != nil {
		// Fallback to message ID if translation not found
		if len(args) > 0 {
			return fmt.Sprintf(messageID, args...)
		}
		return messageID
	}

	if len(args) > 0 {
		return fmt.Sprintf(msg, args...)
	}

	return msg
}

// Tf translates a message with template data
func (t *translator) Tf(messageID string, data map[string]interface{}) string {
	msg, err := t.localizer.Localize(&i18n.LocalizeConfig{
		MessageID:    messageID,
		TemplateData: data,
	})

	if err != nil {
		return messageID
	}

	return msg
}

// GetLanguage returns the current language
func (t *translator) GetLanguage() string {
	return t.language
}

// SetLanguage changes the current language
func (t *translator) SetLanguage(lang string) error {
	tag, err := language.Parse(lang)
	if err != nil {
		return err
	}

	t.localizer = i18n.NewLocalizer(t.bundle, tag.String())
	t.language = tag.String()
	return nil
}

// GetAvailableLanguages returns list of available languages
func (t *translator) GetAvailableLanguages() []string {
	return []string{
		"en", // English
		"tr", // Turkish
		"es", // Spanish
		"fr", // French
		"de", // German
		"ja", // Japanese
		"zh", // Chinese
		"ru", // Russian
	}
}

// Message IDs for common messages
const (
	MsgWelcome           = "welcome"
	MsgScanning          = "scanning"
	MsgCleaning          = "cleaning"
	MsgComplete          = "complete"
	MsgError             = "error"
	MsgConfirm           = "confirm"
	MsgYes               = "yes"
	MsgNo                = "no"
	MsgFilesFound        = "files_found"
	MsgSizeToClean       = "size_to_clean"
	MsgFilesCleaned      = "files_cleaned"
	MsgSpaceFreed        = "space_freed"
	MsgDryRunMode        = "dry_run_mode"
	MsgNoFilesFound      = "no_files_found"
	MsgOperationCanceled = "operation_canceled"
	MsgCleaningFailed    = "cleaning_failed"
	MsgScanningTarget    = "scanning_target"
	MsgCleaningTarget    = "cleaning_target"
	MsgTargetNotFound    = "target_not_found"
	MsgInvalidTarget     = "invalid_target"
	MsgAvailableTargets  = "available_targets"
	MsgSelectTargets     = "select_targets"
	MsgProcessing        = "processing"
	MsgSkipping          = "skipping"
	MsgDeleting          = "deleting"
	MsgProtectedFile     = "protected_file"
	MsgPermissionDenied  = "permission_denied"
	MsgFileNotFound      = "file_not_found"
	MsgSuccess           = "success"
	MsgWarning           = "warning"
	MsgInfo              = "info"
	MsgDebug             = "debug"
	MsgFatal             = "fatal"
)
