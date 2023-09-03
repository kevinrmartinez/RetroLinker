using ImageMagick;
using ImageMagick.Formats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppForTesting
{
    public class IconConvert
    {
        public static void ConvertIcon1()
        {
            var settings = new MagickReadSettings();
            // Tells the xc: reader the image to create should be 256x256
            settings.Width = 256;
            settings.Height = 256;

            // Create image that is completely purple and 256x256
            var purple = new MagickImage("xc:transparent", settings);

            // Sets the output format to png
            purple.Format = MagickFormat.Ico;
            purple.Write("purple.ico");


            MagickImage image = new("input.png");
            var size = new MagickGeometry(256, 256)
            {
                Greater = true,
                IgnoreAspectRatio = false,
            };
            MagickDefine resize = new("resize", "256x256^");
            MagickDefine gravity = new("-gravity", "center");
            MagickDefine crop = new("-crop", "256+256+0+0");
            //MagickDefine repage = new("+repage", "");
            //IDefines defines = (IDefines)new List<MagickDefine>() { resize, gravity, crop};

            //fr.Settings.Depth;
            image.Format = MagickFormat.Ico;
            image.InterpolativeResize(size, PixelInterpolateMethod.Blend);
            
            image.Crop(size);
            image.RePage();

            image.Write("mid.ico");


            //using var images = new MagickImageCollection
            //{
            //    image, purple
            //};
            

            //using var result = images.Mosaic();
            //result.Format = MagickFormat.Ico;
            //result.Write("output.ico");
        }
    }
}
