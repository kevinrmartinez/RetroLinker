using System;
using System.IO;

namespace RetroarchShortcutterV2.Models.WinIco
{
    public class WinIcoStream : IEquatable<WinIcoStream>
    {
        public string ExeDir { get; set; }
        public MemoryStream IconStream { get; set; }
        public int? comboIconIndex { get; set; }

        public WinIcoStream()
        {

        }

        public WinIcoStream(string exeDir, MemoryStream iconStream)
        {
            ExeDir = exeDir;
            IconStream = iconStream;
        }

        public WinIcoStream(string exeDir, MemoryStream iconStream, int comboIndex)
        {
            ExeDir = exeDir;
            IconStream = iconStream;
            comboIconIndex = comboIndex;
        }

        public bool Equals(WinIcoStream other)
        {
            if (other.ExeDir == this.ExeDir)
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
