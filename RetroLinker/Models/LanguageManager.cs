using System;
using System.Globalization;
using System.IO;

namespace RetroLinker.Models;

public static class LanguageManager
{
    private static readonly CultureInfo ENLocale = new CultureInfo("en_US");
    private static readonly CultureInfo ESLocale = new CultureInfo("es_ES");
    
    private const string ENIcon = "avares://RetroLinkerLib/Assets/Icons/EN.png";
    private const string ESIcon = "avares://RetroLinkerLib/Assets/Icons/ES.png";
    
    public enum AvailableLocale
    {
        EN0, ES0
    }

    public static LanguageItem EnglishItem = new LanguageItem
    (
        "English",
        new Uri(ENIcon)
    )
    {
        DefaultLocale = true
    };

    public static LanguageItem SpanishItem = new LanguageItem
    (
        "Español",
        new Uri(ESIcon)
    );

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

public class LanguageItem
{
    public string Name { get; set; }
    public Uri LangIconPath { get; set; }
    public int? ItemIndex { get; set; }
    public bool DefaultLocale { get; set; } = false;

    public LanguageItem(string name, Uri langIconPath)
    {
        Name = name;
        LangIconPath = langIconPath;
    }

    public LanguageItem(string name, Uri langIconPath, int itemIndex)
    {
        Name = name;
        LangIconPath = langIconPath;
        ItemIndex = itemIndex;
    }
}