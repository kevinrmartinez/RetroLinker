using ImageMagick;
using System;
using System.Collections.Generic;
using System.IO;

namespace RetroarchShortcutterV2.Models.Icons
{
    public class IconProc
    {
        const int MaxRes = 256; // Magick no permite trabajar icos mas grandes...
        public static List<IconsItems> IconItemsList { get; set; }

        static WinFuncImport.WinFuncMethods ExtractIco;

        public static MagickImage ImageConvert(string DIR)
        {
            var ICO = new MagickImage(DIR)
            { Format = MagickFormat.Ico };
            ICO = ResizeToIco(ICO);
            return ICO;
        }

        public static MagickImage ImageConvert(MemoryStream IMG)
        {
            var ICO = new MagickImage(IMG)
            { Format = MagickFormat.Ico };
            ICO = ResizeToIco(ICO);
            return ICO;
        }

        public static MagickImage ResizeToIco(MagickImage ICO)
        {
            var ICOGeo = new MagickGeometry(MaxRes)
            { IgnoreAspectRatio = false };
            bool IsSquare = (ICO.BaseHeight == ICO.BaseWidth);
            //if (ICO.Height > MaxRes || ICO.Width > MaxRes) { ICO.Resize(ICOGeo); }
            //else { ICO.Scale(ICOGeo); }
            ICO.Scale(ICOGeo);
            if (!IsSquare) { ICO.Extent(ICOGeo, Gravity.Center, MagickColors.Transparent); }
            return ICO;
        }

        public static MemoryStream GetStream(string DIR)
        {
            // Fijar el background transparente antes de leer lar imagen. Solucion gracias a Micah y Mateen Ulhaq en stackoverflow.com
            var IMG = new MagickImage()
            { BackgroundColor = MagickColors.Transparent };
            var ImgStream = new MemoryStream();
            IMG.Read(DIR);
            IMG.Format = MagickFormat.Png32;
            IMG.Write(ImgStream);
            return ImgStream;
        }

        public static void BuildIconItem(string file_path, int new_index, bool OS)
        {
            string file_name = Path.GetFileName(file_path);
            string file_ext = Path.GetExtension(file_path);
            file_path = Path.GetFullPath(file_path);
            var ico_item = new IconsItems(file_name, file_path, new_index);
            
            if (FileOps.IsVectorImage(file_path)) { ico_item.IconStream = GetStream(file_path); }
            //if (FileOps.IsWinEXE(file_path)) { }

            if (OS)
            {
                if (FileOps.IsWinEXE(file_path)) { ico_item.IconStream = IcoExtraction(file_path); }
                if (FileOps.WinConvertibleIconsExt.Contains("*" + file_ext) || file_ext == ".exe")
                { ico_item.ConvertionRequiered = true; }
            }
            IconItemsList.Add(ico_item);
        }

        #region Windows Only
        public static void StartImport() => ExtractIco = WinFuncImport.FuncLoader.GetIcoExtractMethod();

        public static MemoryStream IcoExtraction(string DIR)
        {
            //var iconStream = new MemoryStream();
            var Args = new object[1] { DIR };
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
        /// Converts a MemoryStream into a MagickImage of the .ico format
        /// </summary>
        /// <param name="iconStream">The MemoryStream that is going to be converted</param>
        /// <returns>A MagickImage instance in the .ico format</returns>
        public static MagickImage SaveIcoToMagick(MemoryStream iconStream) => new MagickImage(iconStream, MagickFormat.Ico);
        #endregion


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
