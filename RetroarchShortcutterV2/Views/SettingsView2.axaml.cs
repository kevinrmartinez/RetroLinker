using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using RetroarchShortcutterV2.Models;

namespace RetroarchShortcutterV2.Views;

public partial class SettingsView2 : UserControl
{
    public SettingsView2()
    { InitializeComponent(); }
    
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
    
    // LOAD
    void SettingsView2_1_Loaded(object sender, RoutedEventArgs e)
    {
        if (FirstTimeLoad)
        {
            if (!DesktopOS)
            {
                panelWindowsOnlyControls.IsEnabled = false;
                txtDefRADir.IsReadOnly = false;
            }
        }
        // Settings
        ApplySettingsToControls();
        if (settings.UserAssetsPath == settings.ConvICONPath) 
        { chkUseUserAssets.IsChecked = true; }
    }
    
    void ApplySettingsToControls()
    { 
        txtUserAssets.Text = System.IO.Path.GetFullPath(settings.UserAssetsPath);
        txtDefRADir.Text = settings.DEFRADir;
        txtDefROMPath.Text = settings.DEFROMPath;
        txtIcoSavPath.Text = System.IO.Path.GetFullPath(settings.ConvICONPath);
        chkExtractIco.IsChecked = settings.ExtractIco;
    }
    
    // USER ASSETS
            async void btnUserAssets_Click(object sender, RoutedEventArgs e)
            {
                string folder = await FileOps.OpenFolderAsync(template:0, ParentWindow);
                // TODO: Resolver sin el uso de null
                if (folder != null)
                { txtUserAssets.Text = folder; settings.UserAssetsPath = folder; }
            }
    
            void btnclrUserAssets_Click(object sender, RoutedEventArgs e)
            {
                txtUserAssets.Text = string.Empty; 
                settings.UserAssetsPath = string.Empty;
            }
    
    
            // EJECUTABLE
            async void btnDefRADir_Click(object sender, RoutedEventArgs e)
            {
                PickerOpt.OpenOpts opt;
                if (DesktopOS) { opt = PickerOpt.OpenOpts.RAexe; }        // FilePicker Option para .exe de Windows
                else { opt = PickerOpt.OpenOpts.RAout; }                  // FilePicker Option para .AppImage de Windows
                string file = await FileOps.OpenFileAsync(opt, ParentWindow);
                // TODO: Resolver sin el uso de null
                if (file != null)
                { 
                    txtDefRADir.Text = file; 
                    settings.DEFRADir = file; 
                }
            }
    
            void btnclrDefRADir_Click(object sender, RoutedEventArgs e)
            {
                txtDefRADir.Text = string.Empty; 
                settings.DEFRADir = string.Empty;
            }
    
            // DIR PADRE
            async void btnDefROMPath_Click(object sender, RoutedEventArgs e)
            {
                string folder = await FileOps.OpenFolderAsync(template:1, ParentWindow);
                // TODO: Resolver sin el uso de null
                if (folder != null)
                { 
                    txtDefROMPath.Text = folder; 
                    settings.DEFROMPath = folder; 
                }
            }   // TODO: No parece aplicarce en ningun Linux
    
            void btnclrDefROMPath_Click(object sender, RoutedEventArgs e)
            {
                txtDefROMPath.Text = string.Empty; settings.DEFROMPath = string.Empty;
            }
            
            // ICONS

            #region WINDOWS OS ONLY
            async void btnIcoSavPath_Click(object sender, RoutedEventArgs e)
            {
                string folder = await FileOps.OpenFolderAsync(template:2, ParentWindow);
                // TODO: Resolver sin el uso de null
                if (folder != null)
                {
                    txtIcoSavPath.Text = folder; 
                    settings.ConvICONPath = folder;
                }
            }

            void btnclrIcoSavPath_Click(object sender, RoutedEventArgs e)
            {
                settings.ConvICONPath = FileOps.DefUserAssetsDir; 
                txtIcoSavPath.Text = System.IO.Path.GetFullPath(settings.ConvICONPath);
            }

            void chkUseUserAssets_Checked(object sender, RoutedEventArgs e)
            {
                panelWindowsOnlyControls2.IsEnabled = !(bool)chkUseUserAssets.IsChecked;
                settings.ConvICONPath = settings.UserAssetsPath;
                txtIcoSavPath.Text = System.IO.Path.GetFullPath(settings.ConvICONPath);
            }
            #endregion
}