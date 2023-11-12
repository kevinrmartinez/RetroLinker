using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using RetroarchShortcutterV2.Models;

namespace RetroarchShortcutterV2.Views;

public partial class SettingsView2 : UserControl
{
    public SettingsView2()
    {
        InitializeComponent();
    }
    
    public SettingsView2(MainWindow mainWindow, bool desktopOs, Settings _settings)
    {
        InitializeComponent();
        ParentWindow = mainWindow;
        DesktopOS = desktopOs;
        settings = _settings;
    }
    
    // Window Obj
    private MainWindow ParentWindow;
    
    // PROPS/STATICS
    private bool FirstTimeLoad = true;
    private Settings settings;
    private bool DesktopOS;
}