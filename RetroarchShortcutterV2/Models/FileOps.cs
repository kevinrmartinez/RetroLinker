using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using RetroarchShortcutterV2.Models.Icons;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RetroarchShortcutterV2.Models
{
    public class FileOps
    {
        public const string SettingFile = "RS_settings.cfg";
        public const string SettingFileBin = "RS_settings.bin";
        public const string UserAssetsDir = "UserAssets";
        public const string CoresFile = "cores.txt";
        public const string tempIco = "temp.ico";
        public const string DEFicon1 = "avares://RetroarchShortcutterV2/Assets/Icons/retroarch.ico";
        public const short MAX_PATH = 255;  // Aplicar en todas partes!

        public static List<string> ConfigDir { get; private set; } = new() { "Default" };
        public static string UserDesktop { get; private set; } = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        public static string UserProfile { get; private set; } = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        public static string UserTemp { get; private set; } = Path.Combine(Path.GetTempPath(), "RetroarchShortcutterV2");
        public static string UserSettings { get; private set; } = Path.Combine(UserProfile, ".RetroarchShortcutterV2");           // Solucion a los directorios de diferentes OSs, gracias a Vilmir en stackoverflow.com
        public static IStorageFolder? DesktopFolder { get; private set; }
        public static IStorageFolder? ROMPadreDir { get; private set; }
        public static string WriteIcoDIR { get; private set; } = string.Empty;


        public static bool ExistSettingsFile() => (File.Exists(SettingFile));

        public static bool ExistSettingsBinFile() => (File.Exists(SettingFileBin));

        public static bool ChkSettingsFile() => (File.ReadAllText(SettingFile) != null);

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
                        if (ext is ".ico" or ".png" or ".xpm" or ".svg" or ".svgz")         // Las imagenes de Avalonia no leen .svg!!
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

        public static bool CheckUsrSetDir(string dir)
        {
            try { Directory.CreateDirectory(dir); return true; }
            catch { Console.WriteLine("No se puede crear la carpeta " + dir); return false; }
            // PENDIENTE: mostrar msbox indicando problema
        }


        public static Uri GetDefaultIcon() 
        {
            Uri DEFicon = new(DEFicon1);
            return DEFicon; 
        }

        public static async void SetROMPadre(string dir_ROMpadre, TopLevel topLevel)
        {
            if (dir_ROMpadre != null)
            { ROMPadreDir = await topLevel.StorageProvider.TryGetFolderFromPathAsync(dir_ROMpadre); }
        }

        #region FileDialogs
        public static async Task<string> OpenFileAsync(int template, TopLevel topLevel)
        {
            var opt = PickerOpt.OpenPickerOpt(template);
            var file = await topLevel.StorageProvider.OpenFilePickerAsync(opt);
            string dir;
            if (file.Count > 0) { dir = Path.GetFullPath(file[0].Path.LocalPath); }
            else { return null; }
            return dir;
        }

        public static async Task<string> OpenFolderAsync(byte template ,TopLevel topLevel)
        {
            FolderPickerOpenOptions opt = new();
            switch(template)
            {
                case 0:
                    opt.Title = "Eliga el directorio padre de ROMs"; 
                    opt.AllowMultiple = false;
                    break;
                case 1:
                    opt.Title = "Eliga el directorio padre de ROMs";
                    opt.AllowMultiple = false;
                    break;
            }    
            var dirList = await topLevel.StorageProvider.OpenFolderPickerAsync(opt);
            string dir;
            if (dirList.Count > 0) { dir = Path.GetFullPath(dirList[0].Path.LocalPath); }
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
        #endregion

        public static string GetAllDeskPath(string link_name, bool OS)
        {
            string new_dir = Path.GetFileNameWithoutExtension(link_name);
            string lnk_ext = (OS) ? ".lnk" : "dsktop";
            new_dir += lnk_ext;
            new_dir = Path.Combine(UserDesktop, new_dir);
            return new_dir;
        }
        
        public static string CpyIconToUsrSet(string og_path)
        {
            string name = Path.GetFileName(og_path);
            string new_path = Path.Combine(WriteIcoDIR, name);  // WriteIcoDIR se llena en SaveWinIco!
            CheckUsrSetDir(WriteIcoDIR);
            if (File.Exists(new_path)) { return Path.GetFullPath(new_path); }
            else 
            {
                File.Copy(og_path, new_path);
                return Path.GetFullPath(new_path);
            }
        }
        
        public static string CpyIconToUsrAss(string og_path)
        {
            string name = Path.GetFileName(og_path);
            string new_path = Path.Combine(UserAssetsDir, name);
            if (File.Exists(new_path)) { return Path.GetFullPath(new_path); }
            else 
            {
                File.Copy(og_path, new_path);
                return Path.GetFullPath(new_path);
            }
        }

        #region Windows Only Ops
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

        public static string SaveWinIco(string icondir, MemoryStream? icoStream)
        {
            WriteIcoDIR = Settings.ConvICONPath;
            string icoExt = Path.GetExtension(icondir);
            string icoName = Path.GetFileNameWithoutExtension(icondir) + ".ico";
            string new_dir = Path.Combine(UserTemp, icoName);
            CheckUsrSetDir(UserTemp);
            ImageMagick.MagickImage icon_image = new();
            if (icoStream != null) { icoStream.Position = 0; }
            switch (icoExt)
            {
                case ".png":
                    icon_image = IconProc.ImageConvert(icondir);
                    icon_image.Write(new_dir);
                    new_dir = CpyIconToUsrSet(new_dir);
                    break;
                case ".jpg":
                    icon_image = IconProc.ImageConvert(icondir);
                    icon_image.Write(new_dir);
                    new_dir = CpyIconToUsrSet(new_dir);
                    break;
                case ".jpeg":
                    icon_image = IconProc.ImageConvert(icondir);
                    icon_image.Write(new_dir);
                    new_dir = CpyIconToUsrSet(new_dir);
                    break;
                case ".exe":
                    if (Settings.ExtractIco) 
                    { 
                        icon_image = IconProc.SaveIcoToMI(icoStream); 
                        icon_image.Write(new_dir);
                        new_dir = CpyIconToUsrSet(new_dir);
                    }
                    break;
                default:
                    break;
            }
            return new_dir;
        }
        #endregion

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
            return file;
        }
#endif
    }
}
