using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RetroLinker.Models;
using RetroLinker.Models.Avalonia;
using RetroLinker.Styles;

namespace RetroLinker.Views;

public partial class SubsystemsView : UserControl
{
    // Props
    public string Core { get; init; }
    public string CoreContent { get; init; }
    public string SubSystem { get; set; }
    public ObservableCollection<string> Arguments { get; set; }
    public string? NewArgument { get; set; }
    
    // Designer Constructor
    public SubsystemsView()
    {
        InitializeComponent();
        Core = "[CORE]";
        CoreContent = "rom.zip";
        SubSystem = "[SUBSYSTEM]";
        _parentWindow = new MainWindow(true);
        Arguments = FillListTest();
        DataContext = this;
    }

    // Active Constructor
    public SubsystemsView(MainWindow mainWindow)
    {
        InitializeComponent();
        _parentWindow = mainWindow;
        var buildingLink = _parentWindow.PermaView?.BuildingLink ?? new Shortcutter();
        Core = buildingLink.ROMcore;
        CoreContent = buildingLink.ROMdir;
        var subsystem = string.Empty;
        var subsystemArgs = new List<string>();
        try
        {
            var subsysArgs = CommandManager.ResolveSubsystemArg(buildingLink.SubsysArg);
            subsystem = subsysArgs.Item1;
            subsystemArgs = subsysArgs.Item2;
        }
        catch (System.ArgumentNullException) {
            // Ignore
        }
        catch (System.ArgumentException ex) {
            Logger.LogErro(ex);
        }
        
        SubSystem = subsystem;
        Arguments = new ObservableCollection<string>(subsystemArgs);
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

    private void ButtonNewArgAccept_OnClick(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(NewArgument))
            if (!Arguments.Contains(NewArgument)) Arguments.Add(NewArgument);
        TextBoxNewArg.Text = null;
        AddNewArgument(false);
    }
    
    private void ArgsList_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (sender is not UserStringList stringList) return;
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
            ? string.Empty 
            : CommandManager.CreateSubsystemArg(SubSystem, Arguments);
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