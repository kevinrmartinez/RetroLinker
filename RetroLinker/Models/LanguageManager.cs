/*
    RetroLinker: A .NET GUI application to help create desktop links of games running on RetroArch.
    Copyright (C) 2024  Kevin Rafael Martinez Johnston

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RetroLinker.Translations;

namespace RetroLinker.Models;

public static class LanguageManager
{
    public static readonly CultureInfo ENLocale = new("en_US");
    public static readonly CultureInfo ESLocale = new("es_ES");
    
    private const string ENIcon = "avares://RetroLinkerLib/Assets/Icons/EN.png";
    private const string ESIcon = "avares://RetroLinkerLib/Assets/Icons/ES.png";

    public static readonly List<LanguageItem> LanguageList =
    [
        new("English", ENLocale, new Uri(ENIcon)) { DefaultLocale = true },
        new("Español", ESLocale, new Uri(ESIcon))
    ];

    public static CultureInfo ResolveCulture(LanguageItem languageItem) => languageItem.Culture;
    
    public static CultureInfo ResolveCulture(string cultureName) =>
        LanguageList.Find(l => l.Culture.Name == cultureName)?.Culture ?? ENLocale;

    public static LanguageItem ResolveLocale(CultureInfo cultureInfo) => 
        LanguageList.Find(l => l.Culture.Name == cultureInfo.Name) ?? LanguageList.First(l => l.DefaultLocale);
    
    public static LanguageItem ResolveLocale(int index) =>
        LanguageList.Find(l => l.ItemIndex == index) ?? LanguageList.First(l => l.DefaultLocale);
    
    public static int GetLocaleIndex(Settings settings)
    {
        var cultureInfo = ResolveCulture(settings.LanguageLocale);
        var item = ResolveLocale(cultureInfo);
        return item.ItemIndex.GetValueOrDefault();
    }
    
    public static bool SetLocale(CultureInfo cultureInfo) => ChangeRuntimeLocale(cultureInfo);

    public static bool SetLocale(string locale) => ChangeRuntimeLocale(ResolveCulture(locale));

    private static bool ChangeRuntimeLocale(CultureInfo cultureInfo) {
        var sameLocale = resMainView.Culture.Equals(cultureInfo);
        if (!sameLocale) SetAllCultureInfo(cultureInfo);
        return sameLocale;
    }

    public static void FixLocale(CultureInfo cultureInfo) => SetAllCultureInfo(cultureInfo);

    private static void SetAllCultureInfo(CultureInfo cultureInfo)
    {
        // https://docs.avaloniaui.net/docs/app-development/localizing
        resAboutWindow.Culture = cultureInfo;
        resAvaloniaOps.Culture = cultureInfo;
        resGeneric.Culture = cultureInfo;
        resMainExtras.Culture = cultureInfo;
        resMainView.Culture = cultureInfo;
        resSettingsWindow.Culture = cultureInfo;
    }
}

public class LanguageItem
{
    public string Name { get; set; }
    public CultureInfo Culture { get; set; }
    public Uri LangIconPath { get; set; }
    public int? ItemIndex { get; set; }
    public bool DefaultLocale { get; set; } = false;

    public LanguageItem(string name, CultureInfo culture, Uri langIconPath) {
        Name = name;
        Culture = culture;
        LangIconPath = langIconPath;
    }
}