using ImageMagick;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroarchShortcutterV2.Models
{
    public class IconProc
    {
        public static string writeIcoDIR;

        public static MemoryStream PngConvert(string DIR)
        {
            var iconStream = new MemoryStream();
            var PNG = new MagickImage(DIR);
            PNG.Format = MagickFormat.Ico;
            if (PNG.Height > 256 || PNG.Width > 256)
            { PNG.AdaptiveResize(256, 256); }
            else
            {
                int sizeMajor = int.Max(PNG.Width, PNG.Height);
                int i;
                for (i = 256; i > sizeMajor; i /= 2) { }
                PNG.AdaptiveResize(i, i);
            }
            PNG.Write(iconStream);
            return iconStream;
        }
    }
}
