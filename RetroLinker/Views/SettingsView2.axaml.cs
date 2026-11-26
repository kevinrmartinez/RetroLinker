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

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using RetroLinker.Models.Avalonia;
using AvaloniaTemplatedControl = Avalonia.Controls.Primitives.TemplatedControl;

namespace RetroLinker.Views;

public partial class SettingsView2 : UserControl
{
    // Window Obj
    // private MainWindow MainAppWindow;
    public SettingsWindow ParentWindow { get; }

    // PROPS/STATICS
    public bool DesktopOS => App.PlatformOs;

    public SettingsView2() {
        // Constructor for Designer
        InitializeComponent();
        ParentWindow = new SettingsWindow(true);
    }

    public SettingsView2(SettingsWindow settingsWindow) {
        InitializeComponent();
        ParentWindow = settingsWindow;

        AvaloniaTemplatedControl[] controlArray = [ txtUserAssetsDir, txtDefRAPath, txtParentRomDir, txtIconParentPath ];
        foreach (var control in controlArray)
            DragDropHandler.AddEventsToControl(control, ControlBox_HandleInput);
    }

    // GENERIC
    private void UpdateContext() {
        DataContext = null;
        DataContext = this;
    }

    void LockControls(bool locked) {
        gridConfigAll2.IsEnabled = !locked;
        UpdateContext();
    }

    // USER ASSETS
    void SetUserAssetsPath(string path) => ParentWindow.NewSettings.UserAssetsPath = path;

    async void btnUserAssets_ClickAsync(TextBox textBox)
    {
        try
        {
            LockControls(true);
            var currentFolder = (string.IsNullOrEmpty(textBox.Text)) ? string.Empty : textBox.Text;
            var folder = await FileDialogOps.OpenFolderAsync(OpenFolderOpts.UserAssets, ParentWindow, currentFolder);
            if (string.IsNullOrWhiteSpace(folder)) return;
            SetUserAssetsPath(folder);
        }
        catch (Exception e) { await this.PopUpGenericError(e); }
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
                    TextBoxActions.Clear => ParentWindow.GetDefSettings().UserAssetsPath,
                    _ => ParentWindow.NewSettings.UserAssetsPath
                };
                break;
        }
        UpdateContext();
    }

    // RA EXECUTABLE
    void SetDEFRADir(string path) => ParentWindow.NewSettings.DEFRADir = path;

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
            SetDEFRADir(file);
        }
        catch (Exception e) { await this.PopUpGenericError(e); }
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
    void SetDefROMPath(string path) => ParentWindow.NewSettings.DEFROMPath = path;

    async void btnDefROMPath_ClickAsync(TextBox textBox)
    {
        try
        {
            LockControls(true);
            string currentFolder = (string.IsNullOrEmpty(textBox.Text)) ? string.Empty : textBox.Text;
            string folder = await FileDialogOps.OpenFolderAsync(OpenFolderOpts.ROMParent, ParentWindow, currentFolder);
            if (string.IsNullOrWhiteSpace(folder)) return;
            SetDefROMPath(folder);
        }
        catch (Exception e) { await this.PopUpGenericError(e); }
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

    // IconParentPath
    void SetIconParentPath(string path) => ParentWindow.NewSettings.IconParentPath = path;

    async void btnIconParentPath_OnClickAsync(TextBox textBox)
    {
        try
        {
            LockControls(true);
            string currentFolder = (string.IsNullOrEmpty(textBox.Text)) ? string.Empty : textBox.Text;
            string folder = await FileDialogOps.OpenFolderAsync(OpenFolderOpts.IconParent, ParentWindow, currentFolder);
            if (string.IsNullOrWhiteSpace(folder)) return;
            SetIconParentPath(folder);
        }
        catch (Exception e) { await this.PopUpGenericError(e); }
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
                var newValue = action switch {
                    TextBoxActions.Restore => ParentWindow.GetOldSettings().IconParentPath,
                    TextBoxActions.Clear => string.Empty,
                    _ => ParentWindow.NewSettings.IconParentPath // Shouldn't happen
                };
                SetIconParentPath(newValue);
                break;
        }
        UpdateContext();
    }

    // Views Event
    private async Task ControlBox_HandleInput()
    {
        var result = true;
        var sender = DragDropHandler.ActiveControl;
        var droppedPath = DragDropHandler.ActiveArgument;
        if (droppedPath.PathType is FileSystemPathType.Folder)
        {
            if (sender.Equals(txtUserAssetsDir))
                SetUserAssetsPath(droppedPath.FullPath);
            else if (sender.Equals(txtParentRomDir))
                SetDefROMPath(droppedPath.FullPath);
            else if (sender.Equals(txtIconParentPath))
                SetIconParentPath(droppedPath.FullPath);
            else result = false;
        }
        else {
            if (sender.Equals(txtDefRAPath)) SetDEFRADir(droppedPath.FullPath);
            else result = false;
        }

        UpdateContext();
        Action resultReporter = result ? DragDropHandler.ParseSuccessful : DragDropHandler.ParseFailure;
        resultReporter();
    }

    private void TxtBox_OnLostFocus(object? sender, FocusChangedEventArgs e) => UpdateContext();
}