using System;
using System.Collections.Generic;
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


    }
}
