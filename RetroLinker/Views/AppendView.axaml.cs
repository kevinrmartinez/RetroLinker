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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RetroLinker.Models;

namespace RetroLinker.Views;

public partial class AppendView : UserControl
{
    // Designer Constructor
    public AppendView()
    {
        InitializeComponent();
        ParentWindow = new MainWindow(true);
        AppendConfigFile = "debug.txt";
    }
    
    // Active Constructor
    public AppendView(MainWindow mainWindow, string appendArg)
    {
        InitializeComponent();
        ParentWindow = mainWindow;
        var file = string.Empty;
        try {
            file = Commander.ResolveAppendConfigArg(appendArg);
        }
        catch (System.ArgumentException argumentException) {
            System.Console.WriteLine(argumentException);
            // TODO
        }
        AppendConfigFile = string.IsNullOrWhiteSpace(file) ? string.Empty : file;
        txtConfigPath.Text = AppendConfigFile;
    }
    
    // Window Object
    private MainWindow ParentWindow;
    
    // FIELDS
    private string AppendConfigFile;
    private PickerOpt.OpenOpts ConfigOpt = PickerOpt.OpenOpts.RAcfg;

    // Append Config controls
    private async void BtnConfigPathBrowse_OnClick(object? sender, RoutedEventArgs e)
    {
        var loadedFile = await AvaloniaOps.OpenFileAsync(ConfigOpt, AppendConfigFile, ParentWindow);
        if (string.IsNullOrWhiteSpace(loadedFile)) return;
        txtConfigPath.Text = loadedFile;
        AppendConfigFile  = loadedFile;
    }

    private void BtnConfigPathClear_OnClick(object? sender, RoutedEventArgs e) {
        txtConfigPath.Clear();
        AppendConfigFile = string.Empty;
    }
    
    // View Buttons
    private void BtnSaveAppend_OnClick(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(AppendConfigFile)) AppendConfigFile = Commander.GetAppendConfigArg(AppendConfigFile);
        ParentWindow.ReturnToMainView(this, AppendConfigFile);
    }

    private void BtnDiscAppend_OnClick(object? sender, RoutedEventArgs e) => ParentWindow.ReturnToMainView(this);
    
}