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
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using AvaloniaBitmap = Avalonia.Media.Imaging.Bitmap; // To distinguish Avalonia's bitmap from the images NuGets

namespace RetroLinker.Models.Avalonia;

public static class Operations
{
    // private static bool FirstLoad = true;
    private static string[] Cores = [];
    private const string CoreList = "avares://RetroLinkerLib/Assets/cores.txt";
    private const string DEFicon1 = "avares://RetroLinkerLib/Assets/Icons/retroarch.ico";
    private const string NaN = "avares://RetroLinkerLib/Assets/Images/NaN.png";
    
    // TODO: these two are set to many times on runtime...
    public static IStorageFolder? DesktopFolder { get; private set; }
    public static IStorageFolder? ROMTopDir { get; private set; }

    
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

    public static async Task<IStorageFolder?> GetStorageFolder(string dir, TopLevel topLevel) =>  
        await topLevel.StorageProvider.TryGetFolderFromPathAsync(dir);

    public static async void SetDesktopStorageFolder(TopLevel topLevel)
    {
        DesktopFolder = await GetStorageFolder(FileOps.UserDesktop, topLevel);
        var dbgOut = (DesktopFolder is null) 
            ? $"DesktopStorageFolder remained null. Attempted dir: \"{FileOps.UserDesktop}\"" 
            : $"DesktopStorageFolder set to: \"{DesktopFolder.Path.LocalPath}\"";
        App.Logger?.LogDebg(dbgOut);
    }
    
    public static async void SetROMTop(string? dir_ROMTop, TopLevel topLevel)
    {
        if (string.IsNullOrWhiteSpace(dir_ROMTop)) return;
        ROMTopDir = await GetStorageFolder(dir_ROMTop, topLevel);
        var dbgOut = (ROMTopDir is null)
            ? $"ROMPadreStorageFolder remained null. Attempted dir:\"{dir_ROMTop}\""
            : $"ROMPadreStorageFolder set to: \"{ROMTopDir.Path.LocalPath}\"";
        App.Logger?.LogDebg(dbgOut);
    }
}