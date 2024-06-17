using Avalonia.Controls;
using Avalonia.Media.Imaging;

namespace RetroLinker.Models
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
            lnk.OutputPaths.Add(new ShortcutterOutput("2048.desktop", lnk.ROMcore));
            lnk.Desc = "Juega 2048 en Retroarch";
            lnk.VerboseB = true;
            if (OS) { OS = false; }
            Shortcutter.BuildShortcut(lnk, OS);

            lnk.Command = $"-L mgba \"{System.IO.Path.Combine(FileOps.UserProfile, "Fire Emblem - The Binding Blade.gba")}\"";
            lnk.OutputPaths.Add(new ShortcutterOutput(System.IO.Path.Combine(FileOps.UserProfile,"Fire Emblem 6.desktop"), lnk.ROMcore));
            
            LinFunc.LinShortcutter.CreateShortcut(lnk, lnk.OutputPaths[0]);
        }
    }
}
