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
        BuildHeader();
    }

    public AppendView(MainWindow mainWindow, string filePath)
    {
        InitializeComponent();
        ParentWindow = mainWindow;
        AppendConfigFile = filePath;
        BuildHeader();
    }

    private void BuildHeader()
    {
        var header = new Styles.MainWindowHeader("Append Config");
        header.Classes.Add("Header");
        Grid.SetRow(header, 0);
        gridContent.Children.Add(header);
    }
    
    // Window Object
    private MainWindow ParentWindow;
    
    // FIELDS
    private string AppendConfigFile;
    private PickerOpt.OpenOpts PatchOpts = PickerOpt.OpenOpts.RAcfg;

    
    // View Buttons
    private void BtnSaveAppend_OnClick(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    private void BtnDiscAppend_OnClick(object? sender, RoutedEventArgs e) => ParentWindow.ReturnToMainView(this);
}