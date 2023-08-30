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

        // Window Obj
        private TopLevel SettWindow { get; set; }
        private Window Current_Window { get; set; }
        private IClassicDesktopStyleApplicationLifetime deskWindow = (IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime;

        // PROPS/STATICS
        private byte PrefTheme { get; set; }
        private string PrefRADir { get; set; }
        private string PrefROMPath { get; set; }
        private bool PrevConfig { get; set; }
        private bool AllwaysDesktop { get; set; }
        private bool CpyUserIcon { get; set; }
        private string ConvIcoDir { get; set; }
        private bool ExtractIco { get; set; }

        private bool DesktopOS { get; } = System.OperatingSystem.IsWindows();

        static readonly ThemeVariant dark_theme = ThemeVariant.Dark;
        static readonly ThemeVariant light_theme = ThemeVariant.Light;
        static readonly ThemeVariant system_theme = ThemeVariant.Default;

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
            LoadFromSettings();
            if (!DesktopOS)
            {
                panelWindowsOnlyControls.IsEnabled = false;
                txtDefRADir.IsReadOnly = false;
            }

            
        }

        void LoadFromSettings()
        {
            PrefTheme = Settings.PreferedTheme;
            PrefRADir = Settings.DEFRADir;
            PrefROMPath = Settings.DEFROMPath;
            PrevConfig = Settings.PrevConfig;
            AllwaysDesktop = Settings.AllwaysDesktop;
            CpyUserIcon = Settings.CpyUserIcon;
            ConvIcoDir = Settings.ConvICONPath;
            ExtractIco = Settings.ExtractIco;

            txtDefRADir.Text = PrefRADir;
            txtDefROMPath.Text = PrefROMPath;
            chkPrevCONFIG.IsChecked = PrevConfig;
            chkAllwaysDesktop.IsChecked = AllwaysDesktop;
            chkCpyUserIcon.IsChecked = CpyUserIcon;
            txtIcoSavPath.Text = System.IO.Path.GetFullPath(ConvIcoDir);
            chkExtractIco.IsChecked = ExtractIco;
        }

        //APARIENCIA
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
            { txtDefRADir.Text = file; PrefRADir = txtDefRADir.Text; }
        }

        void btnclrDefRADir_Click(object sender, RoutedEventArgs e)
        {
            txtDefRADir.Text = string.Empty; PrefRADir = string.Empty;
        }

        // DIR PADRE
        async void btnDefROMPath_Click(object sender, RoutedEventArgs e)
        {
            string file = await FileOps.OpenFolderAsync(0, SettWindow);
            if (file != null)
            { txtDefROMPath.Text = file; PrefROMPath = txtDefROMPath.Text; }
        }

        void btnclrDefROMPath_Click(object sender, RoutedEventArgs e)
        {
            txtDefROMPath.Text = string.Empty; PrefROMPath = string.Empty;
        }

        // ICONS
        // WINDOWS OS ONLY
        async void btnIcoSavPath_Click(object sender, RoutedEventArgs e)
        {
            string file = await FileOps.OpenFolderAsync(1, SettWindow);
            if (file != null)
            { txtIcoSavPath.Text = file; ConvIcoDir = txtIcoSavPath.Text; }
        }

        void btnclrIcoSavPath_Click(object sender, RoutedEventArgs e)
        {
            ConvIcoDir = FileOps.UserAssetsDir; txtIcoSavPath.Text = System.IO.Path.GetFullPath(ConvIcoDir);
        }

        #region Window/Dialog Controls
        // Window/Dialog Controls
        async void btnDEFSettings_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxStandardParams msparams = new()
            {
                ContentTitle = "Restaurar Defaults",
                ContentMessage = "¿Está seguro de restaurar la configuracion default?",
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ButtonDefinitions = MsBox.Avalonia.Enums.ButtonEnum.OkCancel,
                EnterDefaultButton = MsBox.Avalonia.Enums.ClickEnum.Ok,
                EscDefaultButton = MsBox.Avalonia.Enums.ClickEnum.Cancel,

            };
            var msbox = MessageBoxManager.GetMessageBoxStandard(msparams);
            var result = await msbox.ShowWindowDialogAsync(Current_Window);
            if (result == MsBox.Avalonia.Enums.ButtonResult.Ok)
            {
                Settings.LoadSettingsDefault();
                LoadFromSettings();
            }
                
        }

        void btnDISSettings_Click(object sender, RoutedEventArgs e)
        {
            Current_Window.Close();
        }

        void btnCONSettings_Click(object sender, RoutedEventArgs e)
        {
            if (!DesktopOS) { PrefRADir = txtDefRADir.Text; }
            // Set bools
            PrevConfig = (bool)chkPrevCONFIG.IsChecked;
            AllwaysDesktop = (bool)chkAllwaysDesktop.IsChecked;
            CpyUserIcon = (bool)chkCpyUserIcon.IsChecked;
            ExtractIco = (bool)chkExtractIco.IsChecked;

            Settings.SetSettings(PrefTheme, PrefRADir, PrefROMPath, PrevConfig,
                                 AllwaysDesktop, CpyUserIcon, ConvIcoDir, ExtractIco);
            Settings.WriteSettingsFile();
            FileOps.SetROMPadre(PrefROMPath, SettWindow);
            Current_Window.Close();
        }
        #endregion
    }
}
