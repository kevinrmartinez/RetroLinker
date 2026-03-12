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
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
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
        ItemsControlPaths.Items = AppendPaths;
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
        catch (System.ArgumentException ex) {
            System.Diagnostics.Trace.WriteLine(ex.Message);
            appendConfigFiles = new();
        }

        AppendPaths = new(appendConfigFiles);
        ItemsControlPaths.Items = AppendPaths;
    }
    
    // Window Object
    private MainWindow ParentWindow;
    
    // Props
    public ObservableCollection<string> AppendPaths { get; private set; }
    
    // FIELDS
    private PickerOpt.OpenOpts ConfigOpt = PickerOpt.OpenOpts.RAcfg;

    // Append Config controls

    private async void BtnConfigPathBrowse_ClickAsync()
    {
        try {
            var loadedFile = await FileDialogOps.OpenFileAsync(ConfigOpt, ParentWindow);
            if (string.IsNullOrWhiteSpace(loadedFile)) return;
            if (!AppendPaths.Contains(loadedFile))
                AppendPaths.Add(loadedFile);
        }
        catch (System.Exception e)
        {
            App.Logger?.LogErro(e);
            MessageBoxStandardParams mbParams = new()
            {
                ContentTitle = Translations.resGeneric.genError,
                ContentHeader = Translations.resGeneric.popUnError_Head0,
                ContentMessage = $"{Translations.resGeneric.popUnError_Head0}\n{e.Message}",
                Icon = MsBox.Avalonia.Enums.Icon.Error,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                MaxWidth = 550
            };
            if (ParentWindow.Icon is { } icon) mbParams.WindowIcon = icon;
            var msBox = MessageBoxManager.GetMessageBoxStandard(mbParams);
            _ = msBox.ShowWindowDialogAsync(ParentWindow);
        }
    }

    private void BtnConfigPathBrowse_OnClick(object? sender, RoutedEventArgs e) => BtnConfigPathBrowse_ClickAsync();
    
    
    // View Buttons
    private void BtnSaveAppend_OnClick(object? sender, RoutedEventArgs e) {
        var appendArg = (AppendPaths.Count == 0) 
            ? string.Empty 
            : Commander.GetAppendConfigArg(new List<string>(AppendPaths));
        ParentWindow.ReturnToMainView(this, appendArg);
    }

    private void BtnDiscAppend_OnClick(object? sender, RoutedEventArgs e) => ParentWindow.ReturnToMainView();

    private List<string> FillListTest()
    {
        var appendPaths = new List<string>();
        var pfx = FileOps.CombineMultipleInputs(FileOps.UserDesktop, "testing");
        for (int i = 0; i < 16; i++) {
            var testPath = FileOps.CombineMultipleInputs(pfx, $"retroarch{i}.cfg");
            appendPaths.Add(testPath);
        }
        return appendPaths;
    }
}