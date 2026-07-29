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

    public SettingsView2()
    {
        // Constructor for Designer
        InitializeComponent();
        TextBox[] textBoxes = [txtUserAssets, txtDefRADir, txtDefROMPath];
        foreach (var textBox in textBoxes) {
            textBox.GotFocus += TxtBox_OnGotFocus;
            textBox.LostFocus += TxtBox_OnLostFocus;
        }
        
        ParentWindow = new SettingsWindow(true);
        DataContext = this;
    }
    
    public SettingsView2(SettingsWindow settingsWindow, bool desktopOs) : this() {
        ParentWindow = settingsWindow;
        DesktopOS = desktopOs;
        // DataContext = this;
    }
    
    // GENERIC
    private void TxtBox_OnGotFocus(object? sender, FocusChangedEventArgs e) => ParentWindow.BindTimer?.Stop();
    private void TxtBox_OnLostFocus(object? sender, FocusChangedEventArgs e) {
        ParentWindow.UpdateContextFromOutside();
        ParentWindow.BindTimer?.Start();
    }
    
    // USER ASSETS
    void LockControls(bool locked) => gridConfigAll2.IsEnabled = !locked;
    
    async void btnUserAssets_ClickAsync()
    {
        try
        {
            LockControls(true);
            string currentFolder = (string.IsNullOrEmpty(txtUserAssets.Text)) ? string.Empty : txtUserAssets.Text;
            string folder = await FileDialogOps.OpenFolderAsync(template: 0, currentFolder, ParentWindow);
            if (string.IsNullOrWhiteSpace(folder)) return;
            // txtUserAssets.Text = folder;
            ParentWindow.settings.UserAssetsPath = folder;
        }
        catch (System.Exception e) { _ = this.PopUpGenericError(e); }
        finally { LockControls(false); }
    }

    void btnUserAssets_OnClick(object sender, RoutedEventArgs e) => btnUserAssets_ClickAsync();
    
    void btnclrUserAssets_OnClick(object sender, RoutedEventArgs e) {
        ParentWindow.settings.UserAssetsPath = ParentWindow.DEFsettings.UserAssetsPath;
        // txtUserAssets.Text = ParentWindow.settings.UserAssetsPath;
    }
    
    // RA EXECUTABLE
    async void btnDefRADir_ClickAsync()
    {
        try {
            LockControls(true);
            var opt = DesktopOS ? OpenOpts.RAexe : OpenOpts.RAbin;
            string currentFile = ((string.IsNullOrEmpty(txtDefRADir.Text)) || !DesktopOS) ? string.Empty : txtDefRADir.Text;
            string file = await FileDialogOps.OpenFileAsync(opt, ParentWindow, currentFile);
            if (string.IsNullOrWhiteSpace(file)) return;
            // txtDefRADir.Text = file;
            ParentWindow.settings.DEFRADir = file;
        }
        catch (System.Exception e) { _ = this.PopUpGenericError(e); }
        finally { LockControls(false); }
    }

    void btnDefRADir_OnClick(object sender, RoutedEventArgs e) => btnDefRADir_ClickAsync();
    
    void btnclrDefRADir_Click(object sender, RoutedEventArgs e) {
        ParentWindow.settings.DEFRADir = (DesktopOS) ? ParentWindow.DEFsettings.DEFRADir : FileOps.LinuxRABin;
        // txtDefRADir.Text = ParentWindow.settings.DEFRADir;
    }
    
    // DEFAULT ROM PATH
    async void btnDefROMPath_ClickAsync()
    {
        try {
            LockControls(true);
            string currentFolder = (string.IsNullOrEmpty(txtDefROMPath.Text)) ? string.Empty : txtDefROMPath.Text;
            string folder = await FileDialogOps.OpenFolderAsync(OpenFolderOpts.UserAssets, currentFolder, ParentWindow);
            if (string.IsNullOrWhiteSpace(folder)) return;
            // txtDefROMPath.Text = folder;
            ParentWindow.settings.DEFROMPath = folder;
        }
        catch (System.Exception e) { _ = this.PopUpGenericError(e); }
        finally { LockControls(false); }
    }

    void btnDefROMPath_OnClick(object sender, RoutedEventArgs e) => btnDefROMPath_ClickAsync();
    
    void btnclrDefROMPath_Click(object sender, RoutedEventArgs e) {
        ParentWindow.settings.DEFROMPath = ParentWindow.DEFsettings.DEFROMPath;
        // txtDefROMPath.Text = ParentWindow.settings.DEFROMPath;
    }
}