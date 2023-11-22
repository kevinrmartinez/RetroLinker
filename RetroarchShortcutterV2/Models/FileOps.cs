﻿using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using RetroarchShortcutterV2.Models.Icons;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RetroarchShortcutterV2.Models
{
    public class FileOps
    {
        //public const string SettingFile = "RS_settings.cfg";
        public const string SettingFileBin = "RS_settings.dat";
        public const string DefUserAssetsDir = "UserAssets";
        public const string CoresFile = "cores.txt";
        public const string tempIco = "temp.ico";
        public const string DEFicon1 = "avares://RetroarchShortcutterV2/Assets/Icons/retroarch.ico";
        public const string NoAplica = "avares://RetroarchShortcutterV2/Assets/Images/no-aplica.png";
        public const byte MAX_PATH = 255; // TODO: Aplicar en todas partes!
        public const string WinLinkExt = ".lnk";
        public const string LinLinkExt = ".desktop";
        public const string LinuxRABin = "retroarch";
        public const string DotDesktopRAIcon = "retroarch";


        public static List<string> ConfigDir { get; private set; }

        public static readonly List<string> WinConvertibleIconsExt =
            new() { "*.png", "*.jpg", "*.jpeg", "*.svg", "*.svgz" };

        public static readonly List<string> LinIconsExt = new() { "*.ico", "*.png", "*.xpm", "*.svg", "*.svgz" };
        public static readonly string UserDesktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        public static readonly string UserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        public static readonly string UserTemp = Path.Combine(Path.GetTempPath(), "RetroarchShortcutterV2");

        // Solucion a los directorios de diferentes OSs, gracias a Vilmir en stackoverflow.com
        public static readonly string WINPublicUser = "C:\\Users\\Public";
        public static readonly string WINPublicDesktop = Path.Combine(WINPublicUser, "Desktop");
        //public static readonly string SettingFileBin = Path.Combine(UserProfile, "RS_settings.dat");

        public static IStorageFolder? DesktopFolder { get; private set; }
        public static IStorageFolder? ROMPadreDir { get; private set; }
        private static Settings LoadedSettings;


        #region Settings

        public static bool ExistSettingsBinFile() => File.Exists(SettingFileBin);

        public static Settings LoadSettingsFO()
        {
            LoadedSettings = SettingsOps.LoadSettings();
            System.Diagnostics.Debug.WriteLine("Settings cargadas para FileOps.", "[Debg]");
            BuildConfigDir();
            return LoadedSettings;
        }

        public static Settings LoadCachedSettingsFO()
        {
            LoadedSettings = SettingsOps.GetCachedSettings();
            BuildConfigDir();
            return LoadedSettings;
        }

        public static void BuildConfigDir()
        {
            const string NormalRAconfig = "Default";
            ConfigDir = new List<string>() { NormalRAconfig };
            if (LoadedSettings.PrevConfig) ConfigDir.AddRange(SettingsOps.PrevConfigs);
        }

        #endregion


        #region Load

        public static async Task<string[]> LoadCores()
        {
            string file = Path.Combine(LoadedSettings.UserAssetsPath, CoresFile);
            if (File.Exists(file))
            {
                System.Diagnostics.Trace.WriteLine($"Empezando la lectura de {file}.", "[Info]");
                var cores = await File.ReadAllLinesAsync(file);
                System.Diagnostics.Trace.WriteLine($"Completada la lectura de {file}.", "[Info]");
                return cores;
            }
            else
            {
                System.Diagnostics.Trace.WriteLine($"El archivo {file} no fue encontrado!", "[Info]");
                return Array.Empty<string>();
            }
        }

        public static async Task<List<string>> LoadIcons(bool OS)
        {
            // TODO: Ser capaz de retornar un error en caso de darse
            IconProc.IconItemsList = new();
            try
            {
                List<string>? files = new(Directory.EnumerateFiles(LoadedSettings.UserAssetsPath));
                System.Diagnostics.Trace.WriteLine(
                    $"Comenzando la lectura de iconos en {LoadedSettings.UserAssetsPath}.", "[Info]");
                for (int i = 0; i < files.Count; i++)
                {
                    string ext = Path.GetExtension(files[i]);
                    string filename = Path.GetFileName(files[i]);
                    string filepath = Path.GetFullPath(Path.Combine(files[i]));
                    if (OS)
                    {
                        if (WinConvertibleIconsExt.Contains("*" + ext) || (ext is ".exe"))
                        {
                            IconProc.IconItemsList.Add(new IconsItems(filename, filepath, true));
                        }
                        else if (ext is ".ico")
                        {
                            IconProc.IconItemsList.Add(new IconsItems(filename, filepath, false));
                        }
                    }
                    else
                    {
                        if (LinIconsExt.Contains("*" + ext))
                        {
                            IconProc.IconItemsList.Add(new IconsItems(filename, filepath));
                        }
                    }
                }

                if (files.Count == 0)
                {
                    System.Diagnostics.Trace.WriteLine(
                        $"No se encontraron archivos en {LoadedSettings.UserAssetsPath}.", "[Info]");
                    return new List<string>();
                }
                else
                {
                    System.Diagnostics.Trace.WriteLine($"Se encontraron {IconProc.IconItemsList.Count} iconos.",
                        "[Info]");
                }

                files.Clear();
                int newindex = 1;
                foreach (var file in IconProc.IconItemsList)
                {
                    files.Add(file.FileName);
                    file.comboIconIndex = newindex;
                    newindex++;
                }

                return files;
            }

            catch (DirectoryNotFoundException)
            {
                System.Diagnostics.Trace.WriteLine(
                    $"No se encontro el directorio '{Path.GetFullPath(LoadedSettings.UserAssetsPath)}'.", "[Warn]");
                return new List<string>();
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine($"Ha ocurrido un error en la carga de iconos.", "[Erro]");
                System.Diagnostics.Debug.WriteLine(
                    $"En FileOps, el elemento {e.Source} a retornado el error '{e.Message}'", "[Erro]");
                return new List<string>();
            }
        }

        public static async Task SetDesktopDir(TopLevel topLevel)
        {
            DesktopFolder ??= await topLevel.StorageProvider.TryGetFolderFromPathAsync(UserDesktop);
            // '??=' le asigna valor solo si esta null
        }

        #endregion

        #region FUNCTIONS

        public static string GetDirFromPath(string path) => Path.GetDirectoryName(path);

        public static bool CheckUsrSetDir(string path)
        {
            // TODO: Ser capaz de retornar un error en caso de darse
            try
            {
                Directory.CreateDirectory(path);
                return true;
            }
            catch
            {
                Console.WriteLine("No se puede crear la carpeta " + path);
                return false;
            }
        }

        public static Uri GetDefaultIcon() => new Uri(DEFicon1);

        public static Uri GetNAimage() => new Uri(NoAplica);

        public static Bitmap GetBitmap(string path) => new Bitmap(path);

        public static Bitmap GetBitmap(Stream img_stream) => new Bitmap(img_stream);

        public static async void SetROMPadre(string? dir_ROMpadre, TopLevel topLevel)
        {
            if (!string.IsNullOrWhiteSpace(dir_ROMpadre))
            {
                ROMPadreDir = await topLevel.StorageProvider.TryGetFolderFromPathAsync(dir_ROMpadre);
            }
        }

        public static string GetDeskLinkPath(string link_name, bool OS)
        {
            string new_dir = Path.GetFileNameWithoutExtension(link_name);
            new_dir = (OS) ? new_dir : new_dir.Replace(" ", "-");
            new_dir += (OS) ? WinLinkExt : LinLinkExt;
            new_dir = Path.Combine(UserDesktop, new_dir);
            return new_dir;
        }

        public static string GetDefinedLinkPath(string link_name, string LinkPath, bool OS)
        {
            string new_dir = Path.GetFileNameWithoutExtension(link_name);
            new_dir = (OS) ? new_dir : new_dir.Replace(" ", "-");
            new_dir += (OS) ? WinLinkExt : LinLinkExt;
            new_dir = Path.Combine(LinkPath, new_dir);
            return new_dir;
        }

        #endregion


        #region FileDialogs

        public static async Task<string> OpenFileAsync(PickerOpt.OpenOpts template, TopLevel topLevel)
        {
            var opt = PickerOpt.OpenPickerOpt(template);
            var file = await topLevel.StorageProvider.OpenFilePickerAsync(opt);
            string dir = file.Count > 0 ? Path.GetFullPath(file[0].Path.LocalPath) : string.Empty;
            return dir;
        }

        public static async Task<string> OpenFolderAsync(byte template, TopLevel topLevel)
        {
            FolderPickerOpenOptions opt = new()
            {
                AllowMultiple = false,
                Title = template switch
                {
                    0 => "Eliga el directorio de UserAssets",
                    1 => "Eliga el directorio padre de ROMs",
                    2 => "Eliga el directorio donde guardar los .ico",
                    // Esta opcion no deberia pasar
                    _ => "Eliga el directorio..."
                }
            };

            var dirList = await topLevel.StorageProvider.OpenFolderPickerAsync(opt);
            string dir = dirList.Count > 0 ? Path.GetFullPath(dirList[0].Path.LocalPath) : string.Empty;
            return dir;
        }

        public static async Task<string> SaveFileAsync(PickerOpt.SaveOpts template, TopLevel topLevel)
        {
            var opt = PickerOpt.SavePickerOpt(template);
            var file_task = topLevel.StorageProvider.SaveFilePickerAsync(opt);
            var file = await file_task;
            string dir = (file != null) ? file.Path.LocalPath : string.Empty;
            return dir;
        }

        #endregion

        #region ICONS

        public static string CpyIconToUsrSet(string ogPath)
        {
            string name = Path.GetFileName(ogPath);
            string newPath = Path.Combine(LoadedSettings.IcoSavPath, name);
            CheckUsrSetDir(LoadedSettings.IcoSavPath);
            if (File.Exists(newPath))
            {
                return Path.GetFullPath(newPath);
            }

            File.Copy(ogPath, newPath);
            return Path.GetFullPath(newPath);
        }

        public static string CpyIconToCustomSet(string ogPath, string destPath)
        {
            destPath = Path.GetDirectoryName(destPath);
            string name = Path.GetFileName(ogPath);
            string newPath = Path.Combine(destPath, name);
            if (File.Exists(newPath))
            {
                return Path.GetFullPath(newPath);
            }

            File.Copy(ogPath, newPath);
            return Path.GetFullPath(newPath);
        }

        public static string CpyIconToUsrAss(string og_path)
        {
            string name = Path.GetFileName(og_path);
            string new_path = Path.Combine(LoadedSettings.UserAssetsPath, name);
            if (File.Exists(new_path))
            {
                return Path.GetFullPath(new_path);
            }
            else
            {
                File.Copy(og_path, new_path);
                return Path.GetFullPath(new_path);
            }
        }

        public static bool IsVectorImage(string svg_file) => (Path.GetExtension(svg_file) is ".svg" or "svgz");

        #endregion


        #region Windows Only Ops

        public static bool IsWinEXE(string exe_file) => (Path.GetExtension(exe_file) == ".exe");

        public static IconsItems GetEXEWinIco(string icondir, int index)
        {
            var iconstream = IconProc.IcoExtraction(icondir);
            var objicon = new IconsItems(null, icondir, iconstream, index, true);
            IconProc.IconItemsList.Add(objicon);
            return objicon;
        }

        //public static MemoryStream GetExeIcoStream(string file_path) => IconProc.IcoExtraction(file_path);

        public static string SaveWinIco(IconsItems selectedIconItem)
        {
            string icoExt = Path.GetExtension(selectedIconItem.FileName);
            string icoName = Path.GetFileNameWithoutExtension(selectedIconItem.FileName) + ".ico";
            string new_dir = Path.Combine(UserTemp, icoName);
            CheckUsrSetDir(UserTemp);
            ImageMagick.MagickImage icon_image = new();
            if (selectedIconItem.IconStream != null)
            {
                selectedIconItem.IconStream.Position = 0;
            }

            switch (icoExt)
            {
                case ".svg" or ".svgz":
                    icon_image = IconProc.ImageConvert(selectedIconItem.IconStream);
                    icon_image.Write(new_dir);
                    //new_dir = CpyIconToUsrSet(new_dir);
                    break;
                case ".exe":
                    if (LoadedSettings.ExtractIco)
                    {
                        icon_image = IconProc.SaveIcoToMagick(selectedIconItem.IconStream);
                        icon_image.Write(new_dir);
                        //new_dir = CpyIconToUsrSet(new_dir);
                    }
                    else
                    {
                        new_dir = selectedIconItem.FilePath;
                    }

                    break;

                default:
                    icon_image = IconProc.ImageConvert(selectedIconItem.FilePath);
                    icon_image.Write(new_dir);
                    //new_dir = CpyIconToUsrSet(new_dir);
                    break;
            }

            //settings.Dispose();
            return new_dir;
        }

        public static string ChangeIcoNameToLinkName(Shortcutter linkObj)
        {
            string iconPath = GetDirFromPath(linkObj.ICONfile);
            string linkName = Path.GetFileNameWithoutExtension(linkObj.LNKdir) + ".ico";
            string newIconPath = Path.Combine(iconPath, linkName);
            
            File.Copy(linkObj.ICONfile, newIconPath, true);
            File.Delete(linkObj.ICONfile);
            
            return newIconPath;
        }

        #endregion

        #region Linux Only

        public static string[] FixLinkName(string LinkDir)
        {
            string[] NameFix =
            {
                // File Path
                Path.GetFileName(LinkDir),
                // .desktop Name field
                Path.GetFileNameWithoutExtension(LinkDir),
                // File Name
                //LinkDir
            };
            NameFix[0] = NameFix[0].Replace(" ", "-");
            //NameFix[2] = NameFix[0];
            NameFix[0] = Path.Combine(Path.GetDirectoryName(LinkDir), NameFix[0]);
            return NameFix;
        }
        
        public static string GetRAIcons()
        {
            // TODO: Decidir si se utilizara este metodo
            // TODO: Escanear los .desktop de RA existentes
            const string RA = "retroarch";
            const string RAsvg = "retroarch.svg";
            const string RApng = "retroarch.png";
            string[] CommonIconPaths = new[]
            {
                "/usr/share/pixmaps/",
                "/usr/share/app-install/icons/",
                $"{Path.Combine(UserProfile, ".local", "share", "icons") + "\\"}",
                "/usr/share/retroarch/"
                //,"/snap/retroarch/"
            };
            string icon_dir = string.Empty;
            for (int i = 0; i < CommonIconPaths.Length; i++)
            {
                
                if (File.Exists(CommonIconPaths[i] + RAsvg))
                { icon_dir = CommonIconPaths[i] + RAsvg; break; }
                if (File.Exists(CommonIconPaths[i] + RApng))
                { icon_dir = CommonIconPaths[i] + RAsvg; break; }
            }
            return (string.IsNullOrEmpty(icon_dir)) ? RA : icon_dir;
        }

        #endregion


#if DEBUG
        public static string IconExtractTest() => "F:\\Zero Fox\\Anime Icon Matcher.exe";
#endif
    }
}
