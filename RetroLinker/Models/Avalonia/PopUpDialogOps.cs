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
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.VisualTree;
using MsBox.Avalonia;
using MsBox.Avalonia.Base;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using RetroLinker.Translations;

namespace RetroLinker.Models.Avalonia;

public readonly struct PopUpGenericContent(string message, string? title = null, string? header = null) {
    public readonly string? Title = title;
    public readonly string? Header = header;
    public readonly string Message =  message;
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
    
    // App extension
    public static async Task PopUpCriticalError(this App app, System.Exception exception)
    {
        Logger.LogCrit($"{nameof(app)} has re-threw a critical exception:");
        Logger.LogCrit(exception);
        
        var applicationLifetime = app.ApplicationLifetime;
        if (applicationLifetime is not IClassicDesktopStyleApplicationLifetime { MainWindow: { } mainWindow }) throw exception;;
        var msBoxParams = new MessageBoxStandardParams()
        {
            Icon = Icon.Stop,
            ContentTitle = "Critical Error",
            ContentHeader = "Execution has stopped due to a critical error",
            ContentMessage = exception.Message,
            ButtonDefinitions = ButtonEnum.Ok,
            EnterDefaultButton = ClickEnum.Ok
        };
        await mainWindow.PopUpMessageBox<ButtonResult>(msBoxParams);
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
        var missingComponentEx = (exception is System.IO.FileNotFoundException fnf) && (fnf.Message.Contains("assembly", System.StringComparison.OrdinalIgnoreCase));
        if (!missingComponentEx) missingComponentEx = (exception is System.DllNotFoundException) || (exception.InnerException is System.DllNotFoundException);
        if (missingComponentEx)
        {
            if (Application.Current is not App app) throw exception;
            await app.ShowCriticalAndQuit(exception);
            return; // The app is dead at this point
        }
        Logger.LogErro(exception);
        var content = new PopUpGenericContent(exception.Message,  customTitle, customHeader);
        await window.PopUpGenericMessageBox(content, GenericPopUpType.Error);
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