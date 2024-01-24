using System;
using System.Collections.Generic;
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

    public static List<LanguageItem> LanguageList = new()
    {
        new LanguageItem
        (
            "English",
            ENLocale,
            new Uri(ENIcon)
        )
        { DefaultLocale = true },
        new LanguageItem
        (
            "Español",
            ESLocale,
            new Uri(ESIcon)
        )
    };

    public static void SetLocale(AvailableLocale locale)
    {
        CultureInfo selLocale = locale switch
        {
            AvailableLocale.EN0 => ENLocale,
            AvailableLocale.ES0 => ESLocale,
            _ => ENLocale
        };
        ChangeRuntimeLocale(selLocale);
    }

    public static bool ChangeRuntimeLocale(object cultureInfo)
    {
        var locale = cultureInfo as CultureInfo;
        var sameLocale = (Translations.resAvaloniaOps.Culture == locale);
        if (!sameLocale)
        {
            Translations.resAvaloniaOps.Culture = locale;
            Translations.resMainView.Culture = locale;
            Translations.resSettingsWindow.Culture = locale;
        }
        return sameLocale;
    }
}

public class LanguageItem
{
    public string Name { get; set; }
    public CultureInfo Culture { get; set; }
    public Uri LangIconPath { get; set; }
    public int? ItemIndex { get; set; }
    public bool DefaultLocale { get; set; } = false;

    public LanguageItem(string name, CultureInfo culture, Uri langIconPath)
    {
        Name = name;
        Culture = culture;
        LangIconPath = langIconPath;
    }

    public LanguageItem(string name, CultureInfo culture, Uri langIconPath, int itemIndex)
    {
        Name = name;
        Culture = culture;
        LangIconPath = langIconPath;
        ItemIndex = itemIndex;
    }
}