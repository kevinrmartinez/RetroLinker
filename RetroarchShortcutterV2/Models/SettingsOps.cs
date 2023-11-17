using SharpConfig;
using System.Collections.Generic;

namespace RetroarchShortcutterV2.Models
{
    public class SettingsOps
    {
        private static Configuration settings_file = new();
        private static Section GeneralSettings = new("GeneralSettings");
        private static Section StoredConfigs = new("StoredConfigs");
        private static Settings CachedSettings = new();
        
        public static List<string>? PrevConfigs { set; get; }

        public static void BuildConfFile()
        {
            settings_file.Add(GeneralSettings);
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
            System.Diagnostics.Debug.WriteLine($"el campo GeneralSettings para setting_file creado con {GeneralSettings.SettingCount} subcampos.", "[Debg]");
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
                    Configuration settings_file = LoadConfiguration(FileOps.SettingFileBin);
                    // Manejo de los settings generales, presentes en la clase Settings
                    GeneralSettings = settings_file["GeneralSettings"];
                    GeneralSettings.SetValuesTo(settings);
                    // Manejo de error: en caso de que el archivo se lea incorrectamente, los campos not-null apareceran null
                    if (string.IsNullOrEmpty(settings.UserAssetsPath))
                    { throw new System.IO.InvalidDataException($"El archivo {FileOps.SettingFileBin} no es valido."); }
                    GeneralSettings.SetValuesTo(CachedSettings);

                    StoredConfigs = settings_file["StoredConfigs"];
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
            catch (System.Exception e)
            { 
                System.Diagnostics.Trace.WriteLine($"Incapaz de escribir el archivo {FileOps.SettingFileBin}!", "[Erro]");
                System.Diagnostics.Debug.WriteLine($"En SettingsOps, el elemento {e.Source} a retornado el error {e.Message}", "[Erro]");
                CachedSettings = new();
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
            objectstring += (PrevConfig) ? "1" : "0";
            objectstring += (AllwaysAskOutput) ? "1" : "0";
            objectstring += (MakeLinkCopy) ? "1" : "0"; 
            objectstring += (CpyUserIcon) ? "1" : "0";
            objectstring += (ExtractIco) ? "1" : "0";
            objectstring += (IcoLinkName) ? "1" : "0";
            objectstring += (LinDesktopPopUp) ? "1" : "0";
            return objectstring;
        }
        
        public string GetBase64()
        {   // Solucion gracias a Kevin Driedger en Stackoverflow.com 
            var objectstring = GetString();
            var objectBytes = System.Text.Encoding.UTF8.GetBytes(objectstring);
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
