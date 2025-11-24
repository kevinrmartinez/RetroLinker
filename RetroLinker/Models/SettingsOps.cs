/*
    A .NET GUI application to help create desktop links of games running on RetroArch.
    Copyright (C) 2023  Kevin Rafael Martinez Johnston

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

using System.Collections.Generic;
using System.Globalization;
using RetroLinker.Models.Generic;

namespace RetroLinker.Models
{
    public static class SettingsOps
    {
        private const string StartMark = "[START]";
        private const string EndMark = "[END]";
        private const string GeneralHeader = "[GENERAL]";
        private const string PrevConfigsHeader = "[PrevConfigs]";
        private const string LinkCopyPathsHeader = "[LinkCopyPaths]";
        private const string InvalidDataMessage = "Invalid settings file!";
        private static Settings CachedSettings = new();
        
        public static List<string> PrevConfigs { get; set; } = new();
        public static List<string> LinkCopyPaths { get; set; } = new();

        public static string[] WINLinkPathCandidates { get; } = new[]
        {
            FileOps.UserDesktop,
            FileOps.WINPublicDesktop,
            FileOps.UserProfile + "\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs",
            "C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs"
        };  // Source: https://en.wikipedia.org/wiki/Start_menu
        
        public static string[] LINLinkPathCandidates { get; } = new[]
        {
            FileOps.UserDesktop,
            FileOps.UserProfile + "/.local/share/applications",
            "/usr/local/share/applications",
            "/usr/share/applications"
        };  // Source: https://askubuntu.com/questions/117341/how-can-i-find-desktop-files

        public const string IcoSavROM = "_ROM";
        public const string IcoSavRA = "_RA";
        
        private static bool GeneralInPlace(string[] settingFile, int headerIndex, int generalCount)
        {
            bool startCorrect = (settingFile[headerIndex + 1] == StartMark);
            bool endCorrect = (settingFile[headerIndex + generalCount + 2] == EndMark);
            
            return startCorrect && endCorrect;
        }

        private static string[] GetSubSectionsValues(string[] settingFile, int sectIndex)
        {
            var valueList = new List<string>();
            int valueCount = SubSectionCount(settingFile, sectIndex);
            if (valueCount > 0)
            {
                for (int i = 0; i < valueCount; i++)
                {
                    int offset = sectIndex + 2 + i;
                    valueList.Add(settingFile[offset]);
                }
            }
            
            return valueList.ToArray();
        }

        private static int SubSectionCount(string[] settingFile, int sectIndex)
        {
            int configsCount = 0;
            bool validSection = false;
            for (int i = sectIndex + 2; i < settingFile.Length; i++)
            {
                configsCount++;
                if (settingFile[i] != EndMark) continue;
                validSection = true;
                break;
            }
            configsCount -= 1;
            
            if ((settingFile[sectIndex + 1] != StartMark) || !validSection) throw new System.IO.InvalidDataException(InvalidDataMessage);
            return configsCount;
        }

        private static bool ResolveBool(string value)
        {
            _ = int.TryParse(value, out var valueInt);
            return valueInt switch
            {
                0 => false,
                1 => true,
                _ => throw new System.IO.InvalidDataException(InvalidDataMessage)
            };
        }

        private static int ResolveNumber(string value)
        {
            bool parsed = int.TryParse(value, out var valueInt);
            if (!parsed) throw new System.IO.InvalidDataException(InvalidDataMessage);
            return valueInt;
        }

        public static Settings LoadSettings()
        {
            Settings settings = new();
            int generalCount = Utils.ExtractClassProperties(typeof(Settings)).Count;

            // TODO: Explain this mess...
            if (FileOps.ExistSettingsBinFile())
            {
                try
                {
                    var settingFile = FileOps.ReadSettingsFile();
                    int headerIndex;
                    for (headerIndex = 0; headerIndex < settingFile.Length; headerIndex++)
                    { if (settingFile[headerIndex] == GeneralHeader) break; }
                    if (GeneralInPlace(settingFile, headerIndex, generalCount))
                    {
                        int i = headerIndex + 1;
                        settings.UserAssetsPath = FileOps.ResolveSettingUA(settingFile[++i]);
                        settings.DEFRADir = settingFile[++i];
                        settings.DEFROMPath = settingFile[++i];
                        // TODO: Save all bools as a single 8-bit number (byte) (0.8)
                        settings.PrevConfig = ResolveBool(settingFile[++i]);
                        settings.AlwaysAskOutput = ResolveBool(settingFile[++i]);
                        settings.DEFLinkOutput = settingFile[++i];
                        settings.MakeLinkCopy = ResolveBool(settingFile[++i]);
                        settings.CpyUserIcon = ResolveBool(settingFile[++i]);
                        settings.IcoSavPath = settingFile[++i];
                        settings.ExtractIco = ResolveBool(settingFile[++i]);
                        settings.IcoLinkName = ResolveBool(settingFile[++i]);
                        settings.PreferedTheme = (byte)ResolveNumber(settingFile[++i]);
                        settings.SetLanguage(LanguageManager.ResolveLocale(settingFile[++i]));
                        settings.LinDesktopPopUp = ResolveBool(settingFile[++i]);
                    }
                    else
                    { throw new System.IO.InvalidDataException(InvalidDataMessage); }
                    CachedSettings = settings;
                    
                    int prevIndex;
                    for (prevIndex = headerIndex + generalCount; prevIndex < settingFile.Length; prevIndex++)
                    { if (settingFile[prevIndex] == PrevConfigsHeader) break; }
                    PrevConfigs.AddRange(GetSubSectionsValues(settingFile, prevIndex));

                    int linkCopyIndex;
                    for (linkCopyIndex = prevIndex + PrevConfigs.Count; linkCopyIndex < settingFile.Length; linkCopyIndex++)
                    { if (settingFile[linkCopyIndex] == LinkCopyPathsHeader) break; }
                    LinkCopyPaths.AddRange(GetSubSectionsValues(settingFile, linkCopyIndex));
                    
                }
                catch (System.Exception e)
                {
                    App.Logger?.LogErro($"There was a error while loading \"{FileOps.SettingFileBin}\"");
                    App.Logger?.LogErro($"{e}\n{e.Message}");
                    settings = new();
                    App.Logger?.LogInfo($"Creating/Overwriting \"{FileOps.SettingFileBin}\"...");
                    WriteSettings(settings);
                }  
            }
            else
            { WriteSettings(settings); }
            return settings;
        }

        public static Settings GetCachedSettings() => CachedSettings;

        public static void WriteSettings(Settings savingSettings)
        {
            // TODO: Explain this mess...
            string fileString = GeneralHeader + "\n";
            fileString += StartMark + "\n";
            fileString += savingSettings.GetSavingString();
            fileString += EndMark + "\n";

            fileString += "\n" + PrevConfigsHeader + "\n";
            fileString += StartMark + "\n";
            if (!savingSettings.PrevConfig) PrevConfigs = new List<string>();
            fileString += Utils.GetStringFromList(PrevConfigs);
            fileString += EndMark + "\n";
            
            fileString += "\n" + LinkCopyPathsHeader + "\n";
            fileString += StartMark + "\n";
            if (!savingSettings.MakeLinkCopy) LinkCopyPaths = new List<string>();
            fileString += Utils.GetStringFromList(LinkCopyPaths);
            fileString += EndMark + "\n";

            CachedSettings = savingSettings;
            FileOps.WriteSettingsFile(fileString);
        }
    }


    public class Settings
    {
        public string UserAssetsPath { get; set; } = FileOps.DefUserAssetsDir;
        public string DEFRADir { get; set; } = string.Empty;
        public string DEFROMPath { get; set; } = string.Empty;
        public bool PrevConfig { get; set; } = false;
        public bool AlwaysAskOutput { get; set; } = true;
        public string DEFLinkOutput { get; set; } = string.Empty;
        public bool MakeLinkCopy { get; set; } = false;
        public bool CpyUserIcon { get; set; } = false;
        public string IcoSavPath { get; set; }
        public bool ExtractIco { get; set; } = false;
        public bool IcoLinkName { get; set; } = false;
        public byte PreferedTheme { get; set; } = 0;
        public CultureInfo LanguageCulture { get; private set; } = DEFLanguage;
        public bool LinDesktopPopUp { get; set; } = true;


        private static readonly CultureInfo DEFLanguage = LanguageManager.ENLocale;
        public Settings()
        { IcoSavPath = UserAssetsPath; }
        
        public void SetLanguage(CultureInfo availableLocale)
        { LanguageCulture = availableLocale; }
        
        public void SetLanguage(LanguageItem languageItem)
        { LanguageCulture = LanguageManager.ResolveLocale(languageItem); }

        public void SetDefaultLanguage()
        { LanguageCulture = DEFLanguage; }
        
        //public void Dispose() => this.Dispose();
        
        public string GetBase64()
        {   // Solution thanks to Kevin Driedger @ Stackoverflow.com
            var objectString = GetString();
            var object64 = Utils.GenerateBase64(objectString);
            return object64;
        }

        public string GetSavingString() => GetString();
        
        private string GetString()
        {
            // TODO: There should be a better way... like JSON (0.8)
            string objectString = 
                $"{UserAssetsPath}\n" +
                $"{DEFRADir}\n" +
                $"{DEFROMPath}\n";
            objectString += (PrevConfig)       ? "1\n" : "0\n";
            objectString += (AlwaysAskOutput) ? "1\n" : "0\n";
            objectString += $"{DEFLinkOutput}\n";
            objectString += (MakeLinkCopy)     ? "1\n" : "0\n"; 
            objectString += (CpyUserIcon)      ? "1\n" : "0\n";
            objectString += $"{IcoSavPath}\n";
            objectString += (ExtractIco)       ? "1\n" : "0\n";
            objectString += (IcoLinkName)      ? "1\n" : "0\n";
            objectString += $"{PreferedTheme.ToString()}\n";
            objectString += $"{LanguageCulture.Name}\n";
            objectString += (LinDesktopPopUp)  ? "1\n" : "0\n";
            return objectString;
        }
    }
}
