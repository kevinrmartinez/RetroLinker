using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroarchShortcutterV2.Models
{
    public class Testing
    {

        
        static string imageDIR = "C:\\Users\\force\\Pictures\\zero.ico";
        //static Stream stream = File.OpenRead(imageDIR);
        public static Bitmap bitmap = new Bitmap(imageDIR);

        public static async void FilePickerTesting(TopLevel? topLevel)
        {
            var dg = await topLevel.StorageProvider.OpenFilePickerAsync(PickerOpt.OpenPickerOpt(1));
            var dg2 = await topLevel.StorageProvider.OpenFilePickerAsync(PickerOpt.OpenPickerOpt(3));
        }
    }
}
