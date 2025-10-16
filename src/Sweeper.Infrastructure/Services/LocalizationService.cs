using Microsoft.Extensions.Logging;
using Sweeper.Domain.Interfaces;
using System.Globalization;
using System.Resources;
using System.Reflection;

namespace Sweeper.Infrastructure.Services;

public class LocalizationService : ILocalizationService
{
    private readonly ILogger<LocalizationService> _logger;
    private readonly Dictionary<string, ResourceManager> _resourceManagers;
    private CultureInfo _currentCulture;
    private readonly List<CultureInfo> _supportedCultures;
    private readonly Dictionary<string, Dictionary<string, string>> _inMemoryResources;

    public LocalizationService(ILogger<LocalizationService> logger)
    {
        _logger = logger;
        _resourceManagers = new Dictionary<string, ResourceManager>();
        _currentCulture = CultureInfo.CurrentUICulture;
        _supportedCultures = new List<CultureInfo>
        {
            new CultureInfo("en-US"),
            new CultureInfo("tr-TR"),
            new CultureInfo("de-DE"),
            new CultureInfo("fr-FR"),
            new CultureInfo("es-ES"),
            new CultureInfo("ja-JP"),
            new CultureInfo("zh-CN"),
            new CultureInfo("ru-RU")
        };

        // Initialize in-memory resources (temporary until proper resource files are created)
        _inMemoryResources = InitializeInMemoryResources();
    }

    public string GetString(string key)
    {
        return GetString(key, _currentCulture);
    }

    public string GetString(string key, params object[] args)
    {
        var format = GetString(key, _currentCulture);
        return string.Format(_currentCulture, format, args);
    }

    public string GetString(string key, CultureInfo culture)
    {
        try
        {
            // First try to get from in-memory resources
            var cultureName = culture.Name;
            if (string.IsNullOrEmpty(cultureName))
                cultureName = "en-US";

            if (_inMemoryResources.ContainsKey(cultureName) && 
                _inMemoryResources[cultureName].ContainsKey(key))
            {
                return _inMemoryResources[cultureName][key];
            }

            // Fallback to English if not found
            if (cultureName != "en-US" && 
                _inMemoryResources.ContainsKey("en-US") && 
                _inMemoryResources["en-US"].ContainsKey(key))
            {
                return _inMemoryResources["en-US"][key];
            }

            // If still not found, return the key itself
            _logger.LogWarning("Localization key not found: {Key} for culture: {Culture}", key, cultureName);
            return key;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting localized string for key: {Key}", key);
            return key;
        }
    }

    public string GetString(string key, CultureInfo culture, params object[] args)
    {
        var format = GetString(key, culture);
        return string.Format(culture, format, args);
    }

    public void SetCulture(string cultureName)
    {
        try
        {
            var culture = new CultureInfo(cultureName);
            SetCulture(culture);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting culture: {CultureName}", cultureName);
        }
    }

    public void SetCulture(CultureInfo culture)
    {
        _currentCulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        _logger.LogInformation("Culture set to: {Culture}", culture.Name);
    }

    public CultureInfo GetCurrentCulture()
    {
        return _currentCulture;
    }

    public List<CultureInfo> GetAvailableCultures()
    {
        return new List<CultureInfo>(_supportedCultures);
    }

    public bool IsCultureSupported(string cultureName)
    {
        return _supportedCultures.Any(c => c.Name.Equals(cultureName, StringComparison.OrdinalIgnoreCase));
    }

    public bool IsCultureSupported(CultureInfo culture)
    {
        return _supportedCultures.Any(c => c.Name.Equals(culture.Name, StringComparison.OrdinalIgnoreCase));
    }

    private Dictionary<string, Dictionary<string, string>> InitializeInMemoryResources()
    {
        var resources = new Dictionary<string, Dictionary<string, string>>();

        // English (en-US) - Default
        resources["en-US"] = new Dictionary<string, string>
        {
            ["app.name"] = "Sweeper",
            ["app.description"] = "Clean temporary files from your system",
            ["app.version"] = "Version {0}",
            
            ["command.clean"] = "Clean temporary files",
            ["command.scan"] = "Scan for temporary files",
            ["command.list"] = "List cleanup categories",
            ["command.estimate"] = "Estimate space to be freed",
            ["command.config"] = "Configure settings",
            
            ["category.systemtemp"] = "System Temporary Files",
            ["category.usertemp"] = "User Temporary Files",
            ["category.browsercache"] = "Browser Cache",
            ["category.applicationcache"] = "Application Cache",
            ["category.logfiles"] = "Log Files",
            ["category.thumbnails"] = "Thumbnails",
            ["category.recyclebin"] = "Recycle Bin",
            ["category.downloadedprograms"] = "Downloaded Programs",
            ["category.windowsupdate"] = "Windows Update Files",
            ["category.packagemanagercache"] = "Package Manager Cache",
            ["category.buildartifacts"] = "Build Artifacts",
            
            ["message.scanning"] = "Scanning {0}...",
            ["message.scanComplete"] = "Scan complete. Found {0} files ({1})",
            ["message.cleaning"] = "Cleaning {0}...",
            ["message.cleanComplete"] = "Cleanup complete. Freed {0}",
            ["message.error"] = "Error: {0}",
            ["message.warning"] = "Warning: {0}",
            ["message.info"] = "Info: {0}",
            ["message.confirmCleanup"] = "Are you sure you want to delete {0} files ({1})? [y/N]",
            ["message.elevated"] = "Administrator privileges required",
            ["message.noFilesFound"] = "No files found to clean",
            
            ["size.bytes"] = "{0} bytes",
            ["size.kb"] = "{0:F2} KB",
            ["size.mb"] = "{0:F2} MB",
            ["size.gb"] = "{0:F2} GB",
            ["size.tb"] = "{0:F2} TB",
            
            ["option.help"] = "Show help information",
            ["option.verbose"] = "Enable verbose output",
            ["option.quiet"] = "Suppress non-error output",
            ["option.force"] = "Force cleanup without confirmation",
            ["option.dryRun"] = "Perform a dry run without deleting files",
            ["option.categories"] = "Specify cleanup categories",
            ["option.exclude"] = "Exclude specific paths or patterns",
            ["option.include"] = "Include specific paths or patterns",
            ["option.recursive"] = "Process directories recursively",
            ["option.parallel"] = "Number of parallel operations",
            ["option.language"] = "Set display language"
        };

        // Turkish (tr-TR)
        resources["tr-TR"] = new Dictionary<string, string>
        {
            ["app.name"] = "Sweeper",
            ["app.description"] = "Sisteminizdeki geçici dosyaları temizleyin",
            ["app.version"] = "Sürüm {0}",
            
            ["command.clean"] = "Geçici dosyaları temizle",
            ["command.scan"] = "Geçici dosyaları tara",
            ["command.list"] = "Temizleme kategorilerini listele",
            ["command.estimate"] = "Boşaltılacak alanı tahmin et",
            ["command.config"] = "Ayarları yapılandır",
            
            ["category.systemtemp"] = "Sistem Geçici Dosyaları",
            ["category.usertemp"] = "Kullanıcı Geçici Dosyaları",
            ["category.browsercache"] = "Tarayıcı Önbelleği",
            ["category.applicationcache"] = "Uygulama Önbelleği",
            ["category.logfiles"] = "Log Dosyaları",
            ["category.thumbnails"] = "Küçük Resimler",
            ["category.recyclebin"] = "Geri Dönüşüm Kutusu",
            ["category.downloadedprograms"] = "İndirilen Programlar",
            ["category.windowsupdate"] = "Windows Güncelleme Dosyaları",
            ["category.packagemanagercache"] = "Paket Yöneticisi Önbelleği",
            ["category.buildartifacts"] = "Derleme Çıktıları",
            
            ["message.scanning"] = "{0} taranıyor...",
            ["message.scanComplete"] = "Tarama tamamlandı. {0} dosya bulundu ({1})",
            ["message.cleaning"] = "{0} temizleniyor...",
            ["message.cleanComplete"] = "Temizlik tamamlandı. {0} alan boşaltıldı",
            ["message.error"] = "Hata: {0}",
            ["message.warning"] = "Uyarı: {0}",
            ["message.info"] = "Bilgi: {0}",
            ["message.confirmCleanup"] = "{0} dosyayı ({1}) silmek istediğinizden emin misiniz? [e/H]",
            ["message.elevated"] = "Yönetici yetkileri gerekli",
            ["message.noFilesFound"] = "Temizlenecek dosya bulunamadı",
            
            ["size.bytes"] = "{0} bayt",
            ["size.kb"] = "{0:F2} KB",
            ["size.mb"] = "{0:F2} MB",
            ["size.gb"] = "{0:F2} GB",
            ["size.tb"] = "{0:F2} TB",
            
            ["option.help"] = "Yardım bilgilerini göster",
            ["option.verbose"] = "Ayrıntılı çıktıyı etkinleştir",
            ["option.quiet"] = "Hata olmayan çıktıları gizle",
            ["option.force"] = "Onay istemeden temizle",
            ["option.dryRun"] = "Dosyaları silmeden deneme çalıştırması yap",
            ["option.categories"] = "Temizleme kategorilerini belirt",
            ["option.exclude"] = "Belirli yolları veya desenleri hariç tut",
            ["option.include"] = "Belirli yolları veya desenleri dahil et",
            ["option.recursive"] = "Dizinleri özyinelemeli işle",
            ["option.parallel"] = "Paralel işlem sayısı",
            ["option.language"] = "Görüntüleme dilini ayarla"
        };

        return resources;
    }
}
