using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Styling;
using RetroarchShortcutterV2.Models;

namespace RetroarchShortcutterV2.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        { InitializeComponent(); }

        // Window Obj
        private TopLevel SettWindow { get; set; }
        private Window Current_Window { get; set; }
        private IClassicDesktopStyleApplicationLifetime deskWindow = (IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime;

        // PROPS/STATICS
        private byte PrefTheme { get; set; }
        private string PrefRADir { get; set; }
        private string PrefROMPath { get; set; }
        private bool PrevConfigs { get; set; }
        private bool AllwaysDesktop { get; set; }
        private bool CpyUserIcon { get; set; }
        private string ConvIcoDir { get; set; }
        private bool ExtractIco { get; set; }
        

        // LOADS
        void SettingsView1_Loaded(object sender, RoutedEventArgs e)
        {
            // Window objects
            SettWindow = TopLevel.GetTopLevel(this);
            var windows_list = deskWindow.Windows;
            Current_Window = windows_list[1];

            // Apariencia
            Current_Window.RequestedThemeVariant = Settings.LoadThemeVariant();
            switch(Settings.PreferedTheme)
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

        //APARIENCIA
        static ThemeVariant dark_theme = ThemeVariant.Dark;
        static ThemeVariant light_theme = ThemeVariant.Light;
        static ThemeVariant system_theme = ThemeVariant.Default;

        void ThemeSwitch_CheckedChanged(object sender, RoutedEventArgs e)
        {
            
            if ((bool)ThemeSwitch.IsChecked)
            {
                deskWindow.MainWindow.RequestedThemeVariant = dark_theme;
                SettWindow.RequestedThemeVariant = dark_theme;
                PrefTheme = 2;
            }
            else
            {
                deskWindow.MainWindow.RequestedThemeVariant = light_theme;
                SettWindow.RequestedThemeVariant = light_theme;
                PrefTheme = 1;
            }
        }

        void ThemeDefault_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if ((bool)ThemeDefault.IsChecked)
            {
                deskWindow.MainWindow.RequestedThemeVariant = system_theme;
                SettWindow.RequestedThemeVariant = system_theme;
                ThemeSwitch.IsEnabled = false;
                PrefTheme = 0;
            }
            else
            { ThemeSwitch.IsEnabled = true; ThemeSwitch_CheckedChanged(sender, e); }

        }

        // EJECUTABLE


        // Dialog Controls
        void btnDISSettings_Click(object sender, RoutedEventArgs e)
        {
            Current_Window.Close();
        }
    }
}
