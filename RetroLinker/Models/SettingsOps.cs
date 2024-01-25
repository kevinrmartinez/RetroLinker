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
using System.Text;

namespace RetroLinker.Models
{
    public static class SettingsOps
    {
        private const string StartMark = "[START]";
        private const string EndMark = "[END]";
        private const string GeneralHeader = "[GENERAL]";
        private const string PrevConfigsHeader = "[PrevConfigs]";
        private const string LinkCopyPathsHeader = "[LinkCopyPaths]";
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
        

        public static void BuildConfFile()
        {
            // TODO: Replace SharpConfig as it is umpredictible at the moment
            WriteSettings(new Settings());
        }

        public static Settings LoadSettings()
        {
            Settings settings = new();
            int generalCount = Utils.ExtractClassProperties(typeof(Settings)).Count;
            
            return settings;
        }

        public static Settings GetCachedSettings() => CachedSettings;

        public static void WriteSettings(Settings savingSettings)
        {
            string fileString = GeneralHeader + "\n";
            fileString += StartMark + "\n";
            fileString += savingSettings.GetSavingString();
            fileString += EndMark + "\n";

            fileString += "\n" + PrevConfigsHeader + "\n";
            fileString += StartMark + "\n";
            fileString += Utils.GetStringFromList(PrevConfigs) + "\n";
            fileString += EndMark + "\n";
            
            fileString += "\n" + LinkCopyPathsHeader + "\n";
            fileString += StartMark + "\n";
            fileString += Utils.GetStringFromList(LinkCopyPaths) + "\n";
            fileString += EndMark + "\n";
            
            FileOps.WriteSettingsFile(fileString);
        }
        
#if DEBUG
        public static void TestfillPrevConfigs()  => PrevConfigs = new() { "esto", "es", "una", "prueba,", "cambio."};
#endif
    }


    public class Settings
    {
        public string UserAssetsPath { get; set; } = FileOps.DefUserAssetsDir;
        public string DEFRADir { get; set; } = string.Empty;
        public string DEFROMPath { get; set; } = string.Empty;
        public bool PrevConfig { get; set; } = false;
        public bool AllwaysAskOutput { get; set; } = true;
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

        public void SetDefaultLaunguage()
        { LanguageCulture = DEFLanguage; }
        
        //public void Dispose() => this.Dispose();
        
        public string GetBase64()
        {   // Solucion gracias a Kevin Driedger en Stackoverflow.com 
            var objectString = GetString();
            var object64 = GenerateBase64(objectString);
            return object64;
        }

        public string GetSavingString() => GetString();
        
        private string GetString()
        {
            // It may not be smart, but if it works...
            string objectString = 
                $"{UserAssetsPath}\n" +
                $"{DEFRADir}\n" +
                $"{DEFROMPath}\n";
            objectString += (PrevConfig)       ? "1\n" : "0\n";
            objectString += (AllwaysAskOutput) ? "1\n" : "0\n";
            objectString += (MakeLinkCopy)     ? "1\n" : "0\n"; 
            objectString += (CpyUserIcon)      ? "1\n" : "0\n";
            objectString += $"{IcoSavPath}\n";
            objectString += (ExtractIco)       ? "1\n" : "0\n";
            objectString += (IcoLinkName)      ? "1\n" : "0\n";
            objectString += PreferedTheme.ToString();
            objectString += $"{LanguageCulture.Name}\n";
            objectString += (LinDesktopPopUp)  ? "1\n" : "0\n";
            return objectString;
        }
        
        private string GenerateBase64(string objectString)
        {   // Solucion gracias a Kevin Driedger en Stackoverflow.com 
            var objectBytes = Encoding.UTF8.GetBytes(objectString);
            var object64 = System.Convert.ToBase64String(objectBytes);
            return object64;
        }
    }
}
