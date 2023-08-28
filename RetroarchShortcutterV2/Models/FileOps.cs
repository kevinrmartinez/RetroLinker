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

        public static List<string> ConfigDir { get; private set; } = new() { "Default" };
        public static string UserProfile { get; private set; } = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        public static string UserSettings { get; private set; } = Path.Combine(UserProfile, ".RetroarchShortcutterV2");           // Solucion a los directorios de diferentes OSs, gracias a Vilmir en stackoverflow.com
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

        public static async void SetROMPadre(string dir_ROMpadre, TopLevel topLevel)
        {
            if (dir_ROMpadre != null)
            { ROMPadreDir = await topLevel.StorageProvider.TryGetFolderFromPathAsync(dir_ROMpadre); }
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

        public static string GetAssetDir(string item, string file_name, string file_path)
        {
            return "";
        }

        public static string SaveWinIco(string icondir, MemoryStream icoStream)
        {
            string icoExt = Path.GetExtension(icondir);
            string icoName = Path.GetFileNameWithoutExtension(icondir) + ".ico";
            string newfile = Path.Combine(UserSettings, icoName);
            string altfile = Path.Combine(UserProfile, icoName);
            ImageMagick.MagickImage icon_stream = new();
            if (icoStream != null) { icoStream.Position = 0; }
            switch (icoExt)
            {
                case ".png":
                    icon_stream = IconProc.ImageConvert(icondir);
                    icondir = IconProc.SaveConvIcoToFile(newfile, icon_stream, altfile);
                    break;
                case ".jpg":
                    icon_stream = IconProc.ImageConvert(icondir);
                    icondir = IconProc.SaveConvIcoToFile(newfile, icon_stream, altfile);
                    break;
                case ".jpeg":
                    icon_stream = IconProc.ImageConvert(icondir);
                    icondir = IconProc.SaveConvIcoToFile(newfile, icon_stream, altfile);
                    break;
                case ".exe":
                    icondir = IconProc.SaveIcoToFile(newfile, icoStream, altfile);
                    break;
                default:
                    break;
            }
            return icondir;
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



        // Origen:
        ///<summary>
        /// Steve Lydford - 12/05/2008.
        ///
        /// Encrypts a file using Rijndael algorithm.
        ///</summary>
        private void EncryptFile(string inputFile, string outputFile)
        {

            try
            {
                string password = @"myKey123"; // Your Key Here
                UnicodeEncoding UE = new UnicodeEncoding();
                byte[] key = UE.GetBytes(password);

                string cryptFile = outputFile;
                FileStream fsCrypt = new FileStream(cryptFile, FileMode.Create);

                Aes AESCrypto = new AesCng();
                CryptoStream cs = new(fsCrypt,
                                      AESCrypto.CreateEncryptor(key, key),
                                      CryptoStreamMode.Write);

                FileStream fsIn = new(inputFile, FileMode.Open);

                int data;
                while ((data = fsIn.ReadByte()) != -1)
                { cs.WriteByte((byte)data); }


                fsIn.Close();
                cs.Close();
                fsCrypt.Close();
            }
            catch
            {
                _ = "Encryption failed!";
            }
        }
        ///<summary>
        /// Steve Lydford - 12/05/2008.
        ///
        /// Decrypts a file using Rijndael algorithm.
        ///</summary>
        private void DecryptFile(string inputFile, string outputFile)
        {
            string password = @"myKey123"; // Your Key Here

            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] key = UE.GetBytes(password);

            FileStream fsCrypt = new FileStream(inputFile, FileMode.Open);
            Aes AESCrypto = new AesCng();

            CryptoStream cs = new CryptoStream(fsCrypt,
                                               AESCrypto.CreateDecryptor(key, key),
                                               CryptoStreamMode.Read);

            FileStream fsOut = new FileStream(outputFile, FileMode.Create);

            int data;
            while ((data = cs.ReadByte()) != -1)
            { fsOut.WriteByte((byte)data); }

            fsOut.Close();
            cs.Close();
            fsCrypt.Close();
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
