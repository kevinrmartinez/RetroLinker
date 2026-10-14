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
using System.Threading.Tasks;
using RetroLinker.Models.IO;

namespace RetroLinker.Models.Settings;

public static class SettingsOps
{
    // TODO: Create a static 'Settings' for the whole program (>=0.9)
    public const string IcoSavRA = "_RA";
    public const string IcoSavROM = "_ROM";
    private const string InvalidDataMessage = "The setting file could not be serialized.";
    private static Settings CachedSettings = new();
        
    public static List<string> PrevConfigs { get; set; } = new();
    public static List<string> LinkCopyPaths { get; set; } = new();
    public static IReadOnlyList<string> RaEnvironmentVariables { get; } = [
        "LIBRETRO_ASSETS_DIRECTORY",
        "LIBRETRO_AUTOCONFIG_DIRECTORY",
        "LIBRETRO_CHEATS_DIRECTORY",
        "LIBRETRO_DATABASE_DIRECTORY",
        "LIBRETRO_DIRECTORY",
        "LIBRETRO_SYSTEM_DIRECTORY",
        "LIBRETRO_VIDEO_FILTER_DIRECTORY", 
        "LIBRETRO_VIDEO_SHADER_DIRECTORY"
    ];
        
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