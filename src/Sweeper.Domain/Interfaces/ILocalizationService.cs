using System.Globalization;

namespace Sweeper.Domain.Interfaces;

public interface ILocalizationService
{
    string GetString(string key);
    string GetString(string key, params object[] args);
    string GetString(string key, CultureInfo culture);
    string GetString(string key, CultureInfo culture, params object[] args);
    void SetCulture(string cultureName);
    void SetCulture(CultureInfo culture);
    CultureInfo GetCurrentCulture();
    List<CultureInfo> GetAvailableCultures();
    bool IsCultureSupported(string cultureName);
    bool IsCultureSupported(CultureInfo culture);
}
