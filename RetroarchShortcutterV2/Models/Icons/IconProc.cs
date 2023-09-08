using ImageMagick;
using ImageMagick.ImageOptimizers;
using System;
using System.Collections.Generic;
using System.IO;

namespace RetroarchShortcutterV2.Models.Icons
{
    public class IconProc
    {
        public static List<IconsItems> IconItemsList { get; set; }


        const int MaxRes = 256; // Magick no permite trabajar icos mas grandes...
        static WinFuncImport.WinFuncMethods ExtractIco;

        public static void StartImport() => ExtractIco = WinFuncImport.FuncLoader.GetIcoExtractMethod();

        public static MagickImage ImageConvert(string DIR)
        {
            var ICO = new MagickImage(DIR)
            { Format = MagickFormat.Ico };
            var ICOGeo = new MagickGeometry(MaxRes);
            bool IsSquare = (ICO.BaseHeight == ICO.BaseWidth);

            if (ICO.Height > MaxRes || ICO.Width > MaxRes) { ICO.Resize(ICOGeo); }
            else { ICO.Scale(ICOGeo); }
            if (!IsSquare) { ICO.Extent(ICOGeo, Gravity.Center, MagickColors.Transparent); }
            return ICO;
        }

        public static MemoryStream GetStream(string DIR)
        {
            var IMG = new MagickImage(DIR)
            { ColorSpace = ColorSpace.sRGB, HasAlpha = true, BackgroundColor = null, Format = MagickFormat.Png};
            var ImgStream = new MemoryStream();
            IMG.Write(ImgStream);
            return ImgStream;
        }

        public static MemoryStream ResizeStream(MemoryStream IMG)
        {
            IMG.Position = 0;
            var mIMG = new MagickImage(IMG, MagickFormat.Png);
            IMG = new();
            var IMGgeo = new MagickGeometry(MaxRes)
            { IgnoreAspectRatio = false };
            mIMG.Scale(IMGgeo);
            mIMG.Write(IMG);
            return IMG;
        }

        public static MemoryStream IcoExtraction(string DIR)
        {
            //var iconStream = new MemoryStream();
            var Args = new object[]
                { DIR };
            try
            {
                MemoryStream icoMS = ExtractIco.MInfo.Invoke(ExtractIco.ObjInstance, Args) as MemoryStream;
                return icoMS;
            }
            catch { Console.WriteLine("WinFunc no pude extraer el icono!"); Console.Beep(); return new MemoryStream(); }
            // PENDIENTE: Mostrar msbox indicando que hay un problema
        }

        /* Posible implementacion para extraer un icono especifico de un .exe o .dll */
        //public static MemoryStream IcoFullExtraction(string DIR)
        //{
        //    var iconStream = new MemoryStream();

        //    return iconStream;
        //}

        /// <summary>
        /// Convert a MemoryStream into a MagickImage of the .ico format
        /// </summary>
        /// <param name="iconStream">The MemoryStream that is going to be converted</param>
        /// <returns>A MagickImage instance in the .ico format</returns>
        public static MagickImage SaveIcoToMI(MemoryStream iconStream)
        {
            var ico = new MagickImage(iconStream, MagickFormat.Ico);
            return ico;
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
