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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using RetroarchShortcutterV2.Models;

namespace RetroarchShortcutterV2.Views;

public partial class SettingsView2 : UserControl
{
    public SettingsView2()
    { InitializeComponent(); }
    
    public SettingsView2(MainWindow mainWindow, SettingsWindow settingsWindow, bool desktopOs)
    {
        InitializeComponent();
        MainAppWindow = mainWindow;
        ParentWindow = settingsWindow;
        DesktopOS = desktopOs;
    }
    
    // Window Obj
    private MainWindow MainAppWindow;
    private SettingsWindow ParentWindow;
    
    // PROPS/STATICS
    private bool FirstTimeLoad = true;
    private bool DesktopOS;
    
    // LOAD
    void SettingsView2_1_Loaded(object sender, RoutedEventArgs e)
    {
        
        // Settings
        ApplySettingsToControls();
    }
    
    void ApplySettingsToControls()
    { 
        // TODO: Decidirse si usar directorio relativo o absoluto
        txtUserAssets.Text = System.IO.Path.GetFullPath(ParentWindow.settings.UserAssetsPath);
        txtDefRADir.IsReadOnly = DesktopOS;
        txtDefRADir.Text = ParentWindow.settings.DEFRADir;
        btnApplyUserAssets.IsVisible = !DesktopOS;
        txtDefROMPath.Text = ParentWindow.settings.DEFROMPath;
    }
    
    // USER ASSETS
    async void btnUserAssets_Click(object sender, RoutedEventArgs e)
    {
        string folder = await FileOps.OpenFolderAsync(template:0, ParentWindow);
        if (!string.IsNullOrWhiteSpace(folder))
        { txtUserAssets.Text = folder; ParentWindow.settings.UserAssetsPath = folder; }
    }
    
    void btnclrUserAssets_Click(object sender, RoutedEventArgs e)
    {
        ParentWindow.settings.UserAssetsPath = ParentWindow.DEFsettings.UserAssetsPath;
        txtUserAssets.Text = ParentWindow.settings.UserAssetsPath;
        
    }
    
    // EJECUTABLE
    private void BtnApplyUserAssets_Click(object? sender, RoutedEventArgs e)
    {
        var _grid = (sender as Control).Parent as Grid;
        var txtbox = _grid.Children[0] as TextBox;
        if (!string.IsNullOrWhiteSpace(txtbox.Text)) ParentWindow.settings.DEFRADir = txtbox.Text;
    }
    
    async void btnDefRADir_Click(object sender, RoutedEventArgs e)
    {
        PickerOpt.OpenOpts opt;
        opt = DesktopOS ? PickerOpt.OpenOpts.RAexe :    // FilePicker Option para .exe de Windows
                          PickerOpt.OpenOpts.RAout;     // FilePicker Option para .AppImage de Linux
        string file = await FileOps.OpenFileAsync(template:opt, ParentWindow);
        if (!string.IsNullOrWhiteSpace(file))
        { 
            txtDefRADir.Text = file; 
            ParentWindow.settings.DEFRADir = file; 
        }
    }
    
    void btnclrDefRADir_Click(object sender, RoutedEventArgs e)
    {
        ParentWindow.settings.DEFRADir = (DesktopOS) ? ParentWindow.DEFsettings.DEFRADir : FileOps.LinuxRABin;
        txtDefRADir.Text = ParentWindow.settings.DEFRADir;
        
    }
    
    // DIR PADRE
    async void btnDefROMPath_Click(object sender, RoutedEventArgs e)
    {
        string folder = await FileOps.OpenFolderAsync(template:1, ParentWindow);
        if (!string.IsNullOrWhiteSpace(folder))
        { 
            txtDefROMPath.Text = folder; 
            ParentWindow.settings.DEFROMPath = folder; 
        }
    }   // TODO: No parece aplicarce en ningun Linux
    
    void btnclrDefROMPath_Click(object sender, RoutedEventArgs e)
    {
        ParentWindow.settings.DEFROMPath = ParentWindow.DEFsettings.DEFROMPath;
        txtDefROMPath.Text = ParentWindow.settings.DEFROMPath;
    }

    //UNLOAD
    private void SettingsView2_1_OnUnloaded(object? sender, RoutedEventArgs e)
    {
        _ = e.Source;
    }
    
#if DEBUG
    void SettingsView2_1_Loaded2(object sender, RoutedEventArgs e)
    {
        _ = sender.ToString();
    }
#endif
}