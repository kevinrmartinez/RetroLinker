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
using RetroLinker.Models;
using RetroLinker.Styles;

namespace RetroLinker.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
            ParentWindow = new SettingsWindow();
        }

        public SettingsView(MainWindow _mainWindow, SettingsWindow parentWindow, bool desktopOs)
        {
            InitializeComponent();
            MainAppWindow = _mainWindow;
            ParentWindow = parentWindow;
            DesktopOS = desktopOs;
            // settings = ParentWindow.settings;
        }
        
        // Window Obj
        private MainWindow MainAppWindow;
        private SettingsWindow ParentWindow;

        // PROPS/STATICS
        private bool FirstTimeLoad = true;
        private bool DesktopOS;
        // Consider using independent Settings references
        // private Settings settings; 
        static readonly ThemeVariant dark_theme = ThemeVariant.Dark;
        static readonly ThemeVariant light_theme = ThemeVariant.Light;
        static readonly ThemeVariant system_theme = ThemeVariant.Default;

        #region Loads
        // LOADS
        void View_OnLoaded(object sender, RoutedEventArgs e)
        { 
            System.Diagnostics.Debug.WriteLine($"SettingView loaded for the fist time? {FirstTimeLoad}", App.DebgTrace);
            if (FirstTimeLoad) FillComboLocale();
            
            // Settings
            ApplySettingsToControls();
        }

        void ApplySettingsToControls()
        { 
            chkPrevCONFIG.IsChecked = ParentWindow.settings.PrevConfig;
            chkCpyUserIcon.IsChecked = ParentWindow.settings.CpyUserIcon;
            LoadTheme(ParentWindow.settings.PreferedTheme);
            comboLocale.SelectedIndex = LanguageManager.GetLocaleIndex(ParentWindow.settings);
        }

        void FillComboLocale()
        {
            int index = (comboLocale.SelectedIndex >= 0) ? comboLocale.SelectedIndex : 0;
            // LanguageManager.EnglishItem.ItemIndex = 0;
            foreach (var languageItem in LanguageManager.LanguageList)
            {
                languageItem.ItemIndex = index;
                comboLocale.Items.Add(LocaleComboItem.GetLocaleComboItem(languageItem));
                index++;
            }
            FirstTimeLoad = false;
        }
        #endregion

        // APARIENCE
        // TODO: Refactorize using an event, possibly based in the theme byte
        void LoadTheme(byte ThemeCode)
        {
            // Avalonia's Designer gets borked on this part; find an alternative do this on DEBUG, or a designer specific code
            switch (ThemeCode)
            {
                case 1: // Light
                    swtThemeSwitch.IsChecked = false;
                    break;
                case 2: // Dark
                    swtThemeSwitch.IsChecked = true;
                    break;
                default:// Any other case (0 = System Theme)
                    chkThemeDefault.IsChecked = true;
                    break;
            }
        }
        
        void ThemeSwitch_CheckedChanged(object sender, RoutedEventArgs e)
        {
            // Avalonia's Designer gets borked on this part
            if ((bool)swtThemeSwitch.IsChecked)
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
            if ((bool)chkThemeDefault.IsChecked)
            {
                // Avalonia's Designer gets borked on this part
                Application.Current.RequestedThemeVariant = system_theme;
                swtThemeSwitch.IsEnabled = false;
                ParentWindow.settings.PreferedTheme = 0;
            }
            else
            { swtThemeSwitch.IsEnabled = true; ThemeSwitch_CheckedChanged(sender, e); }
        }
        
        
        
        private void BtnLocale_OnClick(object? sender, RoutedEventArgs e)
        {
            var locale = LanguageManager.ResolveLocale(comboLocale.SelectedIndex);
            try
            { ParentWindow.settings.SetLanguage(locale); }
            catch
            { ParentWindow.settings.SetDefaultLaunguage(); }
        }

        // OTHER PREFERENCES
        void View1ChecksHandle(object? sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox) == null) return;
            ParentWindow.settings.PrevConfig = (bool)chkPrevCONFIG.IsChecked;
            ParentWindow.settings.CpyUserIcon = (bool)chkCpyUserIcon.IsChecked;
        }
        
#if DEBUG
        void SettingsView1_Loaded2(object sender, RoutedEventArgs e)
        {
            _ = sender.ToString();
        }
        // UNLOAD
        void SettingsView1_OnUnloaded(object? sender, RoutedEventArgs e)
        {
            _ = e.ToString();
        }
#endif
    }
}
