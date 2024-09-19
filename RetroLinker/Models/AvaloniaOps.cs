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
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using RetroLinker.Translations;

namespace RetroLinker.Models;

public static class AvaloniaOps
{
    private static bool FirstLoad = true;
    private static Task<string[]> coresTask;
    private static Task<object[]> iconTask;
    private static string[] cores = Array.Empty<string>();
    private static object[] iconList = Array.Empty<object>();
    private const string CoreList = "avares://RetroLinkerLib/Assets/cores.txt";
    private const string DEFicon1 = "avares://RetroLinkerLib/Assets/Icons/retroarch.ico";
    private const string NoAplica = "avares://RetroLinkerLib/Assets/Images/no-aplica.png";
    
    public static IStorageFolder? DesktopFolder { get; private set; }
    public static IStorageFolder? ROMTopDir { get; private set; }

    
    #region FUNCTIONS
    public static Settings MainViewPreConstruct()
    {
        if (!FirstLoad) return FileOps.LoadCachedSettingsFO();
        System.Diagnostics.Trace.WriteLine($"Current OS: {RuntimeInformation.OSDescription}", App.InfoTrace);
        var settings = FileOps.LoadSettingsFO();
        System.Diagnostics.Debug.WriteLine("Settings loaded for MainView.", App.DebgTrace);
        System.Diagnostics.Debug.WriteLine("Settings converted to Base64:" + settings.GetBase64(), App.DebgTrace);
        
        LanguageManager.SetLocale(settings.LanguageCulture);
        
        return settings;
    }

    public static Settings DesignerMainViewPreConstruct() => FileOps.LoadDesignerSettingsFO(false);

    public static void MainViewLoad(bool DesktopOS)
    {
        string coreFile;
        if (!FileOps.GetCoreFile(out coreFile))
        {
            var assetStream = AssetLoader.Open(GetDefaultCores());
            coreFile = FileOps.DumpStreamToFile(assetStream);
        }
        coresTask = FileOps.LoadCores(coreFile);
        iconTask = FileOps.LoadIcons(DesktopOS);

        FirstLoad = false;
    }

    public static async Task<string[]> GetCoresArray()
    {
        if (cores.Length < 1) cores = await coresTask;
        return cores;
    }
    
    public static async Task<object[]> GetIconList()
    {
        if (iconList.Length != 0) return iconList;
        iconList = await iconTask;
        return iconList;
    }

    public static Uri GetDefaultCores() => new Uri(CoreList);
    
    public static Uri GetDefaultIcon() => new Uri(DEFicon1);

    public static Uri GetNAimage() => new Uri(NoAplica);
    
    public static Bitmap GetBitmap(string path) => new Bitmap(path);

    public static Bitmap GetBitmap(Stream imgStream) => new Bitmap(imgStream);

    private static async Task<IStorageFolder?> GetStorageFolder(string dir, TopLevel topLevel) =>  
        await topLevel.StorageProvider.TryGetFolderFromPathAsync(dir);

    public static async void SetDesktopStorageFolder(TopLevel topLevel)
    {
        DesktopFolder = await GetStorageFolder(FileOps.UserDesktop, topLevel);
        System.Diagnostics.Debug.WriteLine($"DesktopStorageFolder set to: {DesktopFolder.Path.LocalPath}", App.DebgTrace);
    }
    
    public static async void SetROMTop(string? dir_ROMTop, TopLevel topLevel)
    {
        if (!string.IsNullOrWhiteSpace(dir_ROMTop))
        {
            ROMTopDir = await GetStorageFolder(dir_ROMTop, topLevel);
            System.Diagnostics.Debug.WriteLine($"ROMPadreStorageFolder set to: {ROMTopDir.Path.LocalPath}", App.DebgTrace);
        }
    }
    #endregion
    
    #region FileDialogs
    public static async Task<string> OpenFileAsync(PickerOpt.OpenOpts template, string currentFile, TopLevel topLevel)
    {
        var opt = PickerOpt.OpenPickerOpt(template);
        if (!string.IsNullOrEmpty(currentFile))
        {
            currentFile = FileOps.GetDirFromPath(currentFile);
            opt.SuggestedStartLocation = await GetStorageFolder(currentFile, topLevel);
        }
        var file = await topLevel.StorageProvider.OpenFilePickerAsync(opt);
        string dir = (file.Count > 0) ? Path.GetFullPath(file[0].Path.LocalPath) : string.Empty;
        return dir;
    }

    public static async Task<string> OpenFileAsync(FilePickerOpenOptions openOptions, TopLevel topLevel)
    {
        var file = await topLevel.StorageProvider.OpenFilePickerAsync(openOptions);
        string dir = file.Count > 0 ? Path.GetFullPath(file[0].Path.LocalPath) : string.Empty;
        return dir;
    }

    public static async Task<string> OpenFolderAsync(byte template, string currentFolder, TopLevel topLevel)
    {
        FolderPickerOpenOptions opt = new()
        {
            AllowMultiple = false,
            Title = template switch
            {
                0 => resAvaloniaOps.dlgFolderUserAssets,
                1 => resAvaloniaOps.dlgFolderROMParent,
                2 => resAvaloniaOps.dlgFolderIcoOutput,
                3 => resAvaloniaOps.dlgFolderLinkCopy,
                // This option shouldn't happen
                _ => resAvaloniaOps.dlgFolderFallback
            },
        };
        if (!string.IsNullOrEmpty(currentFolder))
            opt.SuggestedStartLocation = await GetStorageFolder(currentFolder, topLevel);
        
        var dirList = await topLevel.StorageProvider.OpenFolderPickerAsync(opt);
        string dir = dirList.Count > 0 ? Path.GetFullPath(dirList[0].Path.LocalPath) : string.Empty;
        return dir;
    }

    public static async Task<string> SaveFileAsync(PickerOpt.SaveOpts template, string currentFile, TopLevel topLevel)
    {
        var opt = PickerOpt.SavePickerOpt(template);
        if (!string.IsNullOrEmpty(currentFile))
        {
            currentFile = FileOps.GetDirFromPath(currentFile);
            opt.SuggestedStartLocation = await GetStorageFolder(currentFile, topLevel);
        }
        var file = await topLevel.StorageProvider.SaveFilePickerAsync(opt);
        string dir = (file != null) ? file.Path.LocalPath : string.Empty;
        return dir;
    }
    #endregion
}