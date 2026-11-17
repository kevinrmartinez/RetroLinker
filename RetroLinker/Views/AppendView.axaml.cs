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

namespace RetroLinker.Views;

public partial class AppendView : UserControl
{
    #region AvaloniaProperties

    public static readonly DirectProperty<AppendView, string?> SelectedConfigFileProperty =
        AvaloniaProperty.RegisterDirect<AppendView, string?>(
            nameof(SelectedConfigFile),
            view => view.SelectedConfigFile);

    public string? SelectedConfigFile { get; init => SetAndRaise(SelectedConfigFileProperty, ref field, value); }

    public static readonly DirectProperty<AppendView, ObservableAppendedConfigList> AppendPathsProperty =
        AvaloniaProperty.RegisterDirect<AppendView, ObservableAppendedConfigList>(
            nameof(AppendPaths),
            view => view.AppendPaths);

    public ObservableAppendedConfigList AppendPaths { get; init =>  SetAndRaise(AppendPathsProperty, ref field, value); }

    public static readonly DirectProperty<AppendView, string?> StoredConfigSelectedProperty =
        AvaloniaProperty.RegisterDirect<AppendView, string?>(
            nameof(StoredConfigSelected),
            view => view.StoredConfigSelected,
            (view, value) => view.StoredConfigSelected = value);

    public string? StoredConfigSelected
    {
        get;
        set {
            SetAndRaise(StoredConfigSelectedProperty, ref field, value);
            RaisePropertyChanged(StoredConfigCanAddProperty, false, false);
        }
    }

    public static readonly DirectProperty<AppendView, List<string>> StoredConfigListProperty =
        AvaloniaProperty.RegisterDirect<AppendView, List<string>>(
            nameof(StoredConfigList),
            view => view.StoredConfigList);

    public List<string> StoredConfigList { get; init =>  SetAndRaise(StoredConfigListProperty, ref field, value); }

    public static readonly DirectProperty<AppendView, bool> StoredConfigCanAddProperty =
        AvaloniaProperty.RegisterDirect<AppendView, bool>(
            nameof(StoredConfigCanAdd),
            view => view.StoredConfigCanAdd);

    [MemberNotNullWhen(true,  nameof(StoredConfigSelected))]
    public bool StoredConfigCanAdd => !string.IsNullOrEmpty(StoredConfigSelected) && !AppendPaths.Contains(StoredConfigSelected);

    #endregion

    public AppendView() {
        // Designer Constructor
        InitializeComponent();
        _parentWindow = new MainWindow(true);
        AppendPaths = new ObservableAppendedConfigList(RetroLinker.Styles.DummyData.FillPathsList("config.cfg"));
        AppendPaths.CollectionChanged += (_, args) =>
            RaisePropertyChanged(StoredConfigCanAddProperty, false, false);

        _prevConfig = true;
        StoredConfigList = [.. RetroLinker.Styles.DummyData.FillPathsList("append.cfg")];
        StoredConfigSelected = StoredConfigList[Index.Start];
    }

    public AppendView(MainWindow mainWindow)
    {
        InitializeComponent();
        _parentWindow = mainWindow;
        SelectedConfigFile = _parentWindow.PermaView?.ConfigSelectedItem.ToString();
        var tryObservablePaths = _parentWindow.PermaView?.BuildingLink.RaAppendedConfig as ObservableAppendedConfigList;
        if (tryObservablePaths is null) {
            // Logger.LogWarn($"[{nameof(AppendView)}] Failed to obtain paths from RaAppendedConfig");
            tryObservablePaths = new ObservableAppendedConfigList();
        }
        AppendPaths = tryObservablePaths;
        AppendPaths.CollectionChanged += (_, args) =>
            RaisePropertyChanged(StoredConfigCanAddProperty, false, false);

        StoredConfigList = _parentWindow.Settings.SavedAppendedConfigs;
        _prevConfig = _parentWindow.Settings.PrevConfig;
        if (StoredConfigList.Count > 0) StoredConfigSelected = StoredConfigList[Index.Start];
    }

    // Window Object
    private readonly MainWindow _parentWindow;

    // FIELDS
    private const OpenOpts ConfigOpt = OpenOpts.RAcfg;
    private readonly bool _prevConfig;
    private readonly List<string> _newAppendedConfigToSave = new();

    // Append Config controls

    private bool AddToAppendPaths(string configFilePath)
    {
        if (configFilePath == string.Empty) return false;
        if (AppendPaths.Contains(configFilePath)) return false;
        AppendPaths.Add(configFilePath);
        return true;
    }

    private void LockControls(bool locked) => gridContent.IsEnabled = !locked;

    private async void BtnConfigPathBrowse_ClickAsync()
    {
        try
        {
            LockControls(true);
            var loadedFile = await FileDialogOps.OpenFileAsync(ConfigOpt, _parentWindow);
            if (string.IsNullOrWhiteSpace(loadedFile)) return;
            AddToAppendPaths(loadedFile);

            if (!_prevConfig) return;
            if (!StoredConfigList.Contains(loadedFile)) _newAppendedConfigToSave.Add(loadedFile);
        }
        catch (Exception e) { await this.PopUpGenericError(e); }
        finally { LockControls(false); }
    }

    private void BtnConfigPathBrowse_OnClick(object? sender, RoutedEventArgs e) => BtnConfigPathBrowse_ClickAsync();

    public void TryAddSavedConfig() {
        if (StoredConfigCanAdd) AddToAppendPaths(StoredConfigSelected);
    }


    // View Buttons
    private void BtnSaveAppend_OnClick(object? sender, RoutedEventArgs e) {
        var appendArg = (AppendPaths.Count == 0)
            ? null
            : AppendPaths;
        _parentWindow.ReturnToMainView(this, appendArg);
    }

    private void BtnDiscAppend_OnClick(object? sender, RoutedEventArgs e) => _parentWindow.ReturnToMainView();

    private void Control_OnUnloaded(object? sender, RoutedEventArgs e) {
        if (_prevConfig) _parentWindow.Settings.SavedAppendedConfigs.AddRange(_newAppendedConfigToSave);
    }
}