/*
    A .NET GUI application to help create desktop links of games running on RetroArch.
    Copyright (C) 2023  Kevin Rafael Martinez Johnston

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using RetroLinker.Models.Icons;

namespace RetroLinker.Models
{
    public static class FileOps
    {
        public const string SettingFile = "RLsettings.cfg";
        public const string SettingFileBin = "RLsettings.dat";
        public const string DefUserAssetsDir = "UserAssets";
        public const string CoresFile = "cores.txt";
        public const string tempIco = "temp.ico";
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
        
        private static Settings LoadedSettings;


        #region Settings

        public static bool ExistSettingsBinFile() => File.Exists(SettingFileBin);

        public static Settings LoadSettingsFO()
        {
            LoadedSettings = SettingsOps.LoadSettings();
            System.Diagnostics.Debug.WriteLine("Settings loaded for FileOps.", App.DebgTrace);
            BuildConfigDir();
            return LoadedSettings;
        }

        public static Settings LoadCachedSettingsFO()
        {
            LoadedSettings = SettingsOps.GetCachedSettings();
            BuildConfigDir();
            return LoadedSettings;
        }

        public static string[] ReadSettingsFile() => File.ReadAllLines(SettingFileBin);
        
        public static async void WriteSettingsFile(string settingString)
        {
            try
            {
                await File.WriteAllTextAsync(SettingFileBin, settingString);
                System.Diagnostics.Trace.WriteLine($"{SettingFileBin} written successfully", App.InfoTrace);
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine($"{SettingFileBin} could not be written!", App.ErroTrace);
                System.Diagnostics.Trace.WriteLine(e);
            }
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
            // TODO: Create a back up cores.txt that compiles with the app
            string file = Path.Combine(LoadedSettings.UserAssetsPath, CoresFile);
            if (File.Exists(file))
            {
                System.Diagnostics.Trace.WriteLine($"Starting reading of {file}.", App.InfoTrace);
                var cores = await File.ReadAllLinesAsync(file);
                System.Diagnostics.Trace.WriteLine($"Completed reading of {file}.", App.InfoTrace);
                return cores;
            }
            else
            {
                System.Diagnostics.Trace.WriteLine($"The file {file} could not be found!", App.InfoTrace);
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
                    $"Starting Icons reading at {LoadedSettings.UserAssetsPath}.", App.InfoTrace);
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
                        $"No icons found at {LoadedSettings.UserAssetsPath}.", App.InfoTrace);
                    return new List<string>();
                }
                else
                { System.Diagnostics.Trace.WriteLine($"{IconProc.IconItemsList.Count} icons were found.", App.InfoTrace); }

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
                // Possibly redundant
                System.Diagnostics.Trace.WriteLine(
                    $"The directory '{Path.GetFullPath(LoadedSettings.UserAssetsPath)}' could not be found.", App.WarnTrace);
                return new List<string>();
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine($"An error has ocurred while loading icons.", App.ErroTrace);
                System.Diagnostics.Debug.WriteLine(
                    $"In FileOps, he element {e.Source} has returned the error:\n{e.Message}", App.ErroTrace);
                return new List<string>();
            }
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

        #region Linux Only Ops

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
        public static string IconExtractTest() => "C:\\Windows\\system32\\notepad.exe";
#endif
    }
}
