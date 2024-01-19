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
using SharpConfig;

namespace RetroLinker.Models
{
    public static class SettingsOps
    {
        private static Configuration settings_file = new();
        private static Section GeneralSettings = new("GeneralSettings");
        private static Section StoredConfigs = new("StoredConfigs");
        private static Section StoredLinkPaths = new("StoredLinkPaths");
        private static Settings CachedSettings = new();

        public static List<string>? PrevConfigs { get; set; } = new();
        public static List<string>? LinkCopyPaths { get; set; } = new();

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
            settings_file.Add(GeneralSettings);
            var settingsProps = Utils.ExtractClassProperties(typeof(Settings));
            foreach (string setting in settingsProps)
            {
                GeneralSettings.Add(setting);
            }
            System.Diagnostics.Debug.WriteLine($"el campo GeneralSettings para setting_file creado con {GeneralSettings.SettingCount} subcampos.", "[Debg]");
            settings_file.Add(StoredConfigs);
            settings_file.Add(StoredLinkPaths);
        }

        private static Configuration LoadConfiguration(string configDIR) => Configuration.LoadFromBinaryFile(configDIR);
        private static Configuration LoadConfiguration(System.IO.Stream configSTREAM) => Configuration.LoadFromBinaryStream(configSTREAM);

        public static Settings LoadSettings()
        {
            Settings settings = new();
            if (FileOps.ExistSettingsBinFile())
            {
                System.Diagnostics.Trace.WriteLine($"Comenzando la carga de {FileOps.SettingFileBin}.", "[Info]");
                try
                {
                    Configuration settingsLoad_file = LoadConfiguration(FileOps.SettingFileBin);
                    // Manejo de los settings generales, presentes en la clase Settings
                    GeneralSettings = settingsLoad_file["GeneralSettings"];
                    GeneralSettings.SetValuesTo(settings);
                    // Manejo de error: en caso de que el archivo se lea incorrectamente, los campos not-null apareceran null
                    if (string.IsNullOrEmpty(settings.UserAssetsPath))
                    { throw new System.IO.InvalidDataException($"El archivo {FileOps.SettingFileBin} no es valido."); }
                    GeneralSettings.SetValuesTo(CachedSettings);

                    PrevConfigs = new List<string>();
                    StoredConfigs = settingsLoad_file["StoredConfigs"];
                    int dir_count = StoredConfigs.SettingCount;
                    if (dir_count > 0)
                    {
                        for (int i = 0; i < dir_count; i++)
                        { PrevConfigs.Add(StoredConfigs[i].StringValue); }
                    }

                    LinkCopyPaths = new List<string>();
                    StoredLinkPaths = settingsLoad_file["StoredLinkPaths"];
                    int path_count = StoredLinkPaths.SettingCount;
                    if (path_count > 0)
                    {
                        for (int i = 0; i < path_count; i++)
                        { LinkCopyPaths.Add(StoredLinkPaths[i].StringValue); }
                    }
                    System.Diagnostics.Trace.WriteLine($"Archivo {FileOps.SettingFileBin} cargado exitosamente.", "[Info]");
                }
                catch (System.Exception e)
                {
                    System.Diagnostics.Trace.WriteLine($"El archivo {FileOps.SettingFileBin} no se puedo cargar correctamente, sobreescribiendo...", "[Erro]");
                    System.Diagnostics.Debug.WriteLine($"En SettingsOps, el elemento {e.Source} a retornado el error {e.Message}", "[Erro]");
                    settings = new(); 
                    WriteSettingsFile(settings);
                }
            }
            else
            { WriteSettingsFile(settings); }
            return settings;
        }

        public static Settings GetCachedSettings() => CachedSettings;

        public static void WriteSettingsFile(Settings savingSettings)
        {
            GeneralSettings.GetValuesFrom(savingSettings);
            if (PrevConfigs.Count > 0)
            {
                int dirCount = PrevConfigs.Count;
                for (int i = 0; i < dirCount; i++)
                {
                    string key = $"dir{i}"; StoredConfigs[key].StringValue = PrevConfigs[i];
                }
            }
            if (LinkCopyPaths.Count > 0)
            {
                int pathCount = LinkCopyPaths.Count;
                for (int i = 0; i < pathCount; i++)
                {
                    string key = $"path{i}"; StoredLinkPaths[key].StringValue = LinkCopyPaths[i];
                }
            }
                
            CachedSettings = savingSettings;
            try 
            {
                settings_file.SaveToBinaryFile(FileOps.SettingFileBin);
                System.Diagnostics.Trace.WriteLine($"Archivo {FileOps.SettingFileBin} creado exitosamente.", "[Info]");
            }
            catch (System.Exception e)
            { 
                System.Diagnostics.Trace.WriteLine($"Incapaz de escribir el archivo {FileOps.SettingFileBin}!", "[Erro]");
                System.Diagnostics.Debug.WriteLine($"En SettingsOps, el elemento {e.Source} a retornado el error {e.Message}", "[Erro]");
                // CachedSettings = new();
            }
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
        public bool LinDesktopPopUp { get; set; } = true;

        public Settings()
        {
            IcoSavPath = UserAssetsPath;
        }
        
        //public void Dispose() => this.Dispose();
        
        private string GetString()
        {
            string objectstring = UserAssetsPath + DEFRADir + DEFROMPath + DEFLinkOutput + IcoSavPath;
            objectstring += PreferedTheme.ToString();
            objectstring += (PrevConfig)       ? "1" : "0";
            objectstring += (AllwaysAskOutput) ? "1" : "0";
            objectstring += (MakeLinkCopy)     ? "1" : "0"; 
            objectstring += (CpyUserIcon)      ? "1" : "0";
            objectstring += (ExtractIco)       ? "1" : "0";
            objectstring += (IcoLinkName)      ? "1" : "0";
            objectstring += (LinDesktopPopUp)  ? "1" : "0";
            return objectstring;
        }
        
        public string GetBase64()
        {   // Solucion gracias a Kevin Driedger en Stackoverflow.com 
            var objectString = GetString();
            var objectBytes = System.Text.Encoding.UTF8.GetBytes(objectString);
            var object64 = System.Convert.ToBase64String(objectBytes);
            return object64;
        }
        
        public string GetBase64(string objectString)
        {   // Solucion gracias a Kevin Driedger en Stackoverflow.com 
            var objectBytes = System.Text.Encoding.UTF8.GetBytes(objectString);
            var object64 = System.Convert.ToBase64String(objectBytes);
            return object64;
        }
    }
}
