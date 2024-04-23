using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RetroLinker.Models;

namespace RetroLinker.Views;

public partial class PatchesView : UserControl
{
    public PatchesView(MainWindow mainWindow)
    {
        InitializeComponent();
        ParentWindow = mainWindow;
    }
    
    // Window Object
    private MainWindow ParentWindow;
    

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        ParentWindow.ReturnToMainView(this);
    }
}