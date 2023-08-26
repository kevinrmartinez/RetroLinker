using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Projektanker.Icons.Avalonia;

namespace RetroarchShortcutterV2.Views
{
    public partial class ConfigView : Window
    {
        public ConfigView()
        { InitializeComponent(); }


        //Settings Controls

        //Aparience
        ThemeVariant dark_theme = ThemeVariant.Dark;
        ThemeVariant light_theme = ThemeVariant.Light;
        ThemeVariant system_theme = ThemeVariant.Default;

        void ThemeSwitch_CheckedChanged(object sender, RoutedEventArgs e)
        {
            var deskWindow = (IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime;
            if ((bool)ThemeSwitch.IsChecked) 
            {
                deskWindow.MainWindow.RequestedThemeVariant = dark_theme;
                this.RequestedThemeVariant = dark_theme; 
            }
            else 
            {
                deskWindow.MainWindow.RequestedThemeVariant = light_theme;
                this.RequestedThemeVariant = light_theme; 
            }
        }


        void ThemeDefault_CheckedChanged(object sender, RoutedEventArgs e)
        {
            var deskWindow = (IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime;
            if ((bool)ThemeDefault.IsChecked) 
            { 
                deskWindow.MainWindow.RequestedThemeVariant = system_theme;
                this.RequestedThemeVariant = system_theme; 
                ThemeSwitch.IsEnabled = false;
            }
            else 
            { ThemeSwitch.IsEnabled = true; ThemeSwitch_CheckedChanged(sender, e); }
            
        }


        // Dialog Controls
        void btnDISSettings_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
