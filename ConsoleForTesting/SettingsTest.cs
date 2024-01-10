using SharpConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppForTesting
{
    public class SettingsTest
    {
        public const string SFile = "SettingsTest.cfg";
        public const string UAssets = "UserAssets";
        private static Configuration settings_file = new();

        public static List<string> PrevConfigs { set; get; }

        public static void BuildConfFile()
        {
            Section General = settings_file["General"];
            General.Add( new Setting("UserAssetsPath") );
            General.Add( new Setting("DEFRADir") );
            General.Add( new Setting("DEFROMPath") );
            General.Add( new Setting("PrevConfig") );
            General.Add( new Setting("AllwaysDesktop") );
            General.Add( new Setting("CpyUserIcon") );
            General.Add( new Setting("ConvICONPath") );
            General.Add( new Setting("ExtractIco") );
            General.Add( new Setting("PreferedTheme") );
        }

        public static Settings LoadSettings()
        {
            Settings settings = new();
            if (File.Exists(SFile))
            {
                try
                {
                    Configuration settings_file = Configuration.LoadFromBinaryFile(SFile);
                    settings_file["General"].SetValuesTo(settings);
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

        public static void WriteSettingsFile(Settings settings)
        {
            settings_file["General"].GetValuesFrom(settings);
            settings_file.SaveToBinaryFile(SFile);
        }

        public static Settings GetSettingsTesting()
        {
            Settings settings = new()
            {
                DEFRADir = "blah",
                PrevConfig = true,
                DEFROMPath = "ROMs"
            };
            return settings;
        }
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
            UserAssetsPath = SettingsTest.UAssets;
            DEFRADir = string.Empty;
            DEFROMPath = string.Empty;
            PrevConfig = false;
            AllwaysDesktop = false;
            CpyUserIcon = false;
            ConvICONPath = SettingsTest.UAssets;
            ExtractIco = false;
            PreferedTheme = 0;
        }
    }
}
