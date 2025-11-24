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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RetroLinker.Models;
using RetroLinker.Models.Avalonia;

namespace RetroLinker.Views;

public partial class AppendView : UserControl
{
    // Designer Constructor
    public AppendView()
    {
        InitializeComponent();
        ParentWindow = new MainWindow(true);
        List<string> appendConfigFiles = FillListTest();
        AppendPaths = new ObservableCollection<string>(appendConfigFiles);
        ItemsControlPaths.ItemsSource = AppendPaths;
        
    }
    
    // Active Constructor
    public AppendView(MainWindow mainWindow, string appendArg)
    {
        InitializeComponent();
        ParentWindow = mainWindow;
        List<string> appendConfigFiles;
        try {
            appendConfigFiles = Commander.ResolveAppendConfigArg(appendArg).Item2;
        }
        catch (System.ArgumentException argumentException) {
            System.Diagnostics.Trace.WriteLine(argumentException.Message);
            appendConfigFiles = new();
        }

        AppendPaths = new ObservableCollection<string>(appendConfigFiles);
        ItemsControlPaths.ItemsSource = AppendPaths;
    }
    
    // Window Object
    private MainWindow ParentWindow;
    
    // Props
    public ObservableCollection<string> AppendPaths { get; private set; }
    
    // FIELDS
    private PickerOpt.OpenOpts ConfigOpt = PickerOpt.OpenOpts.RAcfg;

    // Append Config controls
    private void ButtonTrash_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        if (button.Parent!.Parent!.Parent is ContentPresenter { Content: string content })
            AppendPaths.Remove(content);
    }
    
    private async void BtnConfigPathBrowse_OnClick(object? sender, RoutedEventArgs e)
    {
        var loadedFile = await FileDialogOps.OpenFileAsync(ConfigOpt, ParentWindow);
        if (string.IsNullOrWhiteSpace(loadedFile)) return;
        if (!AppendPaths.Contains(loadedFile))
            AppendPaths.Add(loadedFile);
    }

    private void BtnConfigPathClear_OnClick(object? sender, RoutedEventArgs e) => AppendPaths.Clear();
    
    
    // View Buttons
    private void BtnSaveAppend_OnClick(object? sender, RoutedEventArgs e)
    {
        var appendArg = (AppendPaths.Count == 0) ? string.Empty : Commander.GetAppendConfigArg(new List<string>(AppendPaths));
        ParentWindow.ReturnToMainView(this, appendArg);
    }

    private void BtnDiscAppend_OnClick(object? sender, RoutedEventArgs e) => ParentWindow.ReturnToMainView();

    private List<string> FillListTest()
    {
        var _appendPaths = new List<string>();
        for (int i = 0; i < 16; i++) {
            _appendPaths.Add($"/home/public/testing/retroarch{i}.cfg");
        }
        return _appendPaths;
    }
}