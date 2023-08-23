using ImageMagick;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace RetroarchShortcutterV2.Models
{
    public class IconProc
    {
        const int MaxRes = 256; // Magick no permite trabajar icos mas grandes...
        static WinFuncImport.WinFuncMethods ExtractIco; 

        public static void StartImport()
        {
            ExtractIco = WinFuncImport.FuncLoader.GetIcoExtractMethod();
        }

        public static MemoryStream PngConvert(string DIR)
        {
            var iconStream = new MemoryStream();
            var PNG = new MagickImage(DIR);
            
            int sizeMajor = int.Max(PNG.Width, PNG.Height);
            if (PNG.Height > MaxRes || PNG.Width > MaxRes)
            { PNG.InterpolativeResize(MaxRes, MaxRes, PixelInterpolateMethod.Bilinear); }
            else
            {
                int i;
                for (i = MaxRes; i > sizeMajor; i /= 2) { }
                PNG.AdaptiveResize(i, i);
            }
            PNG.Write(iconStream, MagickFormat.Ico);
            return iconStream;
        }

        public static MemoryStream IcoExtraction(string DIR)
        {
            var iconStream = new MemoryStream();
            var Args = new object[]
                { DIR };
            iconStream = ExtractIco.mInfo.Invoke(ExtractIco.objInstance, Args) as MemoryStream;
            return iconStream;
        }

        public static MemoryStream IcoFullExtraction(string DIR)
        {
            var iconStream = new MemoryStream();

            return iconStream;
        }

        public static string SaveIcoToFile(string filename, string saveDir, MemoryStream iconStream)
        {
            var ico = new MagickImage(iconStream);
            string DIR = Path.Combine(saveDir, filename);
            ico.Write(DIR);
            return DIR;
        }



#if DEBUG
        public static void TestWriteToFile()
        {
            var image = new MagickImage("E:\\CommonDevAssets\\ava1.1.png");

            int sizeMajor = int.Max(image.Width, image.Height);
            if (image.Height > MaxRes || image.Width > MaxRes)
            { image.InterpolativeResize(MaxRes, MaxRes, PixelInterpolateMethod.Bilinear); }
            else
            {
                int i;
                for (i = MaxRes; i > sizeMajor; i /= 2) { }
                image.AdaptiveResize(i, i);
            }
            image.Write("E:\\CommonDevAssets\\zero-icon.ico", MagickFormat.Ico);
        }
#endif
    }
}
