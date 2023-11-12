using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RetroarchShortcutterV2.Models;
using RetroarchShortcutterV2.ViewModels;

namespace RetroarchShortcutterV2.Views;

public partial class SettingsView2 : UserControl
{
    public SettingsView2()
    {
        InitializeComponent();
    }
    
    public SettingsView2(MainWindow mainWindow, SettingsWindow settingsWindow, bool desktopOs, Settings _settings)
    {
        InitializeComponent();
        MainAppWindow = mainWindow;
        ParentWindow = settingsWindow;
        DesktopOS = desktopOs;
        settings = _settings;
    }
    
    // Window Obj
    private MainWindow MainAppWindow;
    private SettingsWindow ParentWindow;
    
    // PROPS/STATICS
    private bool FirstTimeLoad = true;
    private Settings settings;
    private bool DesktopOS;
}