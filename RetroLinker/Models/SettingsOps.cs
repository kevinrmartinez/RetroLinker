/*
    RetroLinker: A .NET GUI application to help create desktop links of games running on RetroArch.
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
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using RetroLinker.Models.Generic;

namespace RetroLinker.Models
{
    public static class SettingsOps
    {
        // TODO: Create a static 'Settings' for the whole program (>=0.9)
        public const string IcoSavRA = "_RA";
        public const string IcoSavROM = "_ROM";
        private const string InvalidDataMessage = "The setting file could not be serialized.";
        private static Settings CachedSettings = new();
        
        public static List<string> PrevConfigs { get; set; } = new();
        public static List<string> LinkCopyPaths { get; set; } = new();
        
        public static Settings GetCachedSettings() => CachedSettings;
        
        // Load & Save
        public static async Task<Settings> LoadSettings()
        {
            Settings? settings = new();
            if (FileOps.ExistSettingsJsonFile())
            {
                try
                {
                    settings = JsonHelper.DeserializeProxy<Settings>(await FileOps.ReadSettingsFile());
                    CachedSettings = settings ?? throw new System.IO.InvalidDataException(InvalidDataMessage);
                    PrevConfigs.AddRange(settings.SavedConfigs);
                    LinkCopyPaths.AddRange(settings.SavedCopyPaths);
                }
                catch (System.Exception e)
                {
                    Logger.LogWarn($"There was a error while loading \"{FileOps.SettingFileJson}\"");
                    Logger.LogErro(e);
                    settings = new();
                    Logger.LogInfo($"Creating/Overwriting \"{FileOps.SettingFileJson}\"...");
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
            var serializedSettings = JsonHelper.SerializeProxy(savingSettings);
            if (!string.IsNullOrEmpty(serializedSettings)) FileOps.WriteSettingsFile(serializedSettings);
            else Logger.LogErro("Settings could not be serialized into a file");
        }
    }
    
    
    public class Settings : LocalSerializable, System.ICloneable
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
        public string? TileIcoPath { get; set; }
        public byte ChosenTheme { get; set; } = 0;
        public string LanguageLocale { get; set; } = DefaultLanguage;
        public List<string> SavedConfigs { get; set; } = new();
        public List<string> SavedCopyPaths { get; set; } = new();
        
        private static readonly string DefaultLanguage = LanguageManager.ENLocale.Name;

        public Settings() {
            IcoSavPath = UserAssetsPath; 
        }

        public object Clone() => this.MemberwiseClone();

        public void SetDefaultLanguage() => LanguageLocale = DefaultLanguage;
        
        public string? GetBase64()
        {   // Solution thanks to Kevin Driedger @ Stackoverflow.com
            var jsonString = JsonHelper.SerializeProxy(this);
            if (string.IsNullOrEmpty(jsonString)) return null;
            var object64 = Utils.GenerateBase64(jsonString);
            return object64;
        }
    }
    
    [JsonSerializable(typeof(Settings))]
    internal partial class SettingsSerializerContext : JsonSerializerContext {
        // I believe this can be left empty only because Settings uses primitives as properties
    }
}
