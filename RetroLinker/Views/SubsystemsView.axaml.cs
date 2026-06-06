using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RetroLinker.Models;
using RetroLinker.Styles;

namespace RetroLinker.Views;

public partial class SubsystemsView : UserControl
{
    // Window Object
    private MainWindow ParentWindow;
    
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
        ParentWindow = new MainWindow(true);
        Arguments = FillListTest();
        DataContext = this;
    }

    // Active Constructor
    public SubsystemsView(MainWindow mainWindow, SubsystemReq subsystemReq)
    {
        InitializeComponent();
        ParentWindow = mainWindow;
        Core = subsystemReq.Core;
        CoreContent = subsystemReq.Content;
        var subsystem = string.Empty;
        var subsystemArgs = new List<string>();
        try {
            var subsysArgs = Commander.ResolveSubsystemArg(subsystemReq.SubSystemArg);
            subsystem = subsysArgs.Item1;
            subsystemArgs = subsysArgs.Item2;
        }
        catch (System.ArgumentException ex) {
            Logger.LogErro(ex);
        }
        SubSystem = subsystem;
        Arguments = new ObservableCollection<string>(subsystemArgs);
        DataContext = this;
    }


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
        try {
            var file = await Models.Avalonia.FileDialogOps.OpenFileAsync(Models.Avalonia.OpenOpts.RAroms, ParentWindow);
            if (string.IsNullOrEmpty(file)) return;
            TextBoxNewArg.Text = file;
        }
        catch (System.Exception ex) {
            Logger.LogErro(ex);
            // TODO: Needs access to an generic error pop-up
        }
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
            : Commander.CreateSubsystemArg(SubSystem, Arguments);
        ParentWindow.ReturnToMainView(this, subsysArg);
    }

    private void BtnDiscSubsystem_OnClick(object? sender, RoutedEventArgs e) => ParentWindow.ReturnToMainView();
    
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

public readonly struct SubsystemReq(string core, string coreContent, string subSystem)
{
    public string Core { get; } = core;
    public string Content { get; } = coreContent;
    public string SubSystemArg { get; } = subSystem;
}