/*
    A .NET GUI application to help create desktop links of games running on RetroArch.
    Copyright (C) 2025  Kevin Rafael Martinez Johnston

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
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using RetroLinker.Translations;

namespace RetroLinker.Models.Avalonia;

public static class FileDialogOps
{
    public static async Task<string> OpenFileAsync(PickerOpt.OpenOpts template, TopLevel topLevel, string? currentFile = null)
    {
        var opt = PickerOpt.OpenPickerOpt(template);
        if (!string.IsNullOrEmpty(currentFile))
        {
            currentFile = FileOps.GetDirFromPath(currentFile)!;
            opt.SuggestedStartLocation = await Operations.GetStorageFolder(currentFile, topLevel);
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
            opt.SuggestedStartLocation = await Operations.GetStorageFolder(currentFolder, topLevel);
        
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
            opt.SuggestedStartLocation = await Operations.GetStorageFolder(currentFile, topLevel);
        }
        var file = await topLevel.StorageProvider.SaveFilePickerAsync(opt);
        string dir = (file != null) ? file.Path.LocalPath : string.Empty;
        return dir;
    }
}