using System.Globalization;

namespace RetroLinker.Models;

public static class LanguageManager
{
    private static readonly CultureInfo ENLocale = new CultureInfo("en_US");
    private static readonly CultureInfo ESLocale = new CultureInfo("es_ES");
    
    public enum AvailableLocale
    {
        EN0, ES0
    }

    public static void ChangeRuntimeLocale(AvailableLocale locale)
    {
        CultureInfo selLocale = locale switch
        {
            AvailableLocale.EN0 => ENLocale,
            AvailableLocale.ES0 => ESLocale,
            _ => ENLocale
        };
        Translations.resAvaloniaOps.Culture = selLocale;
        Translations.resMainView.Culture = selLocale;
        Translations.resSettingsWindow.Culture = selLocale;
    }
}