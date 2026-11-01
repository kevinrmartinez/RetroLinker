/*
    RetroLinker: A .NET GUI application to help create desktop links of games running on RetroArch.
    Copyright (C) 2023-2026 Kevin Rafael Martinez Johnston

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

using System.IO;
using SkiaSharp;
using Svg.Skia;

namespace RetroLinker.Models.IO;

public static partial class IconProc
{
    // https://learn.microsoft.com/en-us/windows/win32/uxguide/vis-icons
    private static IReadOnlyList<int> _icoAppSizes = [ 16, 32, 48, 256 ];
    
    // public static MagickImage ImageConvert(string path) => ResizeToIco(new MagickImage(path) {Format = MagickFormat.Ico});
    //
    // public static MagickImage ImageConvert(MemoryStream img) => ResizeToIco(new MagickImage(img) {Format = MagickFormat.Ico});
    //     
    // public static MagickImage ReverseImageConvert(string path) => new(path) {Format = MagickFormat.Png};
    //     
    // public static MagickImage ReverseImageConvert(MemoryStream img) => new(img) {Format = MagickFormat.Png};
    //
    // private static MagickImage ResizeToIco(MagickImage ico)
    // {
    //     // ICOGeo is a square geometry equals to MaxRes
    //     var ICOGeo = new MagickGeometry(IcoMaxRes) {IgnoreAspectRatio = false};
    //     // IsSquare: true if the image is a square (Height = Width)
    //     bool IsSquare = (ico.BaseHeight == ico.BaseWidth);
    //         
    //     // Transform the image into the geometry ICOGeo, if it's not square it will rescale into its original Aspect Ratio
    //     ico.Scale(ICOGeo);
    //     /* if it's not square, the image will be extended into MaxRes, it will be centered,
    //      *  and it will be in front of a transparent background */
    //     if (!IsSquare) ico.Extent(ICOGeo, Gravity.Center, MagickColors.Transparent);
    //     return ico;
    // }
    //
    // private static MemoryStream GetStream(MagickImage img) {
    //     var imgStream = new MemoryStream();
    //     img.Write(imgStream);
    //     imgStream.Position = 0;
    //     return imgStream;
    // }
    //
    
    public static MemoryStream EncodeAsPngMk2(SKBitmap bitmap)
    {
        // This method bellow is confirmed to run ~4x faster than the method above, even with a little extra compression
        // Thanks to mjblacker on GitHub: https://github.com/mono/SkiaSharp/issues/2757#issuecomment-2424337566
        using var tempImage = SKImage.FromBitmap(bitmap);
        var ms = new MemoryStream();
        var pixmap = tempImage.PeekPixels();
        pixmap.Encode(ms, new SKPngEncoderOptions(SKPngEncoderFilterFlags.None, zLibLevel: 1));
        ms.Position = 0;
        return ms;
    }

    public static MemoryStream EncodeAsPngMk2(string path) => EncodeAsPngMk2(SKBitmap.Decode(path));
    public static MemoryStream EncodeAsPngMk2(MemoryStream img) => EncodeAsPngMk2(SKBitmap.Decode(img));

    public static MemoryStream DecodeSvg(Stream svgFileStream)
    {
        var ms = new MemoryStream();
        using var tempSvg = new SKSvg();
        var picture = tempSvg.Load(svgFileStream);
        if (picture is not null)
        {
            var biggerSide = Math.Max(picture.CullRect.Width, picture.CullRect.Height);
            float scaleXY = 1 / (biggerSide / IcoMaxRes);
            using var bitmap = picture.ToBitmap(SKColors.Empty, scaleXY, scaleXY, SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb());
            if (bitmap is not null) ms = EncodeAsPngMk2(bitmap);
        }
        return ms;
    }

    #region Windows Only

    public static bool IsIcoConversionRequired(string fileExtension) 
        => !fileExtension.Equals(FileOps.WinIcoExt, StringComparison.OrdinalIgnoreCase);
    
    [SupportedOSPlatform(App.PlatformWin)]
    public static MemoryStream ExtractIco(string path, int index)
    {
        var icoExtractor = new Windows.IconExtractor(path);
        var icoCount = icoExtractor.Count;
        if (index >= icoCount) index -= icoCount;
        return icoExtractor.GetIcon(index);
    }
    
    public static async Task<string> ResolveImageForTileico(string iconFile, string? tileIcoImage, IIconItem iconItem)
    {
        MemoryStream? memoryStream = null;
        string? ogFilePath = null;
        var imageFromIco = string.Empty;

        if (!string.IsNullOrWhiteSpace(tileIcoImage)) {
            // if a conversion to .ico happened, 'outputLink.TileIcoImage' has either: an image file, or a Windows PE
            var path = tileIcoImage;
            if (FileOps.IsFileWinPE(path)) {                    // If it's a Windows PE...
                if (FileOps.IsFileWinPE(iconFile)) {            //  and was NOT extracted
                    memoryStream = await iconItem.GetImageStream();
                    ogFilePath = iconItem.FilePath;
                }
                else {                                          //  and was extracted
                    ogFilePath = iconFile;
                }
            }
            // else if (FileOps.IsVectorImage(path)) {             // If it's a vector image, the raw file won't work?
            //     memoryStream = await iconItem.GetImageStream();
            //     ogFilePath = iconItem.FilePath;
            // }
            else imageFromIco = path;   // If it's an image file, just return that
        }
        else if (iconItem is { ConversionRequired: false }) {
            // if a conversion did not happen, then 'outputLink.IconPath' is a .ico file
            memoryStream = await iconItem.GetImageStream();
            ogFilePath = iconItem.FilePath;
        }

        if (string.IsNullOrEmpty(imageFromIco))
        {
            MemoryStream? image = null;
            if (memoryStream is not null) {
                memoryStream.Position = 0;
                image = EncodeAsPngMk2(memoryStream);
                await memoryStream.DisposeAsync();
            }
            else if (!string.IsNullOrWhiteSpace(ogFilePath)) {
                image = EncodeAsPngMk2(ogFilePath);
            }

            if (image is not null) {
                var newName = FileOps.GenerateTempFileName("png");
                imageFromIco = await FileOps.DumpStreamToFile(image, newName);
                await image.DisposeAsync();
            }
        }

        return imageFromIco;
    }

    public static byte[] VirtuallyCreateIco(MemoryStream icoStream) {
        using var bitmap = SKBitmap.Decode(icoStream);
        var ico = new IcoContainer(bitmap, _icoAppSizes);
        return ico.ToBytes();
    }

    #endregion
}