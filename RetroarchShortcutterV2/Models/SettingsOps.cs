using Avalonia.Styling;
using SharpConfig;
using System.Collections.Generic;

namespace RetroarchShortcutterV2.Models
{
    public class SettingsOps
    {
        public static List<string> PrevConfigs { set; get; }
        private static Configuration settings_file = new();
        private static Settings MemSettings { set; get; } = new();

        public static void BuildConfFile()
        {
            Section GeneralSettings = settings_file["GeneralSettings"];
            GeneralSettings.Add(new Setting("UserAssetsPath"));
            GeneralSettings.Add(new Setting("DEFRADir"));
            GeneralSettings.Add(new Setting("DEFROMPath"));
            GeneralSettings.Add(new Setting("PrevConfig"));
            GeneralSettings.Add(new Setting("AllwaysDesktop"));
            GeneralSettings.Add(new Setting("CpyUserIcon"));
            GeneralSettings.Add(new Setting("ConvICONPath"));
            GeneralSettings.Add(new Setting("ExtractIco"));
            GeneralSettings.Add(new Setting("PreferedTheme"));
        }

        public static Settings LoadSettings()
        {
            Settings settings = new();
            if (FileOps.ExistSettingsBinFile())
            {
                try
                {
                    Configuration settings_file = Configuration.LoadFromBinaryFile(FileOps.SettingFileBin);
                    Section GeneralSettings = settings_file["GeneralSettings"];
                    GeneralSettings.SetValuesTo(settings);
                    GeneralSettings.SetValuesTo(MemSettings);

                    Section StoredConfigs = settings_file["StoredConfigs"];
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
            else
            { WriteSettingsFile(settings); }
            return settings;
        }

        public static Settings GetMemSettings() => MemSettings;

        public static void WriteSettingsFile(Settings settings)
        {
            try
            {
                MemSettings = settings;
                settings_file["GeneralSettings"].GetValuesFrom(settings);

                Section StoredConfigs = settings_file["StoredConfigs"];
                if (PrevConfigs != null)
                {
                    int dir_count = PrevConfigs.Count;
                    string key;
                    for (int i = 0; i < dir_count; i++)
                    {
                        key = "dir" + i; StoredConfigs[key].StringValue = PrevConfigs[i];
                    }
                }

                settings_file.SaveToBinaryFile(FileOps.SettingFileBin);
            }
            catch
            { _ = "Icapaz de salver el archivo!"; }
        }

        public static ThemeVariant LoadThemeVariant()
        {
            ThemeVariant theme = MemSettings.PreferedTheme switch
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
        }
    }
}
