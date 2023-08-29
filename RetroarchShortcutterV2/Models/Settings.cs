using Avalonia.Styling;
using SharpConfig;
using System.Collections.Generic;

namespace RetroarchShortcutterV2.Models
{
    public class Settings
    {
        public static string DEFRADir { get; set; }
        public static string DEFROMPath { get; set; }
        public static bool PrevConfig { get; set; }
        public static bool AllwaysDesktop { get; set; }
        public static bool CpyUserIcon { get; set; }
        public static string ConvICONPath { get; set; }
        public static bool ExtractIco { get; set; }
        public static byte PreferedTheme { get; set; }

        public static List<string> PrevConfigs { set; get; }

        public static void LoadSettings()
        {
            if (FileOps.ExistSettingsBinFile())
            {
                try
                {
                    Configuration settings_file = Configuration.LoadFromBinaryFile(FileOps.SettingFileBin);
                    Section GeneralSettings = settings_file["GeneralSettings"];
                    Section StoredConfigs = settings_file["StoredConfigs"];

                    DEFRADir = GeneralSettings["DEFRADir"].StringValue;
                    DEFROMPath = GeneralSettings["DEFROMPath"].StringValue;
                    PrevConfig = GeneralSettings["PrevConfig"].BoolValue;
                    AllwaysDesktop = GeneralSettings["AllwaysDesktop"].BoolValue;
                    CpyUserIcon = GeneralSettings["CpyUserIcon"].BoolValue;
                    ConvICONPath = GeneralSettings["ConvICONPath"].StringValue;
                    ExtractIco = GeneralSettings["ExtractIco"].BoolValue;
                    PreferedTheme = GeneralSettings["PreferedTheme"].ByteValue;

                    int dir_count = StoredConfigs.SettingCount;
                    if (dir_count > 0)
                    {
                        PrevConfigs = new List<string>();
                        for (int i = 0; i < dir_count; i++)
                        { PrevConfigs.Add(StoredConfigs[i].StringValue); }
                    }
                }
                catch
                {
                    _ = "El archivo setting no se puedo cargar correctamente, sobreescribiendo...";
                    LoadSettingsDefault(); WriteSettingsFile();
                }
                
            }
            else
            { LoadSettingsDefault(); WriteSettingsFile(); }
        }

        public static void LoadSettingsDefault()
        {
            DEFRADir = string.Empty;
            DEFROMPath = string.Empty;
            PrevConfig = false;
            AllwaysDesktop = false;
            CpyUserIcon = false;
            ConvICONPath = FileOps.UserAssetsDir;
            ExtractIco = false;
            PreferedTheme = 0;
        }

        public static void SetSettings(byte theme, string defRADir, string defROMPath, bool prevConf,
                                       bool desktop, bool cpyUserIcon, string convIcoDir, bool extractIco)
        {
            DEFRADir = defRADir;
            DEFROMPath = defROMPath;
            PrevConfig = prevConf;
            AllwaysDesktop = desktop;
            CpyUserIcon = cpyUserIcon;
            ConvICONPath = convIcoDir;
            ExtractIco = extractIco;
            PreferedTheme = theme;
        }

        public static void WriteSettingsFile()
        {
            Configuration settings_file = new Configuration();
            Section GeneralSettings = settings_file["GeneralSettings"];
            Section StoredConfigs = settings_file["StoredConfigs"];

            GeneralSettings["DEFRADir"].StringValue = DEFRADir;
            GeneralSettings["DEFROMPath"].StringValue = DEFROMPath;
            GeneralSettings["PrevConfig"].BoolValue = PrevConfig;
            GeneralSettings["AllwaysDesktop"].BoolValue = AllwaysDesktop;
            GeneralSettings["CpyUserIcon"].BoolValue = CpyUserIcon;
            GeneralSettings["ConvICONPath"].StringValue = ConvICONPath;
            GeneralSettings["ExtractIco"].BoolValue = ExtractIco;
            GeneralSettings["PreferedTheme"].ByteValue = PreferedTheme;
            
            //TestfillPrevConfigs();

            if (PrevConfigs != null)
            {
                int dir_count = PrevConfigs.Count;
                string key;
                for (int i = 0; i < dir_count; i++)
                {
                    key = "dir" + i; StoredConfigs[key].StringValue = PrevConfigs[i];
                }
            }

            //settings_file.SaveToFile(FileOps.SettingFile);
            settings_file.SaveToBinaryFile(FileOps.SettingFileBin);
        }

        public static ThemeVariant LoadThemeVariant()
        {
            ThemeVariant theme;
            switch(PreferedTheme)
            {
                case 1:
                    theme = ThemeVariant.Light;
                    break;
                case 2:
                    theme = ThemeVariant.Dark;
                    break;
                default:
                    theme = ThemeVariant.Default;
                    break;
            }
            return theme;
        }


#if DEBUG
        public static void TestfillPrevConfigs()
        {
            PrevConfigs = new() { "esto", "es", "una", "prueba", "cambio."};
        }
#endif
    }
}
