using ImageMagick;
using System;
using System.IO;

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

        public static string SaveIcoToFile(string savePath, MemoryStream iconStream, string? altPath)
        {
            var ico = new MagickImage(iconStream, MagickFormat.Ico);
            if (FileOps.CheckUsrSetDir()) { ico.Write(savePath); }
            else 
            { 
                Console.WriteLine("No existe la carpeta para escribir, alternando..."); 
                ico.Write(altPath); return altPath;
            }
            return savePath;
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
