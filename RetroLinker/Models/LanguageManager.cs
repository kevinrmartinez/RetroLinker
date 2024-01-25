using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace RetroLinker.Models;

public static class LanguageManager
{
    public static readonly CultureInfo ENLocale = new CultureInfo("en_US");
    public static readonly CultureInfo ESLocale = new CultureInfo("es_ES");
    
    private const string ENIcon = "avares://RetroLinkerLib/Assets/Icons/EN.png";
    private const string ESIcon = "avares://RetroLinkerLib/Assets/Icons/ES.png";
    
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
    
    public static CultureInfo ResolveLocale(LanguageItem languageItem) => languageItem.Culture;

    public static LanguageItem ResolveLocale(int index)
    {
        // Defaults to en-US
        LanguageItem item = LanguageList[0];
        for (int i = 0; i < LanguageList.Count; i++)
        {
            if (LanguageList[i].ItemIndex == index)
            {
                item = LanguageList[i];
                break;
            }
        }
        return item;
    }

    public static LanguageItem ResolveLocale(CultureInfo cultureInfo)
    {
        int i;
        for (i = 0; i < LanguageList.Count; i++)
        {
            if (LanguageList[i].Culture.Equals(cultureInfo)) break;
        }

        return LanguageList[i];
    }
    
    public static int GetLocaleIndex(Settings settings)
    {
        var item = ResolveLocale(settings.LanguageCulture);
        return (int)item.ItemIndex;
    }
    
    public static bool SetLocale(CultureInfo cultureInfo) => ChangeRuntimeLocale(cultureInfo);

    public static bool SetLocale(Settings settings) => ChangeRuntimeLocale(settings.LanguageCulture);

    private static bool ChangeRuntimeLocale(CultureInfo cultureInfo)
    {
        var sameLocale = (Translations.resMainView.Culture.Equals(cultureInfo));
        if (!sameLocale)
        {
            Translations.resAvaloniaOps.Culture = cultureInfo;
            Translations.resMainView.Culture = cultureInfo;
            Translations.resSettingsWindow.Culture = cultureInfo;
        }
        return sameLocale;
    }

    public static void FixLocale(CultureInfo cultureInfo)
    {
        Translations.resAvaloniaOps.Culture = cultureInfo;
        Translations.resMainView.Culture = cultureInfo;
        Translations.resSettingsWindow.Culture = cultureInfo;
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