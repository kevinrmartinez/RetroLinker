using Avalonia.Controls;
using Avalonia.Media.Imaging;
using RetroarchShortcutterV2.Models.WinIco;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RetroarchShortcutterV2.Models
{
    public class FileOps
    {
        public const string UserAssetsDir = "UserAssets";
        public const string CoresFile = "cores.txt";
        public const string tempIco = "temp.ico";
        public const string DEFicon1 = "retroarch.ico";
        public const string DEFicon2 = "icon_light.ico";
        public const string DEFicon3 = "icon_dark.ico";
        public static List<string> IconsDir = new List<string> { "Default", "Default Alt Light", "Default Alt Dark" };
        public static List<string> ConfigDir = new List<string> { "Default" };
        public static string UserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        public static string UserSettings = Path.Combine(UserProfile, ".RetroarchShortcutterV2");
        public static string writeIcoDIR;


        public static string[] LoadCores()
        {
            string file = Path.Combine(UserAssetsDir, CoresFile);
            if (File.Exists(file)) { var cores = File.ReadAllLines(file); return cores; }
            else { return new string[] { }; }
        }

        public static bool CheckUsrSetDir()
        {
            try { Directory.CreateDirectory(UserSettings); return true; }
            catch { Console.WriteLine("No se puede crear la carpeta " + UserSettings); return false; }
        }

        public static string picFillWithDefault(int index) 
        {
            string icon_file = string.Empty;
            switch (index)
            {
                case 0:
                    icon_file = Path.Combine(UserAssetsDir, DEFicon1);
                    break;
                case 1:
                    icon_file = Path.Combine(UserAssetsDir, DEFicon2);
                    break;
                case 2:
                    icon_file = Path.Combine(UserAssetsDir, DEFicon3);
                    break;
            }
            return icon_file;
        }

        public static bool IsWinEXE(string exe)
        {
            return (Path.GetExtension(exe) == ".exe");
        }

        public static void WorkWinEXE(string exe, int index)
        {

        }

        public static WinIcoStream GetEXEWinIco(string icondir, int index)
        {
            var iconstream = IconProc.IcoExtraction(icondir);
            var objicon = new WinIcoStream(icondir, iconstream, index);
            IconProc.IcoStreams.Add(objicon);
            return objicon;
        }

        public static string SaveWinIco(string icondir, MemoryStream icoStream)
        {
            string icoExt = Path.GetExtension(icondir);
            string icoName = Path.GetFileNameWithoutExtension(icondir) + ".ico";
            string newfile = Path.Combine(UserSettings, icoName);
            string altfile = Path.Combine(UserProfile, icoName);
            if (icoStream != null) { icoStream.Position = 0; }
            switch (icoExt)
            {
                case ".png":
                    var icon_stream = IconProc.PngConvert(icondir);
                    icondir = IconProc.SaveIcoToFile(newfile, icon_stream, altfile);
                    break;
                case ".exe":
                    icondir = IconProc.SaveIcoToFile(newfile, icoStream, altfile);
                    break;
                default:
                    break;
            }
            return icondir;
        }

        public static async Task<string> OpenFileAsync(int template, TopLevel topLevel)
        {
            var opt = PickerOpt.OpenPickerOpt(template);
            var file = await topLevel.StorageProvider.OpenFilePickerAsync(opt);
            string dir;
            if (file.Count > 0) { dir = Path.GetFullPath(file[0].Path.LocalPath); }
            else { return null; }
            return dir;
        }

        public static async Task<string> SaveFileAsync(int template, TopLevel topLevel)
        {
            var opt = PickerOpt.SavePickerOpt(template);
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(opt);
            string dir;
            if (file != null) { dir = file.Path.LocalPath; }
            else { return null; }
            return dir;
        }

        public static string CpyIconToUsrSet(string path)
        {
            string name = Path.GetFileName(path);
            string newpath = Path.Combine(UserSettings, name);
            CheckUsrSetDir();
            if (File.Exists(newpath)) { return newpath; }
            else 
            {
                File.Copy(path, newpath);
                return newpath;
            }
        }

        public static Bitmap GetBitmap(string path)
        {
            Bitmap bitmap = new Bitmap(path);
            return bitmap;
        }

        public static Bitmap GetBitmap(Stream stream)
        {
            Bitmap bitmap = new Bitmap(stream);
            return bitmap;
        }



#if DEBUG
        public static string IconExtractTest()
        {
            string file = "F:\\Zero Fox\\Anime Icon Matcher.exe";
            string name = Path.GetFileNameWithoutExtension(file) + ".ico";
            var icoStream = IconProc.IcoExtraction(file);
            string newfile = Path.Combine(UserSettings, name);
            string altfile = Path.Combine(UserProfile, name);
            newfile = IconProc.SaveIcoToFile(newfile, icoStream, altfile);
            return newfile;
        }
#endif
    }
}
