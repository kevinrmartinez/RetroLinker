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
    // TODO: give the user an option for Nearest sampling
    private static readonly SKSamplingOptions NearestResampler = new(SKFilterMode.Nearest, SKMipmapMode.Nearest); 
    private static readonly SKSamplingOptions LinearResampler = new(SKFilterMode.Linear, SKMipmapMode.Linear); 
    // https://learn.microsoft.com/en-us/windows/win32/uxguide/vis-icons
    private static readonly IReadOnlyList<int> _icoAppSizes = [ 16, 32, 48, 256 ];

    public static SKBitmap ResizeAndPad(SKBitmap sourceBitmap, int targetHeightWidth)
    {
        // If the image is already square, there's no need for padding
        if (sourceBitmap.Height == sourceBitmap.Width) {
            return sourceBitmap.Resize(new SKSizeI(targetHeightWidth, targetHeightWidth), LinearResampler);
        }
        
        /* Disclosure: This part was mostly provided by Gemini (the LLM) */
        // 1. Calculate scale factors to fit maintaining aspect ratio
        float scaleX = (float)targetHeightWidth / sourceBitmap.Width;
        float scaleY = (float)targetHeightWidth / sourceBitmap.Height;
        float scale = Math.Min(scaleX, scaleY);
        
        // 2. Compute the scaled dimensions
        float scaledWidth = sourceBitmap.Width * scale;
        float scaledHeight = sourceBitmap.Height * scale;
        
        // 3. Compute padding offsets to center the image
        float left = (targetHeightWidth - scaledWidth) / 2;
        float top = (targetHeightWidth - scaledHeight) / 2;
        
        // 4. Create the target bitmap with the exact requested dimensions
        var info = new SKImageInfo(targetHeightWidth, targetHeightWidth, SKColorType.Bgra8888, SKAlphaType.Premul);
        var targetBitmap = new SKBitmap(info);
        
        // 5. Prepare a Canvas and Rectangle for drawing the target at the center of the Canvas
        using var canvas = new SKCanvas(targetBitmap);
        canvas.Clear(SKColors.Transparent);
        var destRect = new SKRect(left, top, left + scaledWidth, top + scaledHeight);
        
        // 6. Resized the original bitmap with Linear filtering
        using var resized = sourceBitmap.Resize(new SKSizeI((int)scaledWidth, (int)scaledHeight), LinearResampler);
        
        // 7. Draw the source bitmap into the destination rectangle
        canvas.DrawBitmap(resized, destRect);

        return targetBitmap;
    }
    
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
            else if (FileOps.IsVectorImage(path)) {             // If it's a vector image, the raw file won't work
                memoryStream = await iconItem.GetImageStream();
                ogFilePath = iconItem.FilePath;
            }
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

    public static byte[] VirtuallyCreateIco(MemoryStream imageStream) {
        using var bitmap = SKBitmap.Decode(imageStream);
        var ico = new IcoContainer(bitmap, _icoAppSizes);
        return ico.ToBytes();
    }

    #endregion
}