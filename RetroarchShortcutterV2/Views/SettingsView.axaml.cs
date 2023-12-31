/*
    A .NET GUI application to help create desktop links of games running on RetroArch.
    Copyright (C) 2023  Kevin Rafael Martinez Johnston

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;

namespace RetroLinker.Views
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
            // El designer de Avalonia se rompe en esta parte
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
                // El designer de Avalonia se rompe en esta parte
                Application.Current.RequestedThemeVariant = system_theme;
                ThemeSwitch.IsEnabled = false;
                ParentWindow.settings.PreferedTheme = 0;
            }
            else
            { ThemeSwitch.IsEnabled = true; ThemeSwitch_CheckedChanged(sender, e); }
        }

        void View1ChecksHandle(object? sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox) == null) return;
            ParentWindow.settings.PrevConfig = (bool)chkPrevCONFIG.IsChecked;
            ParentWindow.settings.CpyUserIcon = (bool)chkCpyUserIcon.IsChecked;
        }
        
        // UNLOAD
        void SettingsView1_OnUnloaded(object? sender, RoutedEventArgs e)
        {
            _ = e.ToString();
        }
        
#if DEBUG
        void SettingsView1_Loaded2(object sender, RoutedEventArgs e)
        {
            _ = sender.ToString();
        }
#endif
        
    }
}
