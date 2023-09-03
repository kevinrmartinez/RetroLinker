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
        private TopLevel topLevel { get; set; }
        private Window Current_Window { get; set; }
        private IClassicDesktopStyleApplicationLifetime deskWindow = (IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime;

        // PROPS/STATICS
        private Settings settings;
        private bool DesktopOS { get; } = System.OperatingSystem.IsWindows();

        static readonly ThemeVariant dark_theme = ThemeVariant.Dark;
        static readonly ThemeVariant light_theme = ThemeVariant.Light;
        static readonly ThemeVariant system_theme = ThemeVariant.Default;

        #region Loads
        // LOADS
        void SettingsView1_Loaded(object sender, RoutedEventArgs e)
        {
            // Window objects
            topLevel = TopLevel.GetTopLevel(this);
            var windows_list = deskWindow.Windows;
            Current_Window = windows_list[1];

            // Settings
            LoadFromSettings();

            // Apariencia
            switch (settings.PreferedTheme)
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
            if (settings.UserAssetsPath == settings.ConvICONPath) 
            { chkUseUserAssets.IsChecked = true; }
            if (!DesktopOS)
            {
                panelWindowsOnlyControls.IsEnabled = false;
                txtDefRADir.IsReadOnly = false;
            }

            
        }

        void LoadFromSettings()
        {
            settings = SettingsOps.GetCachedSettings();

            txtUserAssets.Text = System.IO.Path.GetFullPath(settings.UserAssetsPath);
            txtDefRADir.Text = settings.DEFRADir;
            txtDefROMPath.Text = settings.DEFROMPath;
            chkPrevCONFIG.IsChecked = settings.PrevConfig;
            chkAllwaysDesktop.IsChecked = settings.AllwaysDesktop;
            chkCpyUserIcon.IsChecked = settings.CpyUserIcon;
            txtIcoSavPath.Text = System.IO.Path.GetFullPath(settings.ConvICONPath);
            chkExtractIco.IsChecked = settings.ExtractIco;
        }
        #endregion

        //APARIENCIA
        void ThemeSwitch_CheckedChanged(object sender, RoutedEventArgs e)
        {
            
            if ((bool)ThemeSwitch.IsChecked)
            {
                deskWindow.MainWindow.RequestedThemeVariant = dark_theme;
                topLevel.RequestedThemeVariant = dark_theme;
                settings.PreferedTheme = 2;
            }
            else
            {
                deskWindow.MainWindow.RequestedThemeVariant = light_theme;
                topLevel.RequestedThemeVariant = light_theme;
                settings.PreferedTheme = 1;
            }
        }

        void ThemeDefault_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if ((bool)ThemeDefault.IsChecked)
            {
                deskWindow.MainWindow.RequestedThemeVariant = system_theme;
                topLevel.RequestedThemeVariant = system_theme;
                ThemeSwitch.IsEnabled = false;
                settings.PreferedTheme = 0;
            }
            else
            { ThemeSwitch.IsEnabled = true; ThemeSwitch_CheckedChanged(sender, e); }

        }

        // USER ASSETS
        async void btnUserAssets_Click(object sender, RoutedEventArgs e)
        {
            string file = await FileOps.OpenFolderAsync(0, topLevel);
            if (file != null)
            { txtUserAssets.Text = file; settings.UserAssetsPath = txtUserAssets.Text; }
        }

        void btnclrUserAssets_Click(object sender, RoutedEventArgs e)
        {
            txtUserAssets.Text = string.Empty; settings.UserAssetsPath = string.Empty;
        }


        // EJECUTABLE
        async void btnDefRADir_Click(object sender, RoutedEventArgs e)
        {
            int template;
            if (DesktopOS) { template = 0; }        // FilePicker Option para .exe de Windows
            else { template = 4; }                  // FilePicker Option para .AppImage de Windows
            string file = await FileOps.OpenFileAsync(template, topLevel);
            if (file != null)
            { txtDefRADir.Text = file; settings.DEFRADir = txtDefRADir.Text; }
        }

        void btnclrDefRADir_Click(object sender, RoutedEventArgs e)
        {
            txtDefRADir.Text = string.Empty; settings.DEFRADir = string.Empty;
        }

        // DIR PADRE
        async void btnDefROMPath_Click(object sender, RoutedEventArgs e)
        {
            string file = await FileOps.OpenFolderAsync(0, topLevel);
            if (file != null)
            { txtDefROMPath.Text = file; settings.DEFROMPath = txtDefROMPath.Text; }
        }

        void btnclrDefROMPath_Click(object sender, RoutedEventArgs e)
        {
            txtDefROMPath.Text = string.Empty; settings.DEFROMPath = string.Empty;
        }

        // ICONS
        // WINDOWS OS ONLY
        async void btnIcoSavPath_Click(object sender, RoutedEventArgs e)
        {
            string file = await FileOps.OpenFolderAsync(1, topLevel);
            if (file != null)
            { txtIcoSavPath.Text = file; settings.ConvICONPath = txtIcoSavPath.Text; }
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

        #region Window/Dialog Controls
        // Window/Dialog Controls
        void btnDISSettings_Click(object sender, RoutedEventArgs e) => Current_Window.Close();

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
                settings = new();
                //SettingsOps.PrevConfigs = new();
                SettingsOps.WriteSettingsFile(settings);
                Current_Window.Close();
            }
                
        }

        void btnCONSettings_Click(object sender, RoutedEventArgs e)
        {
            if (!DesktopOS) { settings.DEFRADir = txtDefRADir.Text; }
            // Set bools
            settings.PrevConfig = (bool)chkPrevCONFIG.IsChecked;
            settings.AllwaysDesktop = (bool)chkAllwaysDesktop.IsChecked;
            settings.CpyUserIcon = (bool)chkCpyUserIcon.IsChecked;
            settings.ExtractIco = (bool)chkExtractIco.IsChecked;

            SettingsOps.WriteSettingsFile(settings);
            Current_Window.Close();
        }
        #endregion

#if DEBUG
        void SettingsView2_Loaded(object sender, RoutedEventArgs e)
        {
            _ = sender.ToString();
        }
#endif
    }
}
