using ImageMagick;
using System;
using System.Drawing;
using System.IO;
using TsudaKageyu;

namespace WinFunc
{
    public class WinIconProc
    {
        public static MemoryStream ExtractIco(string DIR)
        {
            // Extraer el mayor resolucion!
            var stream = new MemoryStream();
            var IcoExtracr = new IconExtractor(DIR);
            int index = IcoExtracr.Count - 1;
            Icon exeIco = IcoExtracr.GetIcon(index);          
            exeIco.Save(stream);
            return stream;
        }




#if DEBUG
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
#endif
    }
}
