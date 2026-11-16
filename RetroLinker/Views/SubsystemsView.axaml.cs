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
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RetroLinker.Models.Avalonia;
using RetroLinker.Models.DesktopLink;

namespace RetroLinker.Views;

public partial class SubsystemsView : UserControl
{
    #region AvaloniaProperties

    public static readonly DirectProperty<SubsystemsView, string> CoreProperty =
        AvaloniaProperty.RegisterDirect<SubsystemsView, string>(
            nameof(Core),
            view => view.Core);

    public string Core { get; init => SetAndRaise(CoreProperty, ref field, value); }

    public static readonly DirectProperty<SubsystemsView, string> CoreContentProperty =
        AvaloniaProperty.RegisterDirect<SubsystemsView, string>(
            nameof(CoreContent),
            view => view.CoreContent);

    public string CoreContent { get; init => SetAndRaise(CoreContentProperty, ref field, value); }

    public static readonly DirectProperty<SubsystemsView, string> SubSystemProperty =
        AvaloniaProperty.RegisterDirect<SubsystemsView, string>(
            nameof(SubSystem),
            view => view.SubSystem,
            (view, obj) => view.SubSystem = obj,
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public string SubSystem { get; set => SetAndRaise(SubSystemProperty, ref field, value); }

    public static readonly DirectProperty<SubsystemsView, bool> AddingNewArgumentProperty =
        AvaloniaProperty.RegisterDirect<SubsystemsView, bool>(
            nameof(AddingNewArgument),
            view => view.AddingNewArgument,
            (view, obj) => view.AddingNewArgument = obj);

    public bool AddingNewArgument
    {
        get;
        set {
            if (!value) NewArgument = null;
            SetAndRaise(AddingNewArgumentProperty, ref field, value);
        }
    }

    public static readonly DirectProperty<SubsystemsView, ICollection<string>> ArgumentsProperty =
        AvaloniaProperty.RegisterDirect<SubsystemsView, ICollection<string>>(
            nameof(Arguments),
            view => view.Arguments,
            (view, col) => view.Arguments = col,
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public ICollection<string> Arguments { get; set => SetAndRaise(ArgumentsProperty, ref field, value); }

    public static readonly DirectProperty<SubsystemsView, string?> NewArgumentProperty =
        AvaloniaProperty.RegisterDirect<SubsystemsView, string?>(
            nameof(NewArgument),
            view => view.NewArgument,
            (view, obj) => view.NewArgument = obj,
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public string? NewArgument { get; set => SetAndRaise(NewArgumentProperty, ref field, value); }

    #endregion

    // Designer Constructor
    public SubsystemsView()
    {
        InitializeComponent();
        Core = "[CORE]";
        CoreContent = "rom.zip";
        SubSystem = "[SUBSYSTEM]";
        Arguments = RetroLinker.Styles.DummyData.DummyPathsList;
        _parentWindow = new MainWindow(true);
    }

    // Active Constructor
    public SubsystemsView(MainWindow mainWindow)
    {
        InitializeComponent();
        _parentWindow = mainWindow;
        IShortcutParameters buildingLink = _parentWindow.PermaView?.BuildingLink ?? new ShortcutBuilder();
        Core = buildingLink.RaCore;
        CoreContent = buildingLink.RaRom;
        var subsystemArgs = buildingLink.RaCoreSubsystem;

        var subsystem = string.Empty;
        var subsystemArgsList = new ObservableCollection<string>();
        if (subsystemArgs is not null) {
            subsystem = subsystemArgs.Subsystem;
            subsystemArgsList = new ObservableCollection<string>(subsystemArgs.Args);
        }

        SubSystem = subsystem;
        Arguments = subsystemArgsList;
    }

    // Window Object
    private readonly MainWindow _parentWindow;

    // Subsystem controls

    private void LockControls(bool locked) => gridContent.IsEnabled = !locked;

    private void ArgsList_OnAddItemClick(object? sender, RoutedEventArgs e) {
        AddingNewArgument = true;
        TextBoxNewArg.Focus();
    }

    private async void ButtonNewArgBrowse_ClickAsync()
    {
        try
        {
            LockControls(true);
            var file = await FileDialogOps.OpenFileAsync(OpenOpts.RAroms, _parentWindow);
            if (string.IsNullOrEmpty(file)) return;
            NewArgument = file;
        }
        catch (Exception ex) { await this.PopUpGenericError(ex); }
        finally { LockControls(false); }
    }

    private void ButtonNewArgBrowse_OnClick(object? sender, RoutedEventArgs e) => ButtonNewArgBrowse_ClickAsync();

    private void ButtonNewArgDiscard_OnClick(object? sender, RoutedEventArgs e) {
        AddingNewArgument = false;
    }

    private void ButtonNewArgAccept_OnClick(object? sender, RoutedEventArgs e) {
        if (!string.IsNullOrWhiteSpace(NewArgument)) Arguments.Add(NewArgument);
        AddingNewArgument = false;
    }

    private void ArgsList_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (sender is not Styles.UserStringList stringList) return;
        if (e.Property == IsEnabledProperty)
        {
            if (!stringList.IsEnabled) AddingNewArgument = false;
        }
    }

    // View Controls
    private void BtnSaveSubsystem_OnClick(object? sender, RoutedEventArgs e)
    {
        var subsysArg = (string.IsNullOrEmpty(SubSystem))
            ? null
            : new SubsystemArgs(SubSystem, new List<string>(Arguments));
        _parentWindow.ReturnToMainView(this, subsysArg);
    }

    private void BtnDiscSubsystem_OnClick(object? sender, RoutedEventArgs e) => _parentWindow.ReturnToMainView();
}