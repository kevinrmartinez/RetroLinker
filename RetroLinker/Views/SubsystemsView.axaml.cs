using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RetroLinker.Models;

namespace RetroLinker.Views;

public partial class SubsystemsView : UserControl
{
    // Window Object
    private MainWindow ParentWindow;
    
    // Props
    public ObservableCollection<string> Arguments { get; set; }
    public string? NewArgument { get; set; }
    
    // Designer Constructor
    public SubsystemsView()
    {
        InitializeComponent();
        ParentWindow = new MainWindow(true);
        Arguments = FillListTest();
        DataContext = this;
    }

    // Active Constructor
    public SubsystemsView(MainWindow mainWindow, object[] subsysArgs)
    {
        InitializeComponent();
        ParentWindow = mainWindow;
        Arguments = new ObservableCollection<string>();
        foreach (var arg in subsysArgs) {
            if (arg is string argString) Arguments.Add(argString);
        }
        DataContext = this;     // Why is this necessary...
    }


    private void AddNewArgument(bool addNew)
    {
        ArgsList.IsEnabled = !addNew;
        BorderNewArg.IsVisible = addNew;
    }

    private void ArgsList_OnAddItemClick(object? sender, RoutedEventArgs e) {
        AddNewArgument(true);
        TextBoxNewArg.Focus();
    }

    private void ButtonNewArgDiscard_OnClick(object? sender, RoutedEventArgs e) {
        NewArgument = null;
        AddNewArgument(false);
    }

    private void ButtonNewArgAccept_OnClick(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(NewArgument))
            if (!Arguments.Contains(NewArgument)) Arguments.Add(NewArgument);
        NewArgument = null;
        AddNewArgument(false);
    }
    
    private void BtnSavePatch_OnClick(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    private void BtnDiscPatch_OnClick(object? sender, RoutedEventArgs e) => ParentWindow.ReturnToMainView();
    
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