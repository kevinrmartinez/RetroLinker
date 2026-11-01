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

namespace RetroLinker.Models.IO;

public static partial class IconProc
{
    private const int IcoReserved = 0;
    private const int IcoMaxRes = 256;
    private const int IconDirEntrySize = 16;
    
    private enum IcoType {
        ICO = (short)1,
        CUR = (short)2
    }
    
    /// <summary> A data Structure representing a valid .ico file. </summary>
    /// <remarks> Working with this may be inneficient, but it's done for undenstanding what's going on here in the future. </remarks>
    private class IcoContainer
    {
        
        // https://en.wikipedia.org/wiki/ICO_(file_format)#Structure
        // ICONDIR structure
        
        /// <summary> Reserved. Must be 0. </summary>
        public short IdReserved { get; }
        
        /// <summary> Image type: 1 for ICO image, 2 for CUR image. Other values are invalid. </summary>
        public IcoType IdType { get; }
        
        /// <summary> Number of images in the file. </summary>
        public short IdCount { get; }
        
        /// <summary> Array of <see cref="IconDirEntry"/>. Each entry represents an image. </summary>
        public IconDirEntry[] IdEntries { get; }

        /// <summary> Collection of Bitmaps present in this file, in exception to the 256×256 frame. </summary>
        public BitmapStructure[] BitmapFrames { get; }
        
        /// <summary> An embedded PNG image for the 256×256 frame. </summary>
        public byte[]? PngFrame { get; }

        private const int IconDirEntryOffset1 = 6;
        private const int IconDirEntryOffset2 = 16;
        
        /// <summary> Construct an ICO file with the provided image and sizes. </summary>
        /// <param name="initialBitmap">The image to convert, provided as a <see cref="SKBitmap"/>.</param>
        /// <param name="sizes">
        /// A list of sizes to convert into, the file will be builded in the same order as this list.
        /// The <see langword="int"/> values will be converted to <see langword="byte"/> with overflow enable.
        /// </param>
        public IcoContainer(SKBitmap initialBitmap, IReadOnlyList<int> sizes)
        {
            IdReserved = IcoReserved;
            IdType = IcoType.ICO;
            IdCount = (short)sizes.Count;
            
            var bitmaps = new List<BitmapStructure>();
            var entries = new List<IconDirEntry>();
            var offset = IconDirEntryOffset1 + (IconDirEntryOffset2 * IdCount);
            foreach (var size in sizes)
            {
                // TODO: Resize
                using var resizedBitmap =
                    initialBitmap.Resize(new SKSizeI(size, size), new SKSamplingOptions(SKFilterMode.Linear));
                int structureLenght = 0;
                if (size < IcoMaxRes) {
                    var bitmapStructure = new BitmapStructure(resizedBitmap); 
                    bitmaps.Add(bitmapStructure);
                    structureLenght = bitmapStructure.SizeInBytes();
                }
                else {
                    PngFrame = EncodeAsPngMk2(resizedBitmap).ToArray();
                    structureLenght = PngFrame.Length;
                }
                entries.Add(new IconDirEntry(size, structureLenght, ref offset));
            }
            
            IdEntries = entries.ToArray();
            BitmapFrames = bitmaps.ToArray();
        }
        
        public MemoryStream ToStream()
        {
            var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            writer.Write(IdReserved);
            writer.Write((short)IdType);
            writer.Write(IdCount);
            foreach (var entry in IdEntries) writer.Write(entry.ToBytes());
            foreach (var bitmapStructure in BitmapFrames) writer.Write(bitmapStructure.ToBytes());
            if (PngFrame is not null) writer.Write(PngFrame);
            writer.Flush();

            stream.Position = 0;
            return stream;
        }
        
        public byte[] ToBytes() => ToStream().ToArray();

        public async Task WriteToFileAsync(string outFile)
        {
            var fileStream = new FileStream(outFile, FileMode.Create);
            await using var writer =  new BinaryWriter(fileStream);
            writer.Write(IdReserved);
            writer.Write((short)IdType);
            writer.Write(IdCount);
            foreach (var entry in IdEntries) writer.Write(entry.ToBytes());
            foreach (var bitmapStructure in BitmapFrames) writer.Write(bitmapStructure.ToBytes());
            if (PngFrame is not null) writer.Write(PngFrame);
            writer.Flush();
            fileStream.Close();
        }
    }
    
    private struct IconDirEntry
    {
        // ICONDIRENTRY structure
        
        /// <summary> Image width in pixels. Can be any number between 0 and 255. 0 means width is 256. </summary>
        public byte Width { get; }
        
        /// <summary> Image height in pixels. Can be any number between 0 and 255. 0 means height is 256. </summary>
        public byte Height { get; }
        
        /// <summary> Number of colors in the color table. For paletted depths: 2 for 1 bpp, 16 for 4 bpp; 0 for 8 bpp and above. </summary>
        public byte ColorCount { get; }
        
        /// <summary> Reserved. Must be 0. </summary>
        public byte Reserved { get; }
        
        /// <summary> In icon format: Specifies color planes. Should be 0 or 1. </summary>
        public short Planes { get; }
        
        /// <summary> In icon format: Specifies bits per pixel. </summary>
        public short BitCount { get; }
        
        /// <summary> Image data size in bytes. </summary>
        public int BytesInRes { get; }
        
        /// <summary> Specifies the offset of the DIB or PNG data from the beginning of the file. </summary>
        public int ImageOffset { get; }

        public IconDirEntry(int squareSize, int imageLength, ref int offset)
        {
            // unchecked is used to allow the to overflow back to 0
            var byteSize = unchecked((byte)squareSize);
            Width = byteSize;
            Height = byteSize;
            ColorCount = 0;
            Reserved = IcoReserved;
            Planes = 1;
            BitCount = 32;
            BytesInRes = imageLength;
            ImageOffset = offset;
            offset += imageLength;
        }

        public MemoryStream ToStream()
        {
            var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            writer.Write([Width, Height, ColorCount, Reserved]);
            writer.Write(Planes);
            writer.Write(BitCount);
            writer.Write(BytesInRes);
            writer.Write(ImageOffset);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public byte[] ToBytes() => ToStream().ToArray();
    }
    
    private struct BitmapStructure
    {
        // BITMAPINFOHEADER structure
        // https://learn.microsoft.com/en-us/windows/win32/api/wingdi/ns-wingdi-bitmapinfoheader
        
        /// <summary> Specifies the number of bytes required by the structure. </summary>
        /// <remarks> Is the sum of the size of all properties aside from image data. </remarks>
        public uint Size { get; }
        
        /// <summary> Specifies the width of the bitmap, in pixels. </summary>
        public int  Width { get; }
        
        /// <summary>
        /// Specifies the height of the bitmap, in pixels.
        /// <list type="bullet">
        /// <item>For uncompressed RGB bitmaps, if biHeight is positive, the bitmap is a bottom-up DIB with the origin
        /// at the lower left corner. If biHeight is negative, the bitmap is a top-down DIB with the origin at the upper
        /// left corner.</item>
        /// <item>For YUV bitmaps, the bitmap is always top-down, regardless of the sign of biHeight. Decoders should
        /// offer YUV formats with positive biHeight, but for backward compatibility they should accept YUV formats with
        /// either positive or negative biHeight.</item>
        /// <item>For compressed formats, biHeight must be positive, regardless of image orientation.</item>
        /// </list>
        /// </summary>
        public int Height { get; }
        
        /// <summary> Specifies the number of planes for the target device. This value must be set to 1. </summary>
        public ushort Planes { get; }
        
        /// <summary> Specifies the number of bits per pixel (bpp). </summary>
        /// <remarks> For uncompressed formats, this value is the average number of bits per pixel. For compressed
        /// formats, this value is the implied bit depth of the uncompressed image, after the image has been decoded. </remarks>
        public ushort BitCount { get; }
        
        /// <summary>
        /// For uncompressed RGB formats, the following values are possible:
        /// <list type="table">
        /// <item>
        /// <term>BI_RGB</term>
        /// <description>Uncompressed RGB.</description>
        /// </item>
        /// <item>
        /// <term>BI_BITFIELDS</term>
        /// <description>Uncompressed RGB with color masks. Valid for 16-bpp and 32-bpp bitmaps.</description>
        /// </item>
        /// </list>
        /// </summary>
        public uint Compression { get; }
        
        /// <summary> Specifies the size, in bytes, of the image. This can be set to 0 for uncompressed RGB bitmaps. </summary>
        public uint SizeImage { get; }
        
        /// <summary> Specifies the horizontal resolution, in pixels per meter, of the target device for the bitmap. </summary>
        public int XPelsPerMeter { get; }
        
        /// <summary> Specifies the vertical resolution, in pixels per meter, of the target device for the bitmap. </summary>
        public int YPelsPerMeter { get; }
        
        /// <summary> Specifies the number of color indices in the color table that are actually used by the bitmap. </summary>
        public uint ClrUsed { get; }
        
        /// <summary> Specifies the number of color indices that are considered important for displaying the bitmap. </summary>
        public uint ClrImportant { get; }
        
        /// <summary> Bitmap color data (XOR mask) in RBGA order. </summary>
        public byte[] RawColorData { get; }
        
        /// <summary> Bitmap transparency data (AND mask) for older ICO parsers. </summary>
        /// <remarks> This can be all zeroes and still be rendered, but not valid. Some older parsers still present on current Windows will still access this. </remarks>
        public byte[] RawTransparencyData { get; } 
        
        private const uint BI_RGB = 0;
        private const uint BI_BITFIELDS = 3;
        private const ushort IcoPlanesValue = 1;
        private const int MaskResolver = 31;
        private const int MaskAlingment = 4;

        public BitmapStructure(SKBitmap resizedBitmap)
        {
            const ushort bpp8 = 32;                 // <= Not working with less here
            int squareSize = resizedBitmap.Width;   // <= All bitmaps are forced to be squares (Width = Height)
            // int maskStride = ((squareSize + MaskResolver) / bpp8) * MaskAlingment;
            
            Size = (sizeof(int) * 9) + (sizeof(short) * 2); 
            Width = squareSize;
            Height = squareSize * 2;    // Part of the specification
            Planes = IcoPlanesValue;
            BitCount = bpp8;
            Compression = BI_RGB;
            
            (RawColorData, RawTransparencyData) = GenerateMasksData(resizedBitmap);
            SizeImage = (uint)(RawColorData.Length + RawTransparencyData.Length);
            
            // Not using these
            XPelsPerMeter = 0;
            YPelsPerMeter = 0;
            ClrUsed = 0;
            ClrImportant = 0;
        }

        private (byte[], byte[]) GenerateMasksData(SKBitmap bitmap)
        {
            int squareSize = bitmap.Width;
            using var xorStream = new MemoryStream();
            using var xorWriter = new BinaryWriter(xorStream);
            using var andStream = new MemoryStream();
            using var andWriter = new BinaryWriter(andStream);

            var byteMax = 7;
            var bitCounter = byteMax;
            byte wrkByte = 0;
            for (int y = squareSize - 1; y >= 0; y--)
            {
                for (int x = 0; x < squareSize; x++)
                {
                    SKColor color = bitmap.GetPixel(x, y); // unpremultiplied
                    xorWriter.Write(color.Blue);
                    xorWriter.Write(color.Green);
                    xorWriter.Write(color.Red);
                    xorWriter.Write(color.Alpha);

                    byte isAlpha = (color.Alpha < 128) ? (byte)1 : (byte)0;
                    wrkByte |= (byte)(isAlpha << bitCounter);
                    bitCounter--;
                    if (bitCounter < 0) {
                        bitCounter = byteMax;
                        andWriter.Write(wrkByte);
                    }
                }
            }
            
            xorWriter.Flush();
            andWriter.Flush();
            return (xorStream.ToArray(), andStream.ToArray());
        }
        
        public int SizeInBytes() => (int)Size + RawColorData.Length + RawTransparencyData.Length;

        public MemoryStream ToStream()
        {
            var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            writer.Write(Size);
            writer.Write(Width);
            writer.Write(Height);
            writer.Write(Planes);
            writer.Write(BitCount);
            writer.Write(Compression);
            writer.Write(SizeImage);
            writer.Write(XPelsPerMeter);
            writer.Write(YPelsPerMeter);
            writer.Write(ClrUsed);
            writer.Write(ClrImportant);
            writer.Write(RawColorData);
            writer.Write(RawTransparencyData);
            
            stream.Position = 0;
            return stream;
        }
        
        public byte[] ToBytes() => ToStream().ToArray();
    }
}