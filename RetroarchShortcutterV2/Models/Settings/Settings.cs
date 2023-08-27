using SharpConfig;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroarchShortcutterV2.Models.Settings
{
    public class Settings
    {
        static public string DEFRADir { get; set; }
        static public string DEFROMPath { get; set; }
        static public bool PrevConfig { get; set; }
        static public string PrevCONFIGPath { get; set; }
        static public bool AllwaysDesktop { get; set; }
        static public bool CpyUserIcon { get; set; }
        static public string ConvICONPath { get; set; }
        static public bool ExtractIco { get; set; }

        static public List<string> PrevConfigs { set; get; }

        static public void LoadSettings()
        {
            FileOps.CheckUsrSetDir();
            Configuration settings_file = Configuration.LoadFromFile(FileOps.SettingFile);
            Section GeneralSettings = settings_file["GeneralSettings"];
            Section StoredConfigs = settings_file["StoredConfigs"];
            Setting setDEFRADir = GeneralSettings.Add("DEFRADir");

            if (FileOps.ChkSettingsFile())
            {
                DEFRADir = GeneralSettings["DEFRADir"].StringValue;
                DEFROMPath = GeneralSettings["DEFROMPath"].StringValue;
                PrevConfig = GeneralSettings["PrevConfig"].BoolValue;
                PrevCONFIGPath = GeneralSettings["PrevCONFIGPath"].StringValue;
                AllwaysDesktop = GeneralSettings["AllwaysDesktop"].BoolValue;
                CpyUserIcon = GeneralSettings["CpyUserIcon"].BoolValue;
                ConvICONPath = GeneralSettings["ConvICONPath"].StringValue;
                ExtractIco = GeneralSettings["ExtractIco"].BoolValue;

                PrevConfigs = new List<string>();
                int dir_count = StoredConfigs.SettingCount;
                if (dir_count > 0)
                {
                    for (int i = 0; i < dir_count; i++)
                    { PrevConfigs.Add(StoredConfigs[i].StringValue); }
                }
            }
            else
            { LoadSettingsDefault(); }
        }

        static public void LoadSettingsDefault()
        {
            DEFRADir = string.Empty;
            DEFROMPath = string.Empty;
            PrevConfig = false;
            PrevCONFIGPath = string.Empty;
            AllwaysDesktop = false;
            CpyUserIcon = false;
            ConvICONPath = FileOps.UserAssetsDir;
            ExtractIco = false;
        }
    }
}
