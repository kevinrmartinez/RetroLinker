using ImageMagick;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFunc
{
    public class WinIconProc
    {
        const string tempIco = "temp.ico";
        public static string writeIcoDIR;

        public static MemoryStream ExtractIco(string DIR)
        {
            var stream = new MemoryStream();
            Icon exeIco = Icon.ExtractAssociatedIcon(DIR);
            exeIco.Save(stream);
            return stream;
        }


        public static string SaveIcoToTempFile(MemoryStream iconStream)
        {

            var ico = new MagickImage(iconStream);
            ico.Write(tempIco);
            string DIR = Path.GetFullPath(tempIco);
            return DIR;
        }

        public static string SaveIcoToFile(MemoryStream iconStream)
        {

            var ico = new MagickImage(iconStream);
            ico.Write(writeIcoDIR);
            string DIR = Path.GetFullPath(writeIcoDIR);
            return DIR;
        }


        // Testing
        public static void TestConvert(string DIR)
        {
            var PNG = new MagickImage(DIR);
            string APPDATApath = Environment.GetEnvironmentVariable("LOCALAPPDATA") + "\\RetroShortcutter\\";
            PNG.Format = MagickFormat.Ico;
            PNG.Resize(256, 256);
            Directory.CreateDirectory(APPDATApath);
            PNG.Write(APPDATApath + "zero.ico");
        }
    }
}
