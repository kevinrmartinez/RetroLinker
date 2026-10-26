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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using RetroLinker.Models.IO;
using RetroLinker.Translations;

namespace RetroLinker.Models.Avalonia;

public static class FileDialogOps
{
    public static async Task<string> OpenFileAsync(OpenOpts template, TopLevel topLevel, string? currentFile = null, string? newDialogTitle = null)
    {
        var opt = PickerOpt.OpenPickerOpt(template);
        opt.Title = newDialogTitle ?? opt.Title;
        if (!string.IsNullOrEmpty(currentFile)) {
            var currentDir = FileOps.GetDirFromPath(currentFile);
            if (!string.IsNullOrEmpty(currentDir)) 
                opt.SuggestedStartLocation = await Operations.GetStorageFolder(currentDir, topLevel);
        }
        var file = await topLevel.StorageProvider.OpenFilePickerAsync(opt);
        string dir = (file.Count > 0) ? FileOps.GetAbsolutePath(file[0].Path.LocalPath) : string.Empty;
        return dir;
    }

    public static async Task<string> OpenFileAsync(FilePickerOpenOptions openOptions, TopLevel topLevel) {
        var file = await topLevel.StorageProvider.OpenFilePickerAsync(openOptions);
        string dir = file.Count > 0 ? FileOps.GetAbsolutePath(file[0].Path.LocalPath) : string.Empty;
        return dir;
    }

    public static async Task<string> OpenFolderAsync(OpenFolderOpts template, TopLevel topLevel, string? currentFolder = null)
    {
        FolderPickerOpenOptions opt = new()
        {
            AllowMultiple = false,
            Title = template switch
            {
                OpenFolderOpts.UserAssets => resAvaloniaOps.dlgFolderUserAssets,
                OpenFolderOpts.ROMParent => resAvaloniaOps.dlgFolderROMParent,
                OpenFolderOpts.IconParent => resAvaloniaOps.dlgFolderIconParent,
                OpenFolderOpts.IcoOutput => resAvaloniaOps.dlgFolderIcoOutput,
                OpenFolderOpts.LinkCopy => resAvaloniaOps.dlgFolderLinkCopy,
                OpenFolderOpts.DefOutput => resAvaloniaOps.dlgFolderDefOutput,
                // Other
                _ => resAvaloniaOps.dlgFolderFallback
            },
        };
        if (!string.IsNullOrEmpty(currentFolder))
            opt.SuggestedStartLocation = await Operations.GetStorageFolder(currentFolder, topLevel);
        
        var dirList = await topLevel.StorageProvider.OpenFolderPickerAsync(opt);
        var dir = dirList.Count > 0 ? FileOps.GetAbsolutePath(dirList[0].Path.LocalPath) : string.Empty;
        return dir.TrimEnd(FileOps.OsDirSeparator);
    }

    public static async Task<string> SaveFileAsync(SaveOpts template, string currentFile, TopLevel topLevel)
    {
        var opt = PickerOpt.SavePickerOpt(template);
        if (!string.IsNullOrEmpty(currentFile))
        {
            currentFile = FileOps.GetDirFromPath(currentFile)!;
            opt.SuggestedStartLocation = await Operations.GetStorageFolder(currentFile, topLevel);
        }
        var file = await topLevel.StorageProvider.SaveFilePickerAsync(opt);
        string dir = (file != null) ? file.Path.LocalPath : string.Empty;   // Rewrite: replace all instances of this line to: file?.Path.LocalPath ?? string.Empty
        return dir;
    }
}