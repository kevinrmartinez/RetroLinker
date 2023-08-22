using Avalonia.Controls;
using Avalonia.Media.Imaging;
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
        public static string UserSettings = Path.Combine(Environment.SpecialFolder.UserProfile.ToString() + ".RetroarchShortcutterV2");
        public static string writeIcoDIR;

        public static string[] LoadCores()
        {
            string file = Path.Combine(FileOps.UserAssetsDir, FileOps.CoresFile);
            if (File.Exists(file)) { var cores = File.ReadAllLines(file); return cores; }
            else { return new string[] { }; }
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
    }
}
