using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Styling;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using RetroarchShortcutterV2.Models;

namespace RetroarchShortcutterV2.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        { InitializeComponent(); }

        public SettingsView(MainWindow _mainWindow, SettingsWindow parentWindow, bool desktopOs)
        {
            InitializeComponent();
            mainWindow = _mainWindow;
            ParentWindow = parentWindow;
            DesktopOS = desktopOs;
        }
        
        // Window Obj
        private MainWindow mainWindow;
        private SettingsWindow ParentWindow;

        // PROPS/STATICS
        private bool FirstTimeLoad = true;
        private bool DesktopOS;
        static readonly ThemeVariant dark_theme = ThemeVariant.Dark;
        static readonly ThemeVariant light_theme = ThemeVariant.Light;
        static readonly ThemeVariant system_theme = ThemeVariant.Default;

        #region Loads
        // LOADS
        void SettingsView1_Loaded(object sender, RoutedEventArgs e)
        { 
            // Settings
            ApplySettingsToControls();
            //System.Diagnostics.Debug.WriteLine($"SettingView Cargado por primera vez? {FirstTimeLoad}", "[Debg]");
        }

        void ApplySettingsToControls()
        { 
            chkPrevCONFIG.IsChecked = ParentWindow.settings.PrevConfig;
            chkAllwaysDesktop.IsChecked = ParentWindow.settings.AllwaysDesktop;
            chkCpyUserIcon.IsChecked = ParentWindow.settings.CpyUserIcon;
            LoadTheme(ParentWindow.settings.PreferedTheme);
        }
        #endregion

        //APARIENCIA
        // TODO: Optimizar con el uso de un evento, posiblemente basado en el byte de tema
        void LoadTheme(byte ThemeCode)
        {
            // El designer de Avalonia se rompe en esta parte, asi que puse una condicion para DEBUG
            switch (ThemeCode)
            {
                case 1: // Case de que el tema sea 'claro'
                    ThemeSwitch.IsChecked = false;
                    break;
                case 2: // Case de que el tema sea 'oscuro'
                    ThemeSwitch.IsChecked = true;
                    break;
                default:// Case de culaquier otro caso (0 = tema segun el sistema)
                    ThemeDefault.IsChecked = true;
                    break;
            }
        }
        
        void ThemeSwitch_CheckedChanged(object sender, RoutedEventArgs e)
        {
            // TODO: El designer de Avalonia se rompe en esta parte
            if ((bool)ThemeSwitch.IsChecked)
            {
                Application.Current.RequestedThemeVariant = dark_theme;
                ParentWindow.settings.PreferedTheme = 2;
            }
            else
            {
                Application.Current.RequestedThemeVariant = light_theme;
                ParentWindow.settings.PreferedTheme = 1;
            }
        }

        void ThemeDefault_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if ((bool)ThemeDefault.IsChecked)
            {
                // TODO: El designer de Avalonia se rompe en esta parte
                Application.Current.RequestedThemeVariant = system_theme;
                ThemeSwitch.IsEnabled = false;
                ParentWindow.settings.PreferedTheme = 0;
            }
            else
            { ThemeSwitch.IsEnabled = true; ThemeSwitch_CheckedChanged(sender, e); }
        }
        
#if DEBUG
        void SettingsView2_Loaded(object sender, RoutedEventArgs e)
        {
            _ = sender.ToString();
        }
#endif
    }
}
