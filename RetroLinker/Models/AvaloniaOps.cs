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
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using RetroLinker.Translations;
using RetroLinker.Views;
using AvaloniaBitmap = Avalonia.Media.Imaging.Bitmap; // To distinguish Avalonia's bitmap from the images NuGets

namespace RetroLinker.Models;

public static class AvaloniaOps
{
    private static bool FirstLoad = true;
    private static string[] Cores = [];
    private const string CoreList = "avares://RetroLinkerLib/Assets/cores.txt";
    private const string DEFicon1 = "avares://RetroLinkerLib/Assets/Icons/retroarch.ico";
    private const string NaN = "avares://RetroLinkerLib/Assets/Images/NaN.png";
    
    public static IStorageFolder? DesktopFolder { get; private set; }
    public static IStorageFolder? ROMTopDir { get; private set; }

    
    #region FUNCTIONS
    public static void MainViewPreConstruct(MainWindow mainWindow, out Settings settings)
    {
        if (mainWindow.IsDesigner)
        {
            settings = FileOps.LoadDesignerSettingsFO(false);
            return;
        }
        if (FirstLoad)
        {
            Trace.WriteLine($"Current OS: {RuntimeInformation.OSDescription}", App.InfoTrace);
            settings = FileOps.LoadSettingsFO();
            Debug.WriteLine("Settings loaded for MainView.", App.DebgTrace);
            Debug.WriteLine("Settings converted to Base64:" + settings.GetBase64(), App.DebgTrace);
        
            LanguageManager.SetLocale(settings.LanguageCulture);
        }
        else settings = FileOps.LoadCachedSettingsFO();
    }

    public static void DesignerMainViewPreConstruct(out Settings settings) {
        settings = FileOps.LoadDesignerSettingsFO(false);
    }

    public static void MainViewLoad() {
        FirstLoad = false;
    }

    public static string[] GetCoresArray()
    {
        if (Cores.Length >= 1) return Cores;
        if (!FileOps.GetCoreFile(out string coresFile))
        {
            var assetStream = AssetLoader.Open(GetDefaultCores());
            coresFile = FileOps.DumpStreamToFile(assetStream);
        }
        Cores = FileOps.LoadCores(coresFile);
        return Cores;
    }

    private static Uri GetDefaultCores() => new(CoreList);
    
    public static Uri GetDefaultIcon() => new(DEFicon1);

    public static Uri GetNAimage() => new(NaN);
    
    public static AvaloniaBitmap GetBitmap(string path) => new(path);

    public static AvaloniaBitmap GetBitmap(Stream imgStream) => new(imgStream);

    private static async Task<IStorageFolder?> GetStorageFolder(string dir, TopLevel topLevel) =>  
        await topLevel.StorageProvider.TryGetFolderFromPathAsync(dir);

    public static async void SetDesktopStorageFolder(TopLevel topLevel)
    {
        DesktopFolder = await GetStorageFolder(FileOps.UserDesktop, topLevel);
        var dbgOut = (DesktopFolder is null) 
            ? $"DesktopStorageFolder remained null. Attempted dir: \"{FileOps.UserDesktop}\"" 
            : $"DesktopStorageFolder set to: \"{DesktopFolder.Path.LocalPath}\"";
        Debug.WriteLine(dbgOut, App.DebgTrace);
    }
    
    public static async void SetROMTop(string? dir_ROMTop, TopLevel topLevel)
    {
        if (string.IsNullOrWhiteSpace(dir_ROMTop)) return;
        ROMTopDir = await GetStorageFolder(dir_ROMTop, topLevel);
        var dbgOut = (ROMTopDir is null)
            ? $"ROMPadreStorageFolder remained null. Attempted dir:\"{dir_ROMTop}\""
            : $"ROMPadreStorageFolder set to: \"{ROMTopDir.Path.LocalPath}\"";
        Debug.WriteLine(dbgOut, App.DebgTrace);
    }
    #endregion
    
    #region FileDialogs
    public static async Task<string> OpenFileAsync(PickerOpt.OpenOpts template, string currentFile, TopLevel topLevel)
    {
        var opt = PickerOpt.OpenPickerOpt(template);
        if (!string.IsNullOrEmpty(currentFile))
        {
            currentFile = FileOps.GetDirFromPath(currentFile)!;
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
            currentFile = FileOps.GetDirFromPath(currentFile)!;
            opt.SuggestedStartLocation = await GetStorageFolder(currentFile, topLevel);
        }
        var file = await topLevel.StorageProvider.SaveFilePickerAsync(opt);
        string dir = (file != null) ? file.Path.LocalPath : string.Empty;
        return dir;
    }
    #endregion
}