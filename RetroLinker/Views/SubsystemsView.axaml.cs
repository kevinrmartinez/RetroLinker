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
using RetroLinker.Models.IO;

namespace RetroLinker.Views;

public partial class SubsystemsView : UserControl
{
    // Avalonia Props
    public static readonly DirectProperty<SubsystemsView, string> SubSystemProperty =
        AvaloniaProperty.RegisterDirect<SubsystemsView, string>(
            nameof(SubSystem),
            view => view.SubSystem,
            (view, obj) => view.SubSystem = obj);
    public string SubSystem { get; set => SetAndRaise(SubSystemProperty, ref field, value); }

    public static readonly DirectProperty<SubsystemsView, ICollection<string>> ArgumentsProperty =
        AvaloniaProperty.RegisterDirect<SubsystemsView, ICollection<string>>(
            nameof(Arguments),
            view => view.Arguments,
            (view, col) => view.Arguments = col);
    public ICollection<string> Arguments { get; set => SetAndRaise(ArgumentsProperty, ref field, value); }
    
    public static readonly DirectProperty<SubsystemsView, string?> NewArgumentProperty =
        AvaloniaProperty.RegisterDirect<SubsystemsView, string?>(
            nameof(NewArgument),
            view => view.NewArgument,
            (view, obj) => view.NewArgument = obj);
    public string? NewArgument { get; set => SetAndRaise(NewArgumentProperty, ref field, value); }
    
    // Props
    public string Core { get; init; }
    public string CoreContent { get; init; }
    
    // Designer Constructor
    public SubsystemsView()
    {
        InitializeComponent();
        Core = "[CORE]";
        CoreContent = "rom.zip";
        SubSystem = "[SUBSYSTEM]";
        Arguments = FillListTest();
        _parentWindow = new MainWindow(true);
        DataContext = this;
    }

    // Active Constructor
    public SubsystemsView(MainWindow mainWindow)
    {
        InitializeComponent();
        _parentWindow = mainWindow;
        IUserLinkParameters buildingLink = _parentWindow.PermaView?.BuildingLink ?? new UserLinkBuilder();
        // Core = buildingLink.ROMcore;
        Core = mainWindow.PermaView?.comboCore.Text ?? string.Empty;    // Temporal solution
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
        DataContext = this;
    }
    
    // Window Object
    private readonly MainWindow _parentWindow;
    
    // Subsystem controls
    
    private void LockControls(bool locked) => gridContent.IsEnabled = !locked;
    
    private void AddNewArgument(bool addNew) {
        if (ArgsList is null) return;
        ArgsList.AddButton?.IsEnabled = !addNew;
        BorderNewArg.IsVisible = addNew;
    }

    private void ArgsList_OnAddItemClick(object? sender, RoutedEventArgs e) {
        AddNewArgument(true);
        TextBoxNewArg.Focus();
    }

    private async void ButtonNewArgBrowse_ClickAsync()
    {
        try 
        {
            LockControls(true);
            var file = await FileDialogOps.OpenFileAsync(OpenOpts.RAroms, _parentWindow);
            if (string.IsNullOrEmpty(file)) return;
            TextBoxNewArg.Text = file;
        }
        catch (System.Exception ex) { _ = this.PopUpGenericError(ex); }
        finally { LockControls(false); }
    }

    private void ButtonNewArgBrowse_OnClick(object? sender, RoutedEventArgs e) => ButtonNewArgBrowse_ClickAsync();

    private void ButtonNewArgDiscard_OnClick(object? sender, RoutedEventArgs e) {
        TextBoxNewArg.Text = null;
        AddNewArgument(false);
    }

    private void ButtonNewArgAccept_OnClick(object? sender, RoutedEventArgs e) {
        if (!string.IsNullOrWhiteSpace(NewArgument)) Arguments.Add(NewArgument);
        TextBoxNewArg.Text = null;
        AddNewArgument(false);
    }
    
    private void ArgsList_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (sender is not Styles.UserStringList stringList) return;
        switch (e.Property.Name) {
            case "IsEnabled":
                if (!stringList.IsEnabled) AddNewArgument(false);
                break;
            default:
                break;
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
    
    // Designer Only
    private ObservableCollection<string> FillListTest()
    {
        var arguments = new ObservableCollection<string>();
        var pfx = FileOps.CombineMultipleInputs(FileOps.UserDesktop, "testing");
        for (int i = 0; i < 16; i++) {
            var testPath = FileOps.CombineMultipleInputs(pfx, $"rom{i}.bin");
            arguments.Add(testPath);
        }
        return arguments;
    }
}