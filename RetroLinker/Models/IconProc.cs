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

using System.Collections.Generic;
using System.IO;
using ImageMagick;
using RetroLinker.Models.Windows;
using SkiaSharp;

namespace RetroLinker.Models
{
    public static class IconProc
    {
        const int MaxRes = 256; // Magick can't work with bigger .ico files...
        public static List<IconsItems> IconItemsList { get; } = new();

        public static MagickImage ImageConvert(string path) => ResizeToIco(new MagickImage(path) {Format = MagickFormat.Ico});

        public static MagickImage ImageConvert(MemoryStream img) => ResizeToIco(new MagickImage(img) {Format = MagickFormat.Ico});

        private static MagickImage ResizeToIco(MagickImage ico)
        {
            // ICOGeo is a square geometry equals to MaxRes
            var ICOGeo = new MagickGeometry(MaxRes) {IgnoreAspectRatio = false};
            // IsSquare: true if the image is a square (Height = Width)
            bool IsSquare = (ico.BaseHeight == ico.BaseWidth);
            
            // Transform the image into the geometry ICOGeo, if it's not square it will rescale into its original Aspect Ratio
            ico.Scale(ICOGeo);
             /* if it's not square, the image will be extended into MaxRes, it will be centered,
             *  and it will be in front of a transparent background */
            if (!IsSquare) ico.Extent(ICOGeo, Gravity.Center, MagickColors.Transparent);
            return ico;
        }

        private static MemoryStream GetStream(string path)
        {
            // Set the background transparent before reading the image.
            // Solution thanks to Micah y Mateen Ulhaq @ stackoverflow.com
            var IMG = new MagickImage() {BackgroundColor = MagickColors.Transparent};
            
            IMG.Read(path);
            IMG.Format = MagickFormat.Png32;
            var ImgStream = new MemoryStream();
            IMG.Write(ImgStream);
            return ImgStream;
        }

        public static void BuildIconItem(string filePath, int newIndex, bool OS)
        {
            filePath = FileOps.GetAbsolutePath(filePath);
            var fileExt = FileOps.GetFileExtFromPath(filePath);
            var icoItem = new IconsItems(filePath, newIndex);
            
            if (FileOps.IsVectorImage(filePath)) icoItem.IconStream = GetStream(filePath);
            if (OS)
            {
                if (FileOps.IsFileWinPE(filePath)) icoItem.IconStream = ExtractIco(filePath, 0);
                if (FileOps.WinExtraIconsExt.Contains($"*{fileExt}") || (FileOps.IsExtWinPE(fileExt))) 
                    icoItem.ConversionRequired = true;
            }
            IconItemsList.Add(icoItem);
        }

        #region Windows Only
        private static MemoryStream ExtractIco(string path, int index)
        {
            var icoExtractor = new IconExtractor(path);
            var icoCount = icoExtractor.Count;
            if (index >= icoCount) index -= icoCount;
            using var rawIco = icoExtractor.GetIcon(index);
            using var skImage = SKImage.FromBitmap(SKBitmap.Decode(rawIco));
            using var skDecode = skImage.Encode();
            if (skDecode is not null)
            {
                var icoStream = new MemoryStream();
                skDecode.SaveTo(icoStream);
                return icoStream;
            }
            else return new MemoryStream();
        }
        #endregion
        
    }
    
    
    public class IconsItems
    {
        public string FileName { get; private set; }

        public string FilePath
        {
            get => filePath;
            set => SetFilePath(value);
        }
        public MemoryStream? IconStream { get; set; }
        public int? comboIconIndex { get; set; }
        public bool ConversionRequired { get; set; }

        private string filePath = string.Empty;
        
        public IconsItems() {
            FileName = string.Empty;
            FilePath = string.Empty;
            ConversionRequired = false;
        }

        public IconsItems(string filePath, bool conversionRequired = false) {
            FileName = string.Empty;
            FilePath = filePath;
            ConversionRequired = conversionRequired;
        }

        public IconsItems(string filePath, int? comboIconInd, bool conversionRequired = false) : this(filePath, conversionRequired) {
            comboIconIndex = comboIconInd;
        }

        private void SetFilePath(string value) {
            filePath = value;
            FileName = (string.IsNullOrWhiteSpace(value)) ? string.Empty : FileOps.GetFileNameNoExtFromPath(filePath);
        }
        
        public bool HasSameFile(IconsItems? other) => (other != null) && (FilePath == other.FilePath);
    }
}
