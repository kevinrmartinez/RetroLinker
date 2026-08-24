/*
    RetroLinker: A .NET GUI application to help create desktop links of games running on RetroArch.
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
using Avalonia.VisualTree;
using MsBox.Avalonia;
using MsBox.Avalonia.Base;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
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
        string dir = (file.Count > 0) ? Path.GetFullPath(file[0].Path.LocalPath) : string.Empty;
        return dir;
    }

    public static async Task<string> OpenFileAsync(FilePickerOpenOptions openOptions, TopLevel topLevel) {
        var file = await topLevel.StorageProvider.OpenFilePickerAsync(openOptions);
        string dir = file.Count > 0 ? Path.GetFullPath(file[0].Path.LocalPath) : string.Empty;
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
                OpenFolderOpts.IcoOutput => resAvaloniaOps.dlgFolderIcoOutput,
                OpenFolderOpts.LinkCopy => resAvaloniaOps.dlgFolderLinkCopy,
                OpenFolderOpts.DefOutput => resAvaloniaOps.dlgFolderDefOutput,
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


public static class PopUpDialogOps
{
    private static Window GetParentWindow(UserControl view) {
        var window = view.FindAncestorOfType<Window>();
        return window ?? throw new System.ArgumentNullException(nameof(view), @"Could not get the view's owner Window.");
    }

    private static void GetContentFromStruct(this AbstractMessageBoxParams @params, PopUpGenericContent content, GenericPopUpType type)
    {
        (string bakTitle, string bakHeader) = type switch
        {
            GenericPopUpType.Error => (resGeneric.genError, resGeneric.popUnError_Head0),
            GenericPopUpType.Warning => (resGeneric.genWarning, ""),
            GenericPopUpType.Question => ("Are you sure?", ""),
            GenericPopUpType.Success => (resGeneric.genSucces, "Operation completed successfully!"),
            _ => ("Info", "")
        };
        @params.ContentMessage = content.Message;
        @params.ContentTitle = content.Title ?? bakTitle;
        @params.ContentHeader = content.Header ?? bakHeader;
    }

    private static async Task<T> CallMessageBox<T>(Window msboxOwner, AbstractMessageBoxParams someParams)
    {
        var typeT = typeof(T); 
        var typeBr = typeof(ButtonResult);
        var typeStr = typeof(string);
        if (typeT != typeBr && typeT != typeStr) 
            throw new System.InvalidOperationException($"Invalid Type was used: {typeT.Name}. Only the types '{typeBr.Name}' and '{typeStr.Name}' are supported.");
        
        if (msboxOwner.Icon is {} wIcon) someParams.WindowIcon = wIcon;
        var msBox = someParams switch {
            MessageBoxStandardParams stdParams => (IMsBox<T>)MessageBoxManager.GetMessageBoxStandard(stdParams),
            MessageBoxCustomParams cusParams => (IMsBox<T>)MessageBoxManager.GetMessageBoxCustom(cusParams),
            _ => throw new System.ArgumentException(@"Can't build pop-up; unknown parameters", nameof(someParams))
        };
        return await msBox.ShowWindowDialogAsync(msboxOwner);
    }
    
    // Window extensions
    public static async Task<T> PopUpMessageBox<T>(this Window window, AbstractMessageBoxParams someParams) {
        return await CallMessageBox<T>(window, someParams);
    }
    
    public static async Task<ButtonResult> PopUpGenericMessageBox(this Window window, PopUpGenericContent content, GenericPopUpType type)
    {
        var msBoxParams = GetCommonStdParams();
        msBoxParams.GetContentFromStruct(content, type);
        msBoxParams.Icon = type switch
        {
            GenericPopUpType.Error => Icon.Error,
            GenericPopUpType.Warning => Icon.Warning,
            GenericPopUpType.Question => Icon.Question,
            GenericPopUpType.Success => Icon.Success,
            _ => Icon.Info
        };
        msBoxParams.ButtonDefinitions = type switch {
            GenericPopUpType.Question => ButtonEnum.YesNo,
            _ => ButtonEnum.Ok
        };
        return await CallMessageBox<ButtonResult>(window, msBoxParams);
    }
    
    public static async Task PopUpGenericError(this Window window, System.Exception exception, string? customTitle = null,  string? customHeader = null)
    {
        Logger.LogErro(exception);
        var content = new PopUpGenericContent(exception.Message,  customTitle, customHeader);
        _ = await window.PopUpGenericMessageBox(content, GenericPopUpType.Error);
    }
    
    // UserControl extensions
    public static async Task<T> PopUpMessageBox<T>(this UserControl view, AbstractMessageBoxParams someParams) {
        var window = GetParentWindow(view);
        return await window.PopUpMessageBox<T>(someParams);
        // return await CallMessageBox<T>(window, someParams);
    }

    public static async Task<ButtonResult> PopUpGenericMessageBox(this UserControl view, PopUpGenericContent content, GenericPopUpType type) {
        var window = GetParentWindow(view);
        return await window.PopUpGenericMessageBox(content, type);
    }

    public static async Task PopUpGenericError(this UserControl view, System.Exception exception, string? customTitle = null,  string? customHeader = null) {
        var window = GetParentWindow(view);
        await window.PopUpGenericError(exception, customTitle, customHeader);
    }

    public static MessageBoxStandardParams GetCommonStdParams()
    {
        return new MessageBoxStandardParams() {
            CanResize = false,
            MaxWidth = 600,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            
        };
    }
}

public readonly struct PopUpGenericContent(string message, string? title = null, string? header = null) {
    public readonly string? Title = title;
    public readonly string? Header = header;
    public readonly string Message =  message;
}

public enum GenericPopUpType {
    Error, Warning, Question, Info, Success
}