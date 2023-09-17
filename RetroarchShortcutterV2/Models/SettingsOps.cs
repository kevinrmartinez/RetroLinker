using Avalonia.Styling;
using SharpConfig;
using System;
using System.Collections.Generic;
using System.IO;

namespace RetroarchShortcutterV2.Models
{
    public class SettingsOps
    {
        public static List<string> PrevConfigs { set; get; }
        private static Configuration settings_file = new();
        private static Section GeneralSettings = settings_file["GeneralSettings"];
        private static Section StoredConfigs = settings_file["StoredConfigs"];
        private static Settings CachedSettings { set; get; } = new();

        public static void BuildConfFile()
        {
            var settings_props = Utils.ExtractClassProperties(typeof(Settings));
            foreach (string setting in settings_props)
            {
                GeneralSettings.Add(setting);
            }
            // GeneralSettings.Add(new Setting("UserAssetsPath"));
            // GeneralSettings.Add(new Setting("DEFRADir"));
            // GeneralSettings.Add(new Setting("DEFROMPath"));
            // GeneralSettings.Add(new Setting("PrevConfig"));
            // GeneralSettings.Add(new Setting("AllwaysDesktop"));
            // GeneralSettings.Add(new Setting("CpyUserIcon"));
            // GeneralSettings.Add(new Setting("ConvICONPath"));
            // GeneralSettings.Add(new Setting("ExtractIco"));
            // GeneralSettings.Add(new Setting("PreferedTheme"));
            // GeneralSettings.Add(new Setting("LinDesktopPopUp"));
            System.Diagnostics.Debug.WriteLine($"el campo GeneralSettings para setting_file creado con {GeneralSettings.SettingCount} subcmpos.", "[Debg]");
        }

        public static Settings LoadSettings()
        {
            Settings settings = new();
            if (FileOps.ExistSettingsBinFile())
            {
                System.Diagnostics.Trace.WriteLine($"Comenzando la carga de {FileOps.SettingFileBin}.", "[Info]");
                try
                {
                    Configuration settings_file = Configuration.LoadFromBinaryFile(FileOps.SettingFileBin);
                    settings_file.Add(GeneralSettings);
                    GeneralSettings.SetValuesTo(settings);
                    if (string.IsNullOrEmpty(settings.UserAssetsPath))
                    { throw new InvalidDataException($"El archivo {FileOps.SettingFileBin} no es valido."); }
                    GeneralSettings.SetValuesTo(CachedSettings);

                    settings_file.Add(StoredConfigs);
                    int dir_count = StoredConfigs.SettingCount;
                    if (dir_count > 0)
                    {
                        PrevConfigs = new List<string>();
                        for (int i = 0; i < dir_count; i++)
                        { PrevConfigs.Add(StoredConfigs[i].StringValue); }
                        FileOps.ConfigDir.AddRange(PrevConfigs);
                        System.Diagnostics.Trace.WriteLine($"Archivo {FileOps.SettingFileBin} cargado exitosamente.", "[Info]");
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Trace.WriteLine($"El archivo {FileOps.SettingFileBin} no se puedo cargar correctamente, sobreescribiendo...", "[Info]");
                    System.Diagnostics.Debug.WriteLine($"En SettingsOps, el elemento {e.Source} a retornado el error {e.Message}", "[Erro]");
                    settings = new(); WriteSettingsFile(settings);
                }
                
            }
            else
            { WriteSettingsFile(settings); }
            return settings;
        }

        public static Settings LoadSettings(Stream file_s)
        {
            Settings settings = new();
            if (file_s != Stream.Null)
            {
                try
                {
                    Configuration settings_file = Configuration.LoadFromBinaryStream(file_s);
                    settings_file.Add(GeneralSettings);
                    GeneralSettings.SetValuesTo(settings);
                    GeneralSettings.SetValuesTo(CachedSettings);

                    settings_file.Add(StoredConfigs);
                    int dir_count = StoredConfigs.SettingCount;
                    if (dir_count > 0)
                    {
                        PrevConfigs = new List<string>();
                        for (int i = 0; i < dir_count; i++)
                        { PrevConfigs.Add(StoredConfigs[i].StringValue); }
                        FileOps.ConfigDir.AddRange(PrevConfigs);
                    }
                }
                catch
                {
                    _ = "El archivo setting no se puedo cargar correctamente, sobreescribiendo...";
                    settings = new(); WriteSettingsFile(settings);
                }
            }
            else { WriteSettingsFile(settings); }
            return settings;
        }

        public static Settings GetCachedSettings() => CachedSettings;

        public static void WriteSettingsFile(Settings settings)
        {
            try 
            {
                GeneralSettings.GetValuesFrom(settings);
                if (PrevConfigs != null)
                {
                    int dir_count = PrevConfigs.Count;
                    string key;
                    for (int i = 0; i < dir_count; i++)
                    {
                        key = $"dir{i}"; StoredConfigs[key].StringValue = PrevConfigs[i];
                    }
                }
                CachedSettings = settings;
                settings_file.SaveToBinaryFile(FileOps.SettingFileBin);
                System.Diagnostics.Trace.WriteLine($"Archivo {FileOps.SettingFileBin} creado exitosamente.", "[Info]");
            }
            catch (Exception e)
            { System.Diagnostics.Trace.WriteLine("Incapaz de escribir el archivo!", "[Erro]");
                System.Diagnostics.Debug.WriteLine($"En SettingsOps, el elemento {e.Source} a retornado el error {e.Message}", "[Erro]");
                CachedSettings = new();
            }
        }

        public static ThemeVariant LoadThemeVariant()
        {
            ThemeVariant theme = CachedSettings.PreferedTheme switch
            {
                1 => ThemeVariant.Light,
                2 => ThemeVariant.Dark,
                _ => ThemeVariant.Default,
            };
            return theme;
        }


#if DEBUG
        public static void TestfillPrevConfigs()
        {
            PrevConfigs = new() { "esto", "es", "una", "prueba", "cambio."};
        }
#endif
    }


    public class Settings
    {
        public string UserAssetsPath { get; set; }
        public string DEFRADir { get; set; }
        public string DEFROMPath { get; set; }
        public bool PrevConfig { get; set; }
        public bool AllwaysDesktop { get; set; }
        public bool CpyUserIcon { get; set; }
        public string ConvICONPath { get; set; }
        public bool ExtractIco { get; set; }
        public byte PreferedTheme { get; set; }
        public bool LinDesktopPopUp { get; set; }

        public Settings()
        {
            UserAssetsPath = FileOps.DefUserAssetsDir;
            DEFRADir = string.Empty;
            DEFROMPath = string.Empty;
            PrevConfig = false;
            AllwaysDesktop = false;
            CpyUserIcon = false;
            ConvICONPath = FileOps.DefUserAssetsDir;
            ExtractIco = false;
            PreferedTheme = 0;
            LinDesktopPopUp = true;
        }

        //public void Dispose() => this.Dispose();
    }
}
