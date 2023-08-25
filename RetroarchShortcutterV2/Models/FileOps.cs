using Avalonia.Controls;
using Avalonia.Media.Imaging;
using RetroarchShortcutterV2.Models.Icons;
using RetroarchShortcutterV2.Views;
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
        public const string DEFicon1 = "avares://RetroarchShortcutterV2/Assets/Icons/retroarch.ico";
        public static List<string> ConfigDir = new List<string> { "Default" };
        public static string UserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        public static string UserSettings = Path.Combine(UserProfile, ".RetroarchShortcutterV2");           // Solucion a los directorios de diferentes OSs, gracias a Vilmir en stackoverflow.com
        public static string writeIcoDIR;


        public static string[] LoadCores()
        {
            string file = Path.Combine(UserAssetsDir, CoresFile);
            if (File.Exists(file)) { var cores = File.ReadAllLines(file); return cores; }
            else { return Array.Empty<string>(); }
        }

        public static List<string> LoadIcons(bool OS)
        {
            IconProc.IconItemsList = new();
            List<string>? files = new(Directory.EnumerateFiles(UserAssetsDir));
            if (files != null)
            {
                for (int i = 0; i < files.Count; i++)
                {
                    string ext = Path.GetExtension(files[i]);
                    string filename = Path.GetFileName(files[i]);
                    string filepath = Path.GetFullPath(Path.Combine(files[i]));
                    if (OS)
                    {
                        if (ext is ".ico" or ".png" or ".exe") 
                        { IconProc.IconItemsList.Add(new IconsItems(filename, filepath)); }
                    }
                    else
                    {
                        if (ext is ".ico" or ".png" or ".xpm" or ".svg" or ".svgz")
                        { IconProc.IconItemsList.Add(new IconsItems(filename, filepath)); }
                    }
                }
                files.Clear();
                int newindex = 1;
                foreach (var file in IconProc.IconItemsList)
                { files.Add(file.FileName); file.comboIconIndex = newindex; newindex++; }
                return files;

            }
            else { return new List<string>(); } 
        }


        public static bool CheckUsrSetDir()
        {
            try { Directory.CreateDirectory(UserSettings); return true; }
            catch { Console.WriteLine("No se puede crear la carpeta " + UserSettings); return false; }
            // PENDIENTE: mostrar msbox indicando problema
        }

        public static Uri GetDefaultIcon() 
        {
            Uri DEFicon = new(DEFicon1);
            return DEFicon; 
        }

        public static bool IsWinEXE(string exe)
        {
            return (Path.GetExtension(exe) == ".exe");
        }

        public static IconsItems GetEXEWinIco(string icondir, int index)
        {
            var iconstream = IconProc.IcoExtraction(icondir);
            var objicon = new IconsItems(null, icondir, iconstream, index);
            IconProc.IconItemsList.Add(objicon);
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
            Bitmap bitmap = new(path);
            return bitmap;
        }

        public static Bitmap GetBitmap(Stream stream)
        {
            Bitmap bitmap = new(stream);
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
