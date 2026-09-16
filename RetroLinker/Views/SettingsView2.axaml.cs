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

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using RetroLinker.Models;
using RetroLinker.Models.Avalonia;

namespace RetroLinker.Views;

public partial class SettingsView2 : UserControl
{
    // Window Obj
    // private MainWindow MainAppWindow;
    public SettingsWindow ParentWindow { get; }
    
    // PROPS/STATICS
    public bool DesktopOS { get; }

    public SettingsView2() {
        // Constructor for Designer
        InitializeComponent();
        ParentWindow = new SettingsWindow(true);
    }
    
    public SettingsView2(SettingsWindow settingsWindow, bool desktopOs) {
        InitializeComponent();
        ParentWindow = settingsWindow;
        DesktopOS = desktopOs;
    }
    
    // GENERIC
    private void UpdateContext() {
        DataContext = null;
        DataContext = this;
    }
    
    private void TxtBox_OnLostFocus(object? sender, FocusChangedEventArgs e) => UpdateContext();
    
    // USER ASSETS
    void LockControls(bool locked) {
        gridConfigAll2.IsEnabled = !locked;
        UpdateContext();
    }
    
    async void btnUserAssets_ClickAsync(TextBox textBox)
    {
        try
        {
            LockControls(true);
            var currentFolder = (string.IsNullOrEmpty(textBox.Text)) ? string.Empty : textBox.Text;
            var folder = await FileDialogOps.OpenFolderAsync(OpenFolderOpts.UserAssets, ParentWindow, currentFolder);
            if (string.IsNullOrWhiteSpace(folder)) return;
            ParentWindow.NewSettings.UserAssetsPath = folder;
        }
        catch (System.Exception e) { _ = this.PopUpGenericError(e); }
        finally { LockControls(false); }
    }

    void btnUserAssets_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        switch (button.CommandParameter)
        {
            case TextBox textBox:
                btnUserAssets_ClickAsync(textBox);
                break;
            case TextBoxActions action:
                ParentWindow.NewSettings.UserAssetsPath = action switch {
                    TextBoxActions.Restore => ParentWindow.GetOldSettings().UserAssetsPath,
                    _ => ParentWindow.NewSettings.UserAssetsPath
                };
                break;
        }
        UpdateContext();
    }
    
    // RA EXECUTABLE
    async void btnDefRADir_ClickAsync(TextBox textBox)
    {
        try 
        {
            LockControls(true);
            var opt = DesktopOS ? OpenOpts.WinExe : OpenOpts.LinBin;
            var dlgTitle = Translations.resAvaloniaOps.dlgFileRAexe;
            var currentFile = ((string.IsNullOrEmpty(textBox.Text)) || !DesktopOS) ? string.Empty : textBox.Text;
            var file = await FileDialogOps.OpenFileAsync(opt, ParentWindow, currentFile, dlgTitle);
            if (string.IsNullOrWhiteSpace(file)) return;
            ParentWindow.NewSettings.DEFRADir = file;
        }
        catch (System.Exception e) { _ = this.PopUpGenericError(e); }
        finally { LockControls(false); }
    }

    void btnDefRADir_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        switch (button.CommandParameter)
        {
            case TextBox textBox:
                btnDefRADir_ClickAsync(textBox);
                break;
            case TextBoxActions action:
                ParentWindow.NewSettings.DEFRADir = action switch {
                    TextBoxActions.Restore => ParentWindow.GetOldSettings().DEFRADir,
                    TextBoxActions.Clear => string.Empty,
                    _ => ParentWindow.NewSettings.DEFRADir
                };
                break;
        }
        UpdateContext();
    }
    
    // DEFAULT ROM PATH
    async void btnDefROMPath_ClickAsync(TextBox textBox)
    {
        try 
        {
            LockControls(true);
            string currentFolder = (string.IsNullOrEmpty(textBox.Text)) ? string.Empty : textBox.Text;
            string folder = await FileDialogOps.OpenFolderAsync(OpenFolderOpts.ROMParent, ParentWindow, currentFolder);
            if (string.IsNullOrWhiteSpace(folder)) return;
            ParentWindow.NewSettings.DEFROMPath = folder;
        }
        catch (System.Exception e) { _ = this.PopUpGenericError(e); }
        finally { LockControls(false); }
    }

    void btnDefROMPath_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        switch (button.CommandParameter)
        {
            case TextBox textBox:
                btnDefROMPath_ClickAsync(textBox);
                break;
            case TextBoxActions action:
                ParentWindow.NewSettings.DEFROMPath = action switch {
                    TextBoxActions.Restore => ParentWindow.GetOldSettings().DEFROMPath,
                    TextBoxActions.Clear => string.Empty,
                    _ => ParentWindow.NewSettings.DEFROMPath // Shouldn't happen
                };
                break;
        }
        UpdateContext();
    }

    async void btnIconParentPath_OnClickAsync(TextBox textBox)
    {
        try
        {
            LockControls(true);
            string currentFolder = (string.IsNullOrEmpty(textBox.Text)) ? string.Empty : textBox.Text;
            string folder = await FileDialogOps.OpenFolderAsync(OpenFolderOpts.IconParent, ParentWindow, currentFolder);
            if (string.IsNullOrWhiteSpace(folder)) return;
            ParentWindow.NewSettings.IconParentPath = folder;
        }
        catch (System.Exception e) { _ = this.PopUpGenericError(e); }
        finally { LockControls(false); }
    }

    private void btnIconParentPath_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        switch (button.CommandParameter)
        {
            case TextBox textBox:
                btnIconParentPath_OnClickAsync(textBox);
                break;
            case TextBoxActions action:
                ParentWindow.NewSettings.IconParentPath = action switch {
                    TextBoxActions.Restore => ParentWindow.GetOldSettings().IconParentPath,
                    TextBoxActions.Clear => string.Empty,
                    _ => ParentWindow.NewSettings.IconParentPath // Shouldn't happen
                };
                break;
        }
        UpdateContext();
    }
}