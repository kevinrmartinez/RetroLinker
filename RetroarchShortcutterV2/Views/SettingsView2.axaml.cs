using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using RetroarchShortcutterV2.Models;

namespace RetroarchShortcutterV2.Views;

public partial class SettingsView2 : UserControl
{
    public SettingsView2()
    { InitializeComponent(); }
    
    public SettingsView2(MainWindow mainWindow, SettingsWindow settingsWindow, bool desktopOs)
    {
        InitializeComponent();
        MainAppWindow = mainWindow;
        ParentWindow = settingsWindow;
        DesktopOS = desktopOs;
    }
    
    // Window Obj
    private MainWindow MainAppWindow;
    private SettingsWindow ParentWindow;
    
    // PROPS/STATICS
    private bool FirstTimeLoad = true;
    private bool DesktopOS;
    
    // LOAD
    void SettingsView2_1_Loaded(object sender, RoutedEventArgs e)
    {
        if (FirstTimeLoad)
        {
            if (!DesktopOS)
            { txtDefRADir.IsReadOnly = false; }

            FirstTimeLoad = false;
        }
        // Settings
        ApplySettingsToControls();
    }
    
    void ApplySettingsToControls()
    { 
        // TODO: Decidirse si usar directorio relativo o absoluto
        txtUserAssets.Text = System.IO.Path.GetFullPath(ParentWindow.settings.UserAssetsPath);
        txtDefRADir.Text = (string.IsNullOrEmpty(ParentWindow.settings.DEFRADir) && !DesktopOS) ? FileOps.LinuxRABin : ParentWindow.settings.DEFRADir;
        txtDefROMPath.Text = ParentWindow.settings.DEFROMPath;
    }
    
    // USER ASSETS
    async void btnUserAssets_Click(object sender, RoutedEventArgs e)
    {
        string folder = await FileOps.OpenFolderAsync(template:0, ParentWindow);
        if (!string.IsNullOrWhiteSpace(folder))
        { txtUserAssets.Text = folder; ParentWindow.settings.UserAssetsPath = folder; }
    }
    
    void btnclrUserAssets_Click(object sender, RoutedEventArgs e)
    {
        ParentWindow.settings.UserAssetsPath = ParentWindow.DEFsettings.UserAssetsPath;
        txtUserAssets.Text = ParentWindow.settings.UserAssetsPath;
        
    }
    
    // EJECUTABLE
    async void btnDefRADir_Click(object sender, RoutedEventArgs e)
    {
        PickerOpt.OpenOpts opt;
        opt = DesktopOS ? PickerOpt.OpenOpts.RAexe :    // FilePicker Option para .exe de Windows
                          PickerOpt.OpenOpts.RAout;     // FilePicker Option para .AppImage de Linux
        string file = await FileOps.OpenFileAsync(template:opt, ParentWindow);
        if (!string.IsNullOrWhiteSpace(file))
        { 
            txtDefRADir.Text = file; 
            ParentWindow.settings.DEFRADir = file; 
        }
    }
    
    void btnclrDefRADir_Click(object sender, RoutedEventArgs e)
    {
        ParentWindow.settings.DEFRADir = (DesktopOS) ? ParentWindow.DEFsettings.DEFRADir : FileOps.LinuxRABin;
        txtDefRADir.Text = ParentWindow.settings.DEFRADir;
        
    }
    
    // DIR PADRE
    async void btnDefROMPath_Click(object sender, RoutedEventArgs e)
    {
        string folder = await FileOps.OpenFolderAsync(template:1, ParentWindow);
        if (!string.IsNullOrWhiteSpace(folder))
        { 
            txtDefROMPath.Text = folder; 
            ParentWindow.settings.DEFROMPath = folder; 
        }
    }   // TODO: No parece aplicarce en ningun Linux
    
    void btnclrDefROMPath_Click(object sender, RoutedEventArgs e)
    {
        ParentWindow.settings.DEFROMPath = ParentWindow.DEFsettings.DEFROMPath;
        txtDefROMPath.Text = ParentWindow.settings.DEFROMPath;
    }

    //UNLOAD
    private void SettingsView2_1_OnUnloaded(object? sender, RoutedEventArgs e)
    {
        _ = e.Source;
    }
    
#if DEBUG
    void SettingsView2_1_Loaded2(object sender, RoutedEventArgs e)
    {
        _ = sender.ToString();
    }
#endif
}