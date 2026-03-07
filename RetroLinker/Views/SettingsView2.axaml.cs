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

using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using RetroLinker.Models;
using RetroLinker.Models.Avalonia;

namespace RetroLinker.Views;

public partial class SettingsView2 : UserControl
{
    public SettingsView2()
    {
        // Constructor for Designer
        InitializeComponent();
        ParentWindow = new SettingsWindow(true);
    }
    
    public SettingsView2(SettingsWindow settingsWindow, bool desktopOs)
    {
        InitializeComponent();
        ParentWindow = settingsWindow;
        DesktopOS = desktopOs;
    }
    
    // Window Obj
    // private MainWindow MainAppWindow;
    private SettingsWindow ParentWindow;
    
    // PROPS/STATICS
    // private bool FirstTimeLoad = true;
    private bool DesktopOS;
    
    // LOAD
    void View_OnLoaded(object sender, RoutedEventArgs e)
    {
        // Settings
        ApplySettingsToControls();
    }
    
    void ApplySettingsToControls()
    { 
        // Absolute path
        txtUserAssets.Text = System.IO.Path.GetFullPath(ParentWindow.settings.UserAssetsPath);
        txtDefRADir.IsReadOnly = DesktopOS;
        txtDefRADir.Text = ParentWindow.settings.DEFRADir;
        btnApplyUserAssets.IsVisible = !DesktopOS;
        txtDefROMPath.Text = ParentWindow.settings.DEFROMPath;
    }
    
    void GenericAsyncErrorPopup(System.Exception error)
    {
        App.Logger?.LogErro(error);
        var stdParams = new MessageBoxStandardParams()
        {
            ContentHeader = Translations.resGeneric.popUnError_Head0, 
            ContentTitle = Translations.resGeneric.genError,
            ContentMessage = $"{Translations.resGeneric.popUnError_Mess0}\n{error.Message}",
            Icon = MsBox.Avalonia.Enums.Icon.Error,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            MaxWidth = 550
        };
        if (ParentWindow.Icon is { } icon) stdParams.WindowIcon = icon;
        var msBox = MessageBoxManager.GetMessageBoxStandard(stdParams);
        _ = msBox.ShowWindowDialogAsync(ParentWindow);
    }
    
    // USER ASSETS
    async void btnUserAssets_ClickAsync()
    {
        try {
            string currentFolder = (string.IsNullOrEmpty(txtUserAssets.Text)) ? string.Empty : txtUserAssets.Text;
            string folder = await FileDialogOps.OpenFolderAsync(template:0, currentFolder, ParentWindow);
            if (string.IsNullOrWhiteSpace(folder)) return;
            txtUserAssets.Text = folder; ParentWindow.settings.UserAssetsPath = folder;
        }
        catch (System.Exception e) { GenericAsyncErrorPopup(e); }
    }

    void btnUserAssets_OnClick(object sender, RoutedEventArgs e) => btnUserAssets_ClickAsync();
    
    void btnclrUserAssets_OnClick(object sender, RoutedEventArgs e) {
        ParentWindow.settings.UserAssetsPath = ParentWindow.DEFsettings.UserAssetsPath;
        txtUserAssets.Text = ParentWindow.settings.UserAssetsPath;
    }
    
    // RA EXECUTABLE
    private void BtnApplyUserAssets_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control control) return;
        if (control.Parent is not Grid grid) return;
        if (grid.Children[0] is not TextBox txtBox) return;
        if (!string.IsNullOrWhiteSpace(txtBox.Text)) ParentWindow.settings.DEFRADir = txtBox.Text;
    }

    async void btnDefRADir_ClickAsync()
    {
        try {
            var opt = DesktopOS ? PickerOpt.OpenOpts.RAexe : PickerOpt.OpenOpts.RAbin;
            string currentFile = ((string.IsNullOrEmpty(txtDefRADir.Text)) || !DesktopOS) ? string.Empty : txtDefRADir.Text;
            string file = await FileDialogOps.OpenFileAsync(opt, ParentWindow, currentFile);
            if (string.IsNullOrWhiteSpace(file)) return;
            txtDefRADir.Text = file; 
            ParentWindow.settings.DEFRADir = file;
        }
        catch (System.Exception e) { GenericAsyncErrorPopup(e); }
    }

    void btnDefRADir_OnClick(object sender, RoutedEventArgs e) => btnDefRADir_ClickAsync();
    
    void btnclrDefRADir_Click(object sender, RoutedEventArgs e) {
        ParentWindow.settings.DEFRADir = (DesktopOS) ? ParentWindow.DEFsettings.DEFRADir : FileOps.LinuxRABin;
        txtDefRADir.Text = ParentWindow.settings.DEFRADir;
    }
    
    // DEFAULT ROM PATH
    async void btnDefROMPath_ClickAsync()
    {
        try {
            string currentFolder = (string.IsNullOrEmpty(txtDefROMPath.Text)) ? string.Empty : txtDefROMPath.Text;
            string folder = await FileDialogOps.OpenFolderAsync(template:1, currentFolder, ParentWindow);
            if (string.IsNullOrWhiteSpace(folder)) return;
            txtDefROMPath.Text = folder; 
            ParentWindow.settings.DEFROMPath = folder;
        }
        catch (System.Exception e) { GenericAsyncErrorPopup(e); }
    }

    void btnDefROMPath_OnClick(object sender, RoutedEventArgs e) => btnDefROMPath_ClickAsync();
    
    void btnclrDefROMPath_Click(object sender, RoutedEventArgs e)
    {
        ParentWindow.settings.DEFROMPath = ParentWindow.DEFsettings.DEFROMPath;
        txtDefROMPath.Text = ParentWindow.settings.DEFROMPath;
    }
}