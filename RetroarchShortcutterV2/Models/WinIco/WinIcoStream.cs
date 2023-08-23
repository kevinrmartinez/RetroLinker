using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroarchShortcutterV2.Models.WinIco
{
    public class WinIcoStream: IEquatable<WinIcoStream>
    {
        public string ExeDir { get; set; }
        public MemoryStream IconStream { get; set; }
        public int comboIconIndex { get; set; }

        public bool Equals(WinIcoStream other)
        {
            if (new WinIcoStream().ExeDir == this.ExeDir)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
    }
}
