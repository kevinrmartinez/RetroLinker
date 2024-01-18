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
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using RetroLinker.Translations;

namespace RetroLinker.Models;

public static class AvaloniaOps
{
    private const string DEFicon1 = "avares://RetroLinkerLib/Assets/Icons/retroarch.ico";
    private const string NoAplica = "avares://RetroLinkerLib/Assets/Images/no-aplica.png";
    
    public static IStorageFolder? DesktopFolder { get; private set; }
    public static IStorageFolder? ROMPadreDir { get; private set; }

    
    #region FUNCTIONS
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
                0 => resAvaloniaOps.dlgFolderUserAssets,
                1 => resAvaloniaOps.dlgFolderROMParent,
                2 => resAvaloniaOps.dlgFolderIcoOutput,
                3 => resAvaloniaOps.dlgFolderLinkCopy,
                // Esta opcion no deberia pasar
                _ => resAvaloniaOps.dlgFolderFallback
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
}