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
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RetroLinker.Models.Avalonia;
using RetroLinker.Models.DesktopLink;
using RetroLinker.Models.IO;

namespace RetroLinker.Views;

public partial class AppendView : UserControl
{
    // Designer Constructor
    public AppendView()
    {
        InitializeComponent();
        _parentWindow = new MainWindow(true);
        AppendPaths = FillListTest();
        ItemsControlPaths.Items = AppendPaths;
        DataContext =  this;
    }
    
    // Active Constructor
    public AppendView(MainWindow mainWindow)
    {
        InitializeComponent();
        // TODO: When 'PrevConfig' == true, also store a list of appended files 
        _parentWindow = mainWindow;
        SelectedConfigFile = _parentWindow.PermaView?.comboConfig.SelectedItem?.ToString();
        var tryObservablePaths = _parentWindow.PermaView?.BuildingLink.RaAppendedConfig as ObservableAppendedConfigs;
        if (tryObservablePaths is null) {
            Logger.LogWarn($"[{nameof(AppendView)}] Failed to obtain paths from RaAppendedConfig");
            tryObservablePaths = new ObservableAppendedConfigs();
        }
        AppendPaths = tryObservablePaths;
        DataContext =  this;
    }
    
    // Window Object
    private readonly MainWindow _parentWindow;
    
    // Props
    public string? SelectedConfigFile { get; }
    public ObservableAppendedConfigs AppendPaths { get; }
    
    // FIELDS
    private const OpenOpts ConfigOpt = OpenOpts.RAcfg;

    // Append Config controls

    private void LockControls(bool locked) => gridContent.IsEnabled = !locked;
    
    private async void BtnConfigPathBrowse_ClickAsync()
    {
        try 
        {
            LockControls(true);
            var loadedFile = await FileDialogOps.OpenFileAsync(ConfigOpt, _parentWindow);
            if (string.IsNullOrWhiteSpace(loadedFile)) return;
            if (!AppendPaths.Contains(loadedFile))
                AppendPaths.Add(loadedFile);
        }
        catch (System.Exception e) { _ = this.PopUpGenericError(e); }
        finally { LockControls(false); }
    }

    private void BtnConfigPathBrowse_OnClick(object? sender, RoutedEventArgs e) => BtnConfigPathBrowse_ClickAsync();
    
    
    // View Buttons
    private void BtnSaveAppend_OnClick(object? sender, RoutedEventArgs e) {
        var appendArg = (AppendPaths.Count == 0) 
            ? null 
            : AppendPaths;
        _parentWindow.ReturnToMainView(this, appendArg);
    }

    private void BtnDiscAppend_OnClick(object? sender, RoutedEventArgs e) => _parentWindow.ReturnToMainView();

    private ObservableAppendedConfigs FillListTest()
    {
        var appendPaths = new ObservableAppendedConfigs();
        var pfx = FileOps.CombineMultipleInputs(FileOps.UserDesktop, "testing");
        for (int i = 0; i < 16; i++) {
            var testPath = FileOps.CombineMultipleInputs(pfx, $"retroarch{i}.cfg");
            appendPaths.Add(testPath);
        }
        return appendPaths;
    }
}