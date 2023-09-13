using Avalonia.Controls;
using Avalonia.Media.Imaging;

namespace RetroarchShortcutterV2.Models
{
    public class Testing
    {

        
        static string imageDIR = "C:\\Users\\force\\Pictures\\zero.ico";
        //static Stream stream = File.OpenRead(imageDIR);
        public static Bitmap bitmap = new(imageDIR);

        public static async void FilePickerTesting(TopLevel? topLevel)
        {
            var dg = await topLevel.StorageProvider.OpenFilePickerAsync(PickerOpt.OpenPickerOpt(PickerOpt.OpenOpts.RAroms));
            var dg2 = await topLevel.StorageProvider.OpenFilePickerAsync(PickerOpt.OpenPickerOpt(PickerOpt.OpenOpts.WINico));
        }

        public static void LinShortcutTest(bool OS)
        {
            Shortcutter lnk = new();
            lnk.RAdir = "retroarch";
            lnk.ROMdir = Commander.contentless;
            lnk.ROMcore = "2048";
            lnk.LNKdir = "2048.desktop";
            lnk.Desc = "Juega 2048 en Retroarch";
            lnk.VerboseB = true;
            if (OS) { OS = false; }
            Shortcutter.BuildLinShorcut(lnk, OS);

            lnk.Command = "-L mgba \"Fire Emblem - The Binding Blade.gba\"";
            lnk.LNKdir = "new2048.desktop";
            LinFunc.LinShortcutter.CreateShortcutIni(lnk);
        }
    }
}
