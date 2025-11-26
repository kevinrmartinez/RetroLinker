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
        public const string IcoSavRA = "_RA";
        public const string IcoSavROM = "_ROM";
        private const string InvalidDataMessage = "The setting file could not be serialized.";
        private static Settings CachedSettings = new();
        
        public static List<string> PrevConfigs { get; set; } = new();
        public static List<string> LinkCopyPaths { get; set; } = new();

        // TODO: change the literal directories with Path.Combine functions, maybe
        public static string[] WinLinkPathCandidates { get; } =
        [
            FileOps.UserDesktop,
            FileOps.WINPublicDesktop,
            FileOps.UserProfile + "\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs",
            "C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs"
        ];  // Source: https://en.wikipedia.org/wiki/Start_menu
        
        public static string[] LinLinkPathCandidates { get; } =
        [
            FileOps.UserDesktop,
            FileOps.UserProfile + "/.local/share/applications",
            "/usr/local/share/applications",
            "/usr/share/applications"
        ];  // Source: https://askubuntu.com/questions/117341/how-can-i-find-desktop-files
        
        public static Settings GetCachedSettings() => CachedSettings;
        
        // Load & Save
        public static Settings LoadSettings()
        {
            Settings? settings = new();
            if (FileOps.ExistSettingsJsonFile())
            {
                try
                {
                    settings = JsonHelper.Deserialize<Settings>(FileOps.ReadSettingsFile());
                    CachedSettings = settings ?? throw new System.IO.InvalidDataException(InvalidDataMessage);
                    PrevConfigs.AddRange(settings.SavedConfigs);
                    LinkCopyPaths.AddRange(settings.SavedCopyPaths);
                }
                catch (System.Exception e)
                {
                    App.Logger?.LogErro($"There was a error while loading \"{FileOps.SettingFileJson}\"");
                    App.Logger?.LogErro($"{e}\n{e.Message}");
                    settings = new();
                    App.Logger?.LogInfo($"Creating/Overwriting \"{FileOps.SettingFileJson}\"...");
                    WriteSettings(settings);
                }  
            }
            else WriteSettings(settings);
            return settings;
        }

        public static void WriteSettings(Settings savingSettings)
        {
            if (!savingSettings.PrevConfig) PrevConfigs = new();
            if (!savingSettings.MakeLinkCopy) LinkCopyPaths = new(); 
            savingSettings.SavedConfigs = PrevConfigs;
            savingSettings.SavedCopyPaths =  LinkCopyPaths;
            CachedSettings = savingSettings;
            var fileString = JsonHelper.Serialize(savingSettings);
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
        public byte ChosenTheme { get; set; } = 0;
        public string LanguageLocale { get; set; } = DefaultLanguage;
        // TODO: Obsolete?
        public bool LinDesktopPopUp { get; set; } = true;
        public List<string> SavedConfigs { get; set; } = new();
        public List<string> SavedCopyPaths { get; set; } = new();
        
        private static readonly string DefaultLanguage = LanguageManager.ENLocale.Name;

        public Settings() {
            IcoSavPath = UserAssetsPath; 
        }

        public void SetDefaultLanguage() => LanguageLocale = DefaultLanguage;
        
        public string GetBase64()
        {   // Solution thanks to Kevin Driedger @ Stackoverflow.com
            var jsonString = JsonHelper.Serialize(this);
            var object64 = Utils.GenerateBase64(jsonString);
            return object64;
        }
    }
}
