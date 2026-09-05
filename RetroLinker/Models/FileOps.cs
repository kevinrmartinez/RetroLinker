/*
    RetroLinker: A .NET GUI application to help create desktop links of games running on RetroArch.
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
using RetroLinker.Models.Linux;

namespace RetroLinker.Models
{
    public static class FileOps
    {
        public const string SettingFileJson = "RLsettings.json";
        public const string DefUserAssets = "UserAssets";
        public const string DefLicenses = "Licenses";
        public const string tempFile = "temp.txt";
        public const string CoresFile = "cores.txt";
        // public const string tempIco = "temp.ico";
        // public const byte MAX_PATH = 255; // Apply Everywhere?
        public const string WinPeExt1 = ".exe";
        public const string WinPeExt2 = ".dll";
        public const string WinLinkExt = ".lnk";
        public const string LinLinkExt = ".desktop";
        public const string LinuxRABin = "retroarch";
        public const string DotDesktopRAIcon = LinuxRABin;


        public static List<string> ConfigDir { get; private set; } = new();
        
        public static readonly char OsDirSeparator = Path.DirectorySeparatorChar;
        public static readonly string BaseDir = AppDomain.CurrentDomain.BaseDirectory;
        // private static string PathToSettingFileBin = Path.Combine(BaseDir, SettingFileBin);
        private static readonly string PathToSettingFileJson = Path.Combine(BaseDir, SettingFileJson);
        public static readonly string DefUserAssetsDir = Path.Combine(BaseDir, DefUserAssets);
        public static readonly string DefLicensesDir = Path.Combine(BaseDir, DefLicenses);
        
        
        public static readonly List<string> WinExtraIconsExt = ["*.png", "*.jpg", "*.jpeg", "*.svg", "*.svgz"];
        public static readonly List<string> LinIconsExt = ["*.ico", "*.png", "*.xpm", "*.svg", "*.svgz"];
        
        public static readonly string UserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        public static readonly string UserDesktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        public static readonly string UserData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public static readonly string UserTemp = Path.Combine(Path.GetTempPath(), Translations.resGeneric.GenAppName);
        // Solution for cross-OS path separators thanks to Vilmir @ stackoverflow.com
        
        public static readonly string WINPublicUser = Path.Combine("C:", "Users", "Public");
        public static readonly string WINPublicDesktop = Path.Combine(WINPublicUser, "Desktop");
        public static readonly string WINUserStartMenu = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
        public static readonly string WINSystemStartMenu = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
        
        public static string[] WinLinkPathCandidates { get; } =
        [
            UserDesktop,
            WINPublicDesktop,
            WINUserStartMenu,
            WINSystemStartMenu
            
        ];  // Source: https://en.wikipedia.org/wiki/Start_menu
        
        public static string[] LinLinkPathCandidates { get; } =
        [
            UserDesktop,
            CombineMultipleInputs(UserProfile, ".local", "share", "applications"),
            CombineMultipleInputs("/", "usr", "local", "share", "applications"),
            CombineMultipleInputs("/", "usr", "share", "applications")
        ];  // Source: https://askubuntu.com/questions/117341/how-can-i-find-desktop-files
        
        public static Dictionary<bool, string> TileIconOutDirs { get; } = new() {
            [false] = WINUserStartMenu,
            [true] = WINSystemStartMenu
        };
        
        private static Settings LoadedSettings = new();


        #region Settings

        public static bool ExistSettingsJsonFile() => File.Exists(PathToSettingFileJson);

        public static async Task<Settings> LoadSettingsFO()
        {
            LoadedSettings = await SettingsOps.LoadSettings();
            Logger.LogDebg($"Settings loaded for {nameof(FileOps)}");
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

        public static Task<string> ReadSettingsFile() => ReadFileTextToEndAsync(PathToSettingFileJson);
        
        public static async void WriteSettingsFile(string settingString)
        {
            try {
                await File.WriteAllTextAsync(PathToSettingFileJson, settingString);
                Logger.LogInfo($"Setting file \"{PathToSettingFileJson}\" written successfully");
            }
            catch (Exception e) {
                Logger.LogWarn($"Setting file \"{PathToSettingFileJson}\" could not be written!");
                Logger.LogErro(e);
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

        public static bool LogFileIsWritable(string logFilePath)
        {
            if (File.Exists(logFilePath))
            {
                try
                {
                    using var fsWriter = File.AppendText(logFilePath);
                    fsWriter.Write("================================================\n");
                    fsWriter.Close();
                }
                catch (Exception ex) {
                    Logger.LogErro(ex);
                    return false; 
                }
            }
            else
            {
                try {
                    File.Create(logFilePath).Close();
                }
                catch (Exception ex) {
                    Logger.LogErro(ex);
                    return false;
                }
            }
            return true;
        }
        
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
        
        public static async Task<string[]> LoadCores(string filePath)
        {
            try
            {
                Logger.LogInfo($"Starting reading of \"{filePath}\".");
                var cores = await ReadFileLinesToEndAsync(filePath);
                Logger.LogInfo($"Completed reading of \"{filePath}\".");
                return cores;
            }
            catch (Exception e)
            {
                Logger.LogWarn($"The file \"{filePath}\" could not be found!");
                Logger.LogErro(e);
                return Array.Empty<string>();
            }
        }

        public static (List<string>, string?) LoadIcons(bool DesktopOS)
        {
            var dir = LoadedSettings.UserAssetsPath + OsDirSeparator;
            var files = new List<string>();
            string? iconException = null;

            try
            {
                var filesList = new DirectoryInfo(dir).GetFiles();
                Logger.LogInfo($"Searching for Icons at \"{dir}\".");
                foreach (var file in filesList)
                {
                    var ext = file.Extension;
                    var filePath = file.FullName;
                    if (DesktopOS)
                    {
                        if (WinExtraIconsExt.Contains("*" + ext) || (ext is ".exe"))
                            IconProc.IconItemsList.Add(new IconsItems(filePath, true));
                        else if (ext is ".ico") IconProc.IconItemsList.Add(new IconsItems(filePath));
                    }
                    else if (LinIconsExt.Contains("*" + ext))
                        IconProc.IconItemsList.Add(new IconsItems(filePath));
                }

                Logger.LogInfo(filesList.Length == 0
                    ? "No icons found."
                    : $"{IconProc.IconItemsList.Count} icons were found.");

                var index = 1;
                foreach (var file in IconProc.IconItemsList) {
                    files.Add(file.FileName);
                    file.comboIconIndex = index;
                    index++;
                }
            }
            catch (Exception e) {
                Logger.LogErro("An error has occurred while loading icons...");
                Logger.LogErro(e);
                iconException = e.Message;
            }
            
            return (files, iconException);
        }
        #endregion

        #region ABSTRACTIONS

        public static string GetAbsolutePath(string path) => Path.GetFullPath(path);
        
        public static string? GetDirFromPath(string path) => Path.GetDirectoryName(path);
        
        public static string GetFileNameFromPath(string pathToFile) => Path.GetFileName(pathToFile);
        
        public static string GetFileNameNoExtFromPath(string pathToFile) => Path.GetFileNameWithoutExtension(pathToFile);
        
        public static string GetFileExtFromPath(string pathToFile) => Path.GetExtension(pathToFile);
        
        public static string CombineMultipleInputs(params string[] paths) => Path.Combine(paths);
        
        public static bool PathAlreadyExists(string path) => Path.Exists(path);

        public static bool PathAlreadyExistsAndNotEmpty(string path) {
            if (!Path.Exists(path)) return true;
            else return (GetFileInfo(path).Length > 0);
        }

        // public static string[] ReadFileLinesToEnd(string filePath) => File.ReadAllLines(filePath);
        public static Task<string[]> ReadFileLinesToEndAsync(string filePath) => File.ReadAllLinesAsync(filePath);
        
        // public static string ReadFileTextToEnd(string filePath) => File.ReadAllText(filePath);
        
        public static async Task<string> ReadFileTextToEndAsync(string filePath) => await File.ReadAllTextAsync(filePath);

        /// <summary>
        /// A wrapper for <see cref="File.Move(string,string,bool)"/>
        /// </summary>
        /// <remarks>Do not use when the UI is running. Reserve for critical operations.</remarks>
        public static void MoveFile(string source, string destination, bool overwrite = false) => File.Move(source, destination, overwrite);

        /// <summary>
        /// An async wrapper for <see cref="File.Move(string,string,bool)"/>
        /// </summary>
        /// <remarks>Use this when the UI is running.</remarks>
        public static async Task MoveFileAsync(string source, string destination, bool overwrite = false) {
            await Task.Run(() => File.Move(source, destination, overwrite));
        } 
        
        /// <summary>
        /// A wrapper for <see cref="File.Delete"/>
        /// </summary>
        /// <remarks>Do not use when the UI is running. Reserve for critical operations.</remarks>
        public static void DeleteFile(string filePath) => File.Delete(filePath);

        /// <summary>
        /// An async wrapper for <see cref="File.Delete"/>
        /// </summary>
        /// <remarks>Use this when the UI is running.</remarks>
        public static async Task DeleteFileAsync(string filePath) {
            await Task.Run(() => File.Delete(filePath));
        }
        
        
        public static FileInfo GetFileInfo(string filePath) => new(filePath);
        
        public static DirectoryInfo GetDirectoryInfo(string dirPath) => new(dirPath);

        public static List<FileInfo> GetFilesInDirectory(string dirPath, string searchPattern = "*", 
            EnumerationOptions? enumerationOptions = null)
        {
            enumerationOptions ??= new EnumerationOptions() {
                IgnoreInaccessible = true,
                MatchCasing = MatchCasing.CaseInsensitive,
                RecurseSubdirectories = false
            };
            var dirInfo = new DirectoryInfo(dirPath);
            return (dirInfo.Exists) ? new List<FileInfo>(dirInfo.GetFiles(searchPattern, enumerationOptions)) 
                : new List<FileInfo>();
        }

        #endregion

        #region FUNCTIONS

        public static string GetOutputExt(bool os) => (os) ? WinLinkExt : LinLinkExt;

        private static bool CheckUsrSetDir(string path)
        {
            try {
                Directory.CreateDirectory(path);
                return true;
            }
            catch (Exception e) {
                Logger.LogWarn($"The folder \"{path}\" could not be created!");
                Logger.LogErro(e);
                return false;
            }
        }

        public static string GetDefinedLinkPath(string linkName, string linkPath) {
            var newDir = Path.GetFileName(linkName);
            newDir = Path.Combine(linkPath, newDir);
            return newDir;
        }
        
        public static void DumpStreamToFile(Stream fileStream, out string destFile, string fileName = tempFile)
        {
            fileStream.Position = 0;
            CheckUsrSetDir(UserTemp);
            destFile = Path.Combine(UserTemp, fileName);
            var streamReader = new StreamReader(fileStream);
            File.WriteAllText(destFile, streamReader.ReadToEnd());
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
            fileExt = GetFileExtFromPath(filePath);
            return (fileExt is ".txt" or ".cfg");
        }
        
        #endregion

        #region ICONS

        public static string CpyIconToUsrSet(string? ogPath)
        {
            if (string.IsNullOrEmpty(ogPath)) return string.Empty;  // If we analyze this in a vacuum, this should throw
            string name = Path.GetFileName(ogPath);
            string newPath = Path.Combine(LoadedSettings.IcoSavPath, name);
            CheckUsrSetDir(LoadedSettings.IcoSavPath);
            // TODO: This line below is problematic, it doesn't let the user replace an existing item (0.9)
            //  Solution 1: Delete old, and copy (or overwrite)
            //  Solution 2: Resolve the naming conflict (add a number at the end of the file name)
            if (File.Exists(newPath)) return GetAbsolutePath(newPath);
            
            File.Copy(ogPath, newPath);
            return GetAbsolutePath(newPath);
        }

        public static string CpyIconToCustomSet(string ogPath, string destPath)
        {
            destPath = Path.GetDirectoryName(destPath)!;
            string name = Path.GetFileName(ogPath);
            string newPath = Path.Combine(destPath, name);
            if (File.Exists(newPath)) return GetAbsolutePath(newPath);
            
            File.Copy(ogPath, newPath);
            return GetAbsolutePath(newPath);
        }

        public static bool IsVectorImage(string file) => (GetFileExtFromPath(file) is ".svg" or ".svgz");

        public static bool IsFileAnIcon(string filePath, bool OS, out string fileExt)
        {
            var extList = new List<string>();
            if (OS)
            {
                foreach (var ext in WinExtraIconsExt) {
                    var fixedExt = ext.Remove(0, 1);
                    extList.Add(fixedExt);
                }
                extList.Add(".exe");
                extList.Add(".dll");
                extList.Add(".ico");
            }
            else extList.AddRange(LinIconsExt);
            fileExt = GetFileExtFromPath(filePath);
            
            return extList.Contains(fileExt);
        }

        #endregion


        #region Windows Only Ops

        public static bool IsExtWinPE(string ext) => ext is WinPeExt1 or WinPeExt2;
        
        public static bool IsFileWinPE(string file) => IsExtWinPE(GetFileExtFromPath(file));

        public static string SaveWinIco(IconsItems selectedIconItem)
        {
            string icoExt = GetFileExtFromPath(selectedIconItem.FileName).ToLower();
            string icoName = Path.GetFileNameWithoutExtension(selectedIconItem.FileName) + ".ico";
            string newDir = (CheckUsrSetDir(UserTemp)) ? UserTemp : LoadedSettings.UserAssetsPath;
            string newPath = Path.Combine(newDir, icoName);
            selectedIconItem.IconStream?.Position = 0;

            switch (icoExt)
            {
                case WinPeExt1 or WinPeExt2:
                    if (LoadedSettings.ExtractIco) {
                        var extractedIco = IconProc.ImageConvert(selectedIconItem.IconStream!);
                        extractedIco.Write(newPath);
                        extractedIco.Dispose();
                    }
                    else newPath = selectedIconItem.FilePath;
                    break;
                default: // .jpg, .png, etc
                    var iconImage = (icoExt is ".svg" or ".svgz") 
                        ? IconProc.ImageConvert(selectedIconItem.IconStream!)   // process IconStream for svg 
                        : IconProc.ImageConvert(selectedIconItem.FilePath);     // process image file 
                    iconImage.Write(newPath);
                    iconImage.Dispose();
                    break;
            }
            return newPath;
        }

        public static string ChangeIcoNameToLinkName(Shortcutter linkObj)
        {
            var iconFilePath = linkObj.ICONfile;
            string iconPath = GetDirFromPath(iconFilePath)!;
            string linkName = Path.ChangeExtension(linkObj.OutputPaths[0].FileName, ".ico");
            string newIconPath = Path.Combine(iconPath, linkName);
            
            File.Copy(iconFilePath, newIconPath, true);
            File.Delete(iconFilePath);
            
            return newIconPath;
        }

        public static string WriteImageToTemp(ImageMagick.MagickImage image, string? fileName = null)
        {
            if (string.IsNullOrEmpty(fileName)) fileName = DateTime.Now.ToString("yyyyMMddHHmmss");
            fileName = GetFileNameNoExtFromPath(fileName) + ".";
            fileName += image.Format.ToString("G").ToLower();
            var newDir = CombineMultipleInputs(UserTemp, fileName);
            if (File.Exists(newDir)) File.Delete(newDir);
            image.Write(newDir);
            image.Dispose();
            return newDir;
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
                GetFileExtFromPath(path)
            ];
        }
        
        public static string[] DesktopEntryArray(string LinkDir, string? core)
        {
            // TODO: Return a struct
            var EntryName = SeparateFileNameFromPath(LinkDir);
            EntryName[2] = DesktopEntry.StdDesktopEntry(EntryName[1], core);
            EntryName[2] += EntryName[3];
            EntryName[0] = Path.Combine(Path.GetDirectoryName(EntryName[0])!, EntryName[2]);
            return EntryName;
        }
        
        public static void WriteDesktopEntry(string outputFile, byte[] fileBytes) => File.WriteAllBytes(outputFile, fileBytes);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
        public static void MakeLinuxFileExecutable(string filePath)
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
