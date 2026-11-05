/*
    RetroLinker: A .NET GUI application to help create desktop links of games running on RetroArch.
    Copyright (C) 2023-2026 Kevin Rafael Martinez Johnston

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

using System.IO;
using RetroLinker.Models.Settings;

namespace RetroLinker.Models.IO;

public static class FileOps
{
    public const string SettingFileJson = "RLsettings.json";
    public const string DefUserAssets = "UserAssets";
    public const string DefLicenses = "Licenses";
    public const string CoresFile = "cores.txt";
    // public const string tempIco = "temp.ico";
    // public const byte MAX_PATH = 255;
    public const string WinPeExtExe = ".exe";
    public const string WinPeExtDll = ".dll";
    public const string WinIcoExt = ".ico";
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
    public static readonly List<string> WinAllIconsExt = [$"*{WinIcoExt}", $"*{WinPeExtExe}", $"*{WinPeExtDll}", ..WinExtraIconsExt];
    public static readonly List<string> LinIconsExt = [$"*{WinIcoExt}", "*.png", "*.xpm", "*.svg", "*.svgz"];
        
    public static readonly string UserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    public static readonly string UserDesktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    public static readonly string UserData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    public static readonly string UserTemp = Path.Combine(Path.GetTempPath(), Translations.resGeneric.GenAppName);
    // Solution for cross-OS path separators thanks to Vilmir @ stackoverflow.com
        
    public static readonly string WINPublicUser = Path.Combine("C:", "Users", "Public");
    public static readonly string WINPublicDesktop = Path.Combine(WINPublicUser, "Desktop");
    public static readonly string WINUserStartMenu = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
    public static readonly string WINSystemStartMenu = Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms);

    private static readonly DirectoryInfo LicensesDir = new(DefLicensesDir);
    public static readonly FileInfo[] LicensesFiles = (LicensesDir.Exists) ? LicensesDir.GetFiles() : [ ];
        
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
        
    private static Settings.Settings _loadedSettings = new();


    #region Settings

    public static bool ExistSettingsJsonFile() => File.Exists(PathToSettingFileJson);

    public static async Task<Settings.Settings> LoadSettingsFO()
    {
        _loadedSettings = await SettingsOps.LoadSettings();
        Logger.LogDebg($"Settings loaded for {nameof(FileOps)}");
        BuildConfigDir(_loadedSettings);
        return _loadedSettings;
    }

    public static Settings.Settings LoadCachedSettingsFO()
    {
        _loadedSettings = SettingsOps.GetCachedSettings();
        BuildConfigDir(_loadedSettings);
        return _loadedSettings;
    }

    public static Settings.Settings SetNewSettings(Settings.Settings settings)
    {
        _loadedSettings = settings;
        return _loadedSettings;
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

    private static void BuildConfigDir(Settings.Settings loadedSettings)
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
        var externalCores = Path.Combine(_loadedSettings.UserAssetsPath, CoresFile);
        if (!File.Exists(externalCores)) {
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
        catch (Exception e) {
            Logger.LogWarn($"The file \"{filePath}\" could not be found!");
            Logger.LogErro(e);
            return Array.Empty<string>();
        }
    }

    public static List<IIconItem> LoadIcons()
    {
        var dir = _loadedSettings.UserAssetsPath + OsDirSeparator;
        var validIcons = new List<IIconItem>();

        var extList = (App.PlatformOs) ? WinAllIconsExt : LinIconsExt;
        var filesList = new DirectoryInfo(dir).GetFiles();
        Logger.LogInfo($"Searching for Icons at \"{dir}\".");
            
        foreach (var file in filesList)
            if (extList.Contains("*" + file.Extension.ToLowerInvariant())) validIcons.Add(new IconItem(file.FullName));

        Logger.LogInfo(validIcons.Count == 0
            ? "No icons found."
            : $"{validIcons.Count} icons were found.");
            
        return validIcons;
    }
    #endregion

    #region ABSTRACTIONS

    public static MemoryStream BytesToStream(byte[] bytes) => new(bytes);

    public static string GetAbsolutePath(string path) => Path.GetFullPath(path);
        
    public static string? GetDirFromPath(string path) => Path.GetDirectoryName(path);
    
    public static string ForceGetDirFromPath(string path) => GetDirFromPath(path) ?? BaseDir;
        
    public static string GetFileNameFromPath(string pathToFile) => Path.GetFileName(pathToFile);
        
    public static string GetFileNameNoExtFromPath(string pathToFile) => Path.GetFileNameWithoutExtension(pathToFile);
        
    public static string GetFileExtFromPath(string pathToFile) => Path.GetExtension(pathToFile);
    
    [return: NotNullIfNotNull("pathToFile")]
    public static string? ChangeFileExtension(string? pathToFile, string? newExtension) => Path.ChangeExtension(pathToFile, newExtension);
        
    public static string CombineMultipleInputs(params string[] paths) => Path.Combine(paths);
        
    public static bool PathAlreadyExists(string path) => Path.Exists(path);

    public static bool PathAlreadyExistsAndNotEmpty(string path) {
        if (!Path.Exists(path)) return true;
        else return (GetFileInfo(path).Length > 0);
    }
    
    public static Task<string[]> ReadFileLinesToEndAsync(string filePath) => File.ReadAllLinesAsync(filePath);
    public static async Task<string> ReadFileTextToEndAsync(string filePath) => await File.ReadAllTextAsync(filePath);
    public static async Task<byte[]> ReadFileBytesToEndAsync(string filePath) => await File.ReadAllBytesAsync(filePath);

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
    /// <remarks>Use this when calling fron the UI.</remarks>
    public static async Task DeleteFileAsync(string filePath) 
        => await Task.Run(() => File.Delete(filePath));
        
    /// <summary>
    /// A wrapper for <see cref="M:File.WriteAllBytesAsync(string,byte[],System.Threading.CancellationToken)"/>.
    /// </summary>
    /// <param name="filePath">Path of the file to write.</param>
    /// <param name="bytes">The bytes to be written.</param>
    /// <remarks>Use this when calling fron the UI.</remarks>
    public static async Task WriteAllBytesToFileAsync(string filePath, byte[] bytes) 
        => await File.WriteAllBytesAsync(filePath, bytes);
        
        
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

    public static string GenerateTempFileName(string? extension = null)
    {
        if (string.IsNullOrWhiteSpace(extension)) extension = ".tmp";
        if (!extension.StartsWith('.')) extension = extension.Insert(0, ".");
        var newFileName = new System.Text.StringBuilder();
        newFileName.Append(DateTime.Now.ToString("yyyy-MM-dd_HHmmss"));
        newFileName.Append('_');
        newFileName.Append(Random.Shared.GetHexString(4, true));
        newFileName.Append(extension);
        return newFileName.ToString();
    }
    
    public static string GenerateTempFileFullName(string? extension = null) 
        => CombineMultipleInputs(UserTemp, GenerateTempFileName(extension));

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
        
    public static async Task<string> DumpStreamToFile(Stream fileStream, string? fileName = null)
    {
        if (string.IsNullOrWhiteSpace(fileName)) fileName = GenerateTempFileName();
        fileStream.Position = 0;
        CheckUsrSetDir(UserTemp);
        var destFile = CombineMultipleInputs(UserTemp, fileName);
        await using var writer = new FileStream(destFile, FileMode.Create);
        await fileStream.CopyToAsync(writer);
        await writer.FlushAsync();
        writer.Close();
        return destFile;
    }

    public static bool IsConfigFile(string filePath, out string fileExt) {
        fileExt = GetFileExtFromPath(filePath);
        return (fileExt.ToLowerInvariant() is ".txt" or ".cfg");
    }
        
    public static string SeparateFileNameFromPath(string path, out string fileNameNoExt, out string fileExt)
    {
        // File Name Without Extension
        fileNameNoExt = GetFileNameNoExtFromPath(path);
                
        // Extension
        fileExt = GetFileExtFromPath(path);
            
        // File Name
        return GetFileNameFromPath(path);
    }
        
    #endregion

    #region ICONS

    public static string RenameIconToUsrSett(string ogPath)
    {
        if (string.IsNullOrEmpty(ogPath)) return string.Empty;  // If we analyze this in a vacuum, this should throw
        var name = GetFileNameFromPath(ogPath);
        var newPath = CombineMultipleInputs(_loadedSettings.IcoSavPath, name);
        CheckUsrSetDir(_loadedSettings.IcoSavPath);
        return GetAbsolutePath(newPath);
    }

    public static string RenameIconToCustomSett(string ogPath, string destPath) {
        destPath = ForceGetDirFromPath(destPath);
        var ogName = GetFileNameFromPath(ogPath);
        var newPath = CombineMultipleInputs(destPath, ogName);
        return GetAbsolutePath(newPath);
    }

    public static bool IsVectorImage(string file) => (GetFileExtFromPath(file).ToLowerInvariant() is ".svg" or ".svgz");

    public static bool IsFileAnIcon(string filePath, bool OS, out string fileExt)
    {
        var extList = new List<string>();
        if (OS)
        {
            foreach (var ext in WinExtraIconsExt) {
                var fixedExt = ext.Remove(0, 1);
                extList.Add(fixedExt);
            }
            extList.AddRange(WinIcoExt, WinPeExtExe, WinPeExtDll);
        }
        else extList.AddRange(LinIconsExt);
        
        fileExt = GetFileExtFromPath(filePath).ToLowerInvariant();
        return extList.Contains(fileExt);
    }

    #endregion


    #region Windows Only Ops

    public static bool IsExtWinPE(string ext) => ext.Equals(WinPeExtExe, StringComparison.OrdinalIgnoreCase) || 
                                                 ext.Equals(WinPeExtDll, StringComparison.OrdinalIgnoreCase);
    
    public static bool IsFileWinPE(string file) => IsExtWinPE(GetFileExtFromPath(file));

    [SupportedOSPlatform(App.PlatformWin)]
    public static string GetConvertedIcoPath(DesktopLink.IShortcutBase linkParameters, string raRom, IIconItem selectedIconItem)
    {
        // TODO: test
        if (IsExtWinPE(selectedIconItem.FileExt) && !_loadedSettings.ExtractIco) return selectedIconItem.FilePath;
        
        var romIcoSave = (string.IsNullOrEmpty(raRom) || raRom == CommandLine.Manager.Contentless)
            ? linkParameters.RaExecutable
            : raRom;
        
        var newPath = GetFileNameNoExtFromPath(selectedIconItem.FileName) + WinIcoExt;
        if (_loadedSettings.IcoLinkName) {
            var dir = ForceGetDirFromPath(linkParameters.IconPath);
            var newName = ChangeFileExtension(linkParameters.OutputFileName, WinIcoExt);
            newPath = CombineMultipleInputs(dir, newName);
        }
        newPath = _loadedSettings.IcoSavPath switch {
            SettingsOps.IcoSavROM => RenameIconToCustomSett(newPath, romIcoSave),
            SettingsOps.IcoSavRA => RenameIconToCustomSett(newPath, linkParameters.RaExecutable),
            _ => RenameIconToUsrSett(newPath)
        };
        return newPath;
    }

    // public static string WriteImageToTemp(ImageMagick.MagickImage image, string? fileName = null)
    // {
    //     if (string.IsNullOrEmpty(fileName)) fileName = DateTime.Now.ToString("yyyyMMddHHmmss");
    //     fileName = GetFileNameNoExtFromPath(fileName) + ".";
    //     fileName += image.Format.ToString("G").ToLower();
    //     var newDir = CombineMultipleInputs(UserTemp, fileName);
    //     if (File.Exists(newDir)) File.Delete(newDir);
    //     image.Write(newDir);
    //     image.Dispose();
    //     return newDir;
    // }

    #endregion

    #region Linux Only Ops
        
    [UnsupportedOSPlatform(App.PlatformWin)]
    public static void MakeUnixFileExecutable(string filePath, bool forAll = false)
    {
        // Freaking bit magic: https://aaronbos.dev/posts/csharp-flags-enum
        // |= Adds file mode to existing ones
        // &= idk, it cleared all file modes and only left added one
        // -= Removes file mode from existing ones
        var fileInfo = new FileInfo(filePath);
        fileInfo.UnixFileMode |= UnixFileMode.UserExecute | UnixFileMode.GroupExecute;
        if (forAll) fileInfo.UnixFileMode |= UnixFileMode.OtherExecute;
    }

    #endregion
}