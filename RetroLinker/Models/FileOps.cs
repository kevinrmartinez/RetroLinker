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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using RetroLinker.Models.Linux;

namespace RetroLinker.Models
{
    public static class FileOps
    {
        // public const string SettingFileBin = "RLsettings.dat";
        public const string SettingFileJson = "RLsettings.json";
        public const string DefUserAssets = "UserAssets";
        public const string tempFile = "temp.txt";
        public const string CoresFile = "cores.txt";
        // public const string tempIco = "temp.ico";
        public const byte MAX_PATH = 255; // Apply Everywhere?
        private const string WinLinkExt = ".lnk";
        private const string LinLinkExt = ".desktop";
        public const string LinuxRABin = "retroarch";
        public const string DotDesktopRAIcon = LinuxRABin;


        public static List<string> ConfigDir { get; private set; } = new();

        public static readonly string BaseDir = AppDomain.CurrentDomain.BaseDirectory;
        // private static string PathToSettingFileBin = Path.Combine(BaseDir, SettingFileBin);
        private static string PathToSettingFileJson = Path.Combine(BaseDir, SettingFileJson);
        public static string DefUserAssetsDir = Path.Combine(BaseDir, DefUserAssets);
        
        public static readonly List<string> WinExtraIconsExt = ["*.png", "*.jpg", "*.jpeg", "*.svg", "*.svgz"];
        public static readonly List<string> LinIconsExt = ["*.ico", "*.png", "*.xpm", "*.svg", "*.svgz"];
        
        public static readonly string UserDesktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        public static readonly string UserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        public static readonly string UserTemp = Path.Combine(Path.GetTempPath(), App.AppName);
        // Solution for cross-OS path separators thanks to Vilmir @ stackoverflow.com
        
        public static readonly string WINPublicUser = "C:\\Users\\Public";
        public static readonly string WINPublicDesktop = Path.Combine(WINPublicUser, "Desktop");
        
        private static Settings LoadedSettings = new();


        #region Settings

        public static bool ExistSettingsJsonFile() => File.Exists(PathToSettingFileJson);

        public static Settings LoadSettingsFO()
        {
            LoadedSettings = SettingsOps.LoadSettings();
            App.Logger?.LogDebg("Settings loaded for FileOps.");
            BuildConfigDir(LoadedSettings);
            return LoadedSettings;
        }

        public static Settings LoadCachedSettingsFO()
        {
            LoadedSettings = SettingsOps.GetCachedSettings();
            BuildConfigDir(LoadedSettings);
            return LoadedSettings;
        }

        public static Settings SetNewSettings(Settings settings)
        {
            LoadedSettings = settings;
            return LoadedSettings;
        }

        public static Settings LoadDesignerSettingsFO(bool fixedOutput)
        {
            LoadedSettings = new Settings();
            BuildConfigDir(LoadedSettings);
            if (fixedOutput) LoadedSettings.AlwaysAskOutput = false;
            return LoadedSettings;
        }

        public static string ReadSettingsFile() => ReadFileTextToEnd(PathToSettingFileJson);
        
        public static async void WriteSettingsFile(string settingString)
        {
            try {
                await File.WriteAllTextAsync(PathToSettingFileJson, settingString);
                App.Logger?.LogInfo($"Setting file \"{PathToSettingFileJson}\" written successfully");
            }
            catch (Exception e) {
                App.Logger?.LogErro($"Setting file \"{PathToSettingFileJson}\" could not be written!");
                App.Logger?.LogErro(e);
            }
        }

        private static void BuildConfigDir(Settings loadedSettings)
        {
            const string NormalRAconfig = "Default";
            ConfigDir = new List<string>() { NormalRAconfig };
            if (loadedSettings.PrevConfig) ConfigDir.AddRange(SettingsOps.PrevConfigs);
        }

        #endregion

        #region Load
        
        public static bool GetCoreFile(out string file)
        {
            var externalCores = Path.Combine(LoadedSettings.UserAssetsPath, CoresFile);
            if (!File.Exists(externalCores))
            {
                file = string.Empty;
                return false;
            }
            file = externalCores;
            return true;
        }
        
        public static string[] LoadCores(string filePath)
        {
            try
            {
                App.Logger?.LogInfo($"Starting reading of \"{filePath}\".");
                var cores = ReadFileLinesToEnd(filePath);
                App.Logger?.LogInfo($"Completed reading of \"{filePath}\".");
                return cores;
            }
            catch
            {
                App.Logger?.LogWarn($"The file \"{filePath}\" could not be found!");
                return Array.Empty<string>();
            }
        }

        public static object[] LoadIcons(bool OS)
        {
            // TODO: Refactor all of this. Also, tuples are a thing in C# (0.8)
            var dir = LoadedSettings.UserAssetsPath + Path.DirectorySeparatorChar;
            var files = new List<string>();
            var isError = false;
            var iconException = string.Empty;
            
            try
            {
                files = new(Directory.EnumerateFiles(LoadedSettings.UserAssetsPath));
                App.Logger?.LogInfo($"Searching for Icons at \"{dir}\".");
                for (int i = 0; i < files.Count; i++)
                {
                    string ext = Path.GetExtension(files[i]);
                    string filename = Path.GetFileName(files[i]);
                    string filepath = Path.GetFullPath(Path.Combine(files[i]));
                    if (OS)
                    {
                        if (WinExtraIconsExt.Contains("*" + ext) || (ext is ".exe")) 
                            IconProc.IconItemsList.Add(new IconsItems(filename, filepath, true));
                        else if (ext is ".ico") IconProc.IconItemsList.Add(new IconsItems(filename, filepath));
                    }
                    else if (LinIconsExt.Contains("*" + ext)) 
                        IconProc.IconItemsList.Add(new IconsItems(filename, filepath));
                }

                App.Logger?.LogInfo(files.Count == 0 ? "No icons found." : $"{IconProc.IconItemsList.Count} icons were found.");

                files.Clear();
                int newindex = 1;
                foreach (var file in IconProc.IconItemsList)
                {
                    files.Add(file.FileName);
                    file.comboIconIndex = newindex;
                    newindex++;
                }
            }
            catch (DirectoryNotFoundException e)
            {
                // TODO: Catch more types of exceptions
                App.Logger?.LogWarn($"The directory \"{dir}\" could not be found.");
                iconException = e.Message;
                isError = true;
            }
            catch (Exception e)
            {
                App.Logger?.LogErro("An error has occurred while loading icons.");
                App.Logger?.LogDebg($"Source: {e.Source}");
                App.Logger?.LogErro(e.Message);
                iconException = e.Message;
                isError = true;
            }
            
            return [ files, isError, iconException ];
        }
        #endregion

        #region FUNCTIONS

        public static string? GetDirFromPath(string path) => Path.GetDirectoryName(path);
        
        public static string GetFileNameFromPath(string pathToFile) => Path.GetFileName(pathToFile);
        
        public static string GetFileNameNoExtFromPath(string pathToFile) => Path.GetFileNameWithoutExtension(pathToFile);

        public static string CombineDirAndFile(string dir, string file) => Path.Combine(dir, file);

        public static string GetDirAndCombine(string fullPath, string newFileName) {
            var dir = GetDirFromPath(fullPath);
            if (string.IsNullOrWhiteSpace(dir)) dir = BaseDir;
            return Path.Combine(dir, newFileName);
        }

        public static string[] ReadFileLinesToEnd(string filePath) => File.ReadAllLines(filePath);
        
        public static string ReadFileTextToEnd(string filePath) => File.ReadAllText(filePath);

        public static string GetOutputExt(bool os) => (os) ? WinLinkExt : LinLinkExt;

        private static bool CheckUsrSetDir(string path)
        {
            try {
                Directory.CreateDirectory(path);
                return true;
            }
            catch (Exception e) {
                App.Logger?.LogDebg(e.Message);
                App.Logger?.LogErro($"The folder \"{path}\" could not be created!");
                return false;
            }
        }

        public static string GetDefinedLinkPath(string linkName, string linkPath) {
            var newDir = Path.GetFileName(linkName);
            newDir = Path.Combine(linkPath, newDir);
            return newDir;
        }
        
        public static string DumpStreamToFile(Stream fileStream)
        {
            fileStream.Position = 0;
            CheckUsrSetDir(UserTemp);
            var temporalFile = Path.Combine(UserTemp, tempFile);
            var streamReader = new StreamReader(fileStream);
            File.WriteAllText(temporalFile, streamReader.ReadToEnd());
            return temporalFile;
        }

        public static ShortcutterOutput[] GetLinkCopyPaths(List<string> linkCopyList, ShortcutterOutput linkOutputBase)
        {
            var linkCopies = new ShortcutterOutput[linkCopyList.Count];
            for (int i = 0; i < linkCopies.Length; i++)
            {
                linkCopies[i] = new ShortcutterOutput(linkOutputBase, linkCopyList[i]);
            }
            return linkCopies;
        }

        public static bool IsConfigFile(string filePath, out string fileExt)
        {
            fileExt = Path.GetExtension(filePath);
            return (fileExt is ".txt" or ".cfg");
        }
        
        #endregion

        #region ICONS

        public static string CpyIconToUsrSet(string ogPath)
        {
            string name = Path.GetFileName(ogPath);
            string newPath = Path.Combine(LoadedSettings.IcoSavPath, name);
            CheckUsrSetDir(LoadedSettings.IcoSavPath);
            if (File.Exists(newPath)) return Path.GetFullPath(newPath);
            
            // TODO: Update IconItem
            File.Copy(ogPath, newPath);
            return Path.GetFullPath(newPath);
        }

        public static string CpyIconToCustomSet(string ogPath, string destPath)
        {
            destPath = Path.GetDirectoryName(destPath)!;
            string name = Path.GetFileName(ogPath);
            string newPath = Path.Combine(destPath, name);
            if (File.Exists(newPath)) return Path.GetFullPath(newPath);

            File.Copy(ogPath, newPath);
            return Path.GetFullPath(newPath);
        }

        public static bool IsVectorImage(string file) => (Path.GetExtension(file) is ".svg" or ".svgz");

        public static bool IsFileAnIcon(string filePath, bool OS, out string fileExt)
        {
            var extList = new List<string>();
            if (OS)
            {
                foreach (var ext in WinExtraIconsExt)
                {
                    var fixedExt = ext.Remove(0, 1);
                    extList.Add(fixedExt);
                }
                extList.Add(".exe");
                extList.Add(".dll");
                extList.Add(".ico");
            }
            else extList.AddRange(LinIconsExt);
            fileExt = Path.GetExtension(filePath);
            
            return extList.Contains(fileExt);
        }

        #endregion


        #region Windows Only Ops

        public static bool IsExtWinPE(string ext) => ext is ".exe" or ".dll";
        
        public static bool IsFileWinPE(string file) => IsExtWinPE(Path.GetExtension(file));

        public static string SaveWinIco(IconsItems selectedIconItem)
        {
            string icoExt = Path.GetExtension(selectedIconItem.FileName);
            string icoName = Path.GetFileNameWithoutExtension(selectedIconItem.FileName) + ".ico";
            string newDir = (CheckUsrSetDir(UserTemp)) ? UserTemp : LoadedSettings.UserAssetsPath;
            string newPath = Path.Combine(newDir, icoName);
            ImageMagick.MagickImage iconImage;
            if (selectedIconItem.IconStream != null) selectedIconItem.IconStream.Position = 0;

            switch (icoExt)
            {
                case ".svg" or ".svgz":
                    iconImage = IconProc.ImageConvert(selectedIconItem.IconStream!);
                    iconImage.Write(newPath);
                    //new_dir = CpyIconToUsrSet(new_dir);
                    break;
                case ".exe" or ".dll":
                    if (LoadedSettings.ExtractIco)
                    {
                        iconImage = IconProc.ImageConvert(selectedIconItem.IconStream!);
                        iconImage.Write(newPath);
                    }
                    else newPath = selectedIconItem.FilePath;
                    break;
                default: // .jpg, .png, etc
                    iconImage = IconProc.ImageConvert(selectedIconItem.FilePath);
                    iconImage.Write(newPath);
                    //new_dir = CpyIconToUsrSet(new_dir);
                    break;
            }
            return newPath;
        }

        public static string ChangeIcoNameToLinkName(Shortcutter linkObj)
        {
            var iconFilePath = linkObj.ICONfile!;
            string iconPath = GetDirFromPath(iconFilePath)!;
            string linkName = Path.ChangeExtension(linkObj.OutputPaths[0].FileName, ".ico");
            string newIconPath = Path.Combine(iconPath, linkName);
            
            File.Copy(iconFilePath, newIconPath, true);
            File.Delete(iconFilePath);
            
            return newIconPath;
        }
        
        public static string WriteIcoToFile(MemoryStream icoStream, string outputPath)
        {
            // Obsolete?
            var fileInfo = new FileInfo(outputPath);
            var fileStream = fileInfo.Create();
            fileStream.Write(icoStream.ToArray());
            fileStream.Close();
            return fileInfo.FullName;
        }

        #endregion

        #region Linux Only Ops

        private static string[] SeparateFileNameFromPath(string path)
        {
            return
            [
                // File Full Path
                path,
                
                // File Name Without Extension
                GetFileNameNoExtFromPath(path),
                
                // File Name
                GetFileNameFromPath(path),
                
                // Extension
                Path.GetExtension(path)
            ];
        }
        
        public static string[] DesktopEntryArray(string LinkDir, string? core)
        {
            var EntryName = SeparateFileNameFromPath(LinkDir);
            EntryName[2] = DesktopEntry.StdDesktopEntry(EntryName[1], core);
            EntryName[2] += EntryName[3];
            EntryName[0] = Path.Combine(Path.GetDirectoryName(EntryName[0])!, EntryName[2]);
            return EntryName;
        }
        
        public static string GetSystemRAIcons()
        {
            // TODO: Find a way to use xdg-desktop-icon and/or xdg-icon-resource to access linux desktop icon files
            return string.Empty;
        }
        
        public static void WriteDesktopEntry(string outputFile, byte[] fileBytes) => File.WriteAllBytes(outputFile, fileBytes);

        [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
        public static void MakeFileExecutable(string filePath)
        {
            
            // Freaking bit magic: https://aaronbos.dev/posts/csharp-flags-enum
            // |= Adds file mode to existing ones
            // &= idk, it cleared all file modes and only left added one
            // -= Removes file mode from existing ones
            var fileInfo = new FileInfo(filePath);
            fileInfo.UnixFileMode |= UnixFileMode.UserExecute | UnixFileMode.GroupExecute;
        }

        #endregion
    }
}
