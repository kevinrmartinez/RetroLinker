/*
    A .NET GUI application to help create desktop links of games running on RetroArch.
    Copyright (C) 2023  Kevin Rafael Martinez Johnston

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.IO;
using ImageMagick;
using RetroLinker.Models.WinFuncImport;

namespace RetroLinker.Models.Icons
{
    public static class IconProc
    {
        const int MaxRes = 256; // Magick can't work with bigger .ico files...
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
            // TODO: Below here deserves an explanation
            bool IsSquare = (ICO.BaseHeight == ICO.BaseWidth);
            ICO.Scale(ICOGeo);
            if (!IsSquare) { ICO.Extent(ICOGeo, Gravity.Center, MagickColors.Transparent); }
            return ICO;
        }

        public static MemoryStream GetStream(string DIR)
        {
            // Set the background transparent before reading the image.
            // Solution thanks to Micah y Mateen Ulhaq @ stackoverflow.com
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
            if (OS)
            {
                if (FileOps.IsWinEXE(file_path)) { ico_item.IconStream = IcoExtraction(file_path); }
                if (FileOps.WinConvertibleIconsExt.Contains($"*{file_ext}") || file_ext == ".exe")
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
            catch
            {
                System.Diagnostics.Trace.WriteLine($"{FuncLoader.WinOnlyLib} couldn't extract the desired Icon!", App.ErroTrace);
                return new MemoryStream();
            }
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
        
    }
    
    
    public class IconsItems : IEquatable<IconsItems?>
    {
        public string? FileName { get; set; }
        public string FilePath { get; set; }
        public MemoryStream? IconStream { get; set; }
        public int? comboIconIndex { get; set; }
        public bool ConvertionRequiered { get; set; }

        public IconsItems() { }

        public IconsItems(string fileName, string filePath, bool convertionRequiered = false) 
        {
            FileName = fileName;
            FilePath = filePath;
            ConvertionRequiered = convertionRequiered;
        }

        public IconsItems(string? fileName, string filePath, int? comboIconInd, bool convertionRequiered = false) : this(fileName, filePath, convertionRequiered)
        { comboIconIndex = comboIconInd; }

        public IconsItems(string fileName, string filePath, MemoryStream? iconStream, int? comboIconInd, bool convertionRequiered = false) : this(fileName, filePath, comboIconInd, convertionRequiered)
        { IconStream = iconStream; }


        public override bool Equals(object? obj) => Equals(obj as IconsItems);

        public bool Equals(IconsItems? other) => (other != null) && (FilePath == other.FilePath);
    }
}
