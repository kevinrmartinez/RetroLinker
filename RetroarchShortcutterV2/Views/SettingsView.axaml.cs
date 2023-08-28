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
        private byte PrefTheme { get; set; } = Settings.PreferedTheme;
        private string PrefRADir { get; set; } = Settings.DEFRADir;
        private string PrefROMPath { get; set; } = Settings.DEFROMPath;
        private bool PrevConfigs { get; set; } = Settings.PrevConfig;
        private bool AllwaysDesktop { get; set; } = Settings.AllwaysDesktop;
        private bool CpyUserIcon { get; set; } = Settings.CpyUserIcon;
        private string ConvIcoDir { get; set; } = Settings.ConvICONPath;
        private bool ExtractIco { get; set; } = Settings.ExtractIco;

        private bool DesktopOS { get; } = System.OperatingSystem.IsWindows();
        

        // LOADS
        void SettingsView1_Loaded(object sender, RoutedEventArgs e)
        {
            // Window objects
            SettWindow = TopLevel.GetTopLevel(this);
            var windows_list = deskWindow.Windows;
            Current_Window = windows_list[1];

            // Apariencia
            //Current_Window.RequestedThemeVariant = Settings.LoadThemeVariant();
            switch(PrefTheme)
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

            // OTROS
            txtDefRADir.Text = PrefRADir;
            txtDefROMPath.Text = PrefROMPath;
            chkPrevCONFIG.IsChecked = PrevConfigs;
            chkAllwaysDesktop.IsChecked = AllwaysDesktop;
            chkCpyUserIcon.IsChecked = CpyUserIcon;
            txtIcoSavPath.Text = System.IO.Path.GetFullPath(ConvIcoDir);
            chkExtractIco.IsChecked = ExtractIco;
            if (!DesktopOS)
            {
                panelWindowsOnlyControls.IsEnabled = false;
                txtDefRADir.IsReadOnly = false;
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
        async void btnDefRADir_Click(object sender, RoutedEventArgs e)
        {
            int template;
            if (DesktopOS) { template = 0; }        // FilePicker Option para .exe de Windows
            else { template = 4; }                  // FilePicker Option para .AppImage de Windows
            string file = await FileOps.OpenFileAsync(template, SettWindow);
            if (file != null)
            { txtDefRADir.Text = file; }
        }

        void btnsavDefRADir_Click(object sender, RoutedEventArgs e)
        {
            if (txtDefRADir.Text != string.Empty) { PrefRADir = txtDefRADir.Text; }
        }

        void btnclrDefRADir_Click(object sender, RoutedEventArgs e)
        {
            txtDefRADir.Text = string.Empty; PrefRADir = string.Empty;
        }

        // DIR PADRE
        async void btnDefROMPath_Click(object sender, RoutedEventArgs e)
        {
            string file = await FileOps.OpenFileAsync(1, SettWindow);
            if (file != null)
            { txtDefROMPath.Text = file; }
        }

        void btnsavDefROMPath_Click(object sender, RoutedEventArgs e)
        {
            if (txtDefROMPath.Text != string.Empty) { PrefROMPath = txtDefROMPath.Text; }
        }

        void btnclrDefROMPath_Click(object sender, RoutedEventArgs e)
        {
            txtDefROMPath.Text = string.Empty; PrefROMPath = string.Empty;
        }

        // ICONS
        // WINDOWS ONLY
        async void btnIcoSavPath_Click(object sender, RoutedEventArgs e)
        {
            int template;
            if (DesktopOS) { template = 3; }        // FilePicker Option para iconos de Windows
            else { template = 5; }
            string file = await FileOps.OpenFileAsync(template, SettWindow);
            if (file != null)
            { txtIcoSavPath.Text = file; }
        }

        void btnsavIcoSavPath_Click(object sender, RoutedEventArgs e)
        {
            if (txtIcoSavPath.Text != string.Empty) { ConvIcoDir = txtIcoSavPath.Text; }
        }

        void btnclrIcoSavPath_Click(object sender, RoutedEventArgs e)
        {
            ConvIcoDir = FileOps.UserAssetsDir; txtIcoSavPath.Text = System.IO.Path.GetFullPath(ConvIcoDir);
        }

        // Dialog Controls
        void btnDISSettings_Click(object sender, RoutedEventArgs e)
        {
            Current_Window.Close();
        }
    }
}
