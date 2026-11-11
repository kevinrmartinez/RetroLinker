/*
    RetroLinker: A .NET GUI application to help create desktop links of games running on RetroArch.
    Copyright (C) 2023-2026 Kevin Rafael Martinez Johnston

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
using RetroLinker.Models.Settings;
using RetroLinker.Styles;

namespace RetroLinker.Views;

public partial class SettingsView : UserControl
{
    // Window Obj
    // private MainWindow MainAppWindow;
    public SettingsWindow ParentWindow { get; }

    public SettingsView() {
        InitializeComponent();
        ParentWindow = new SettingsWindow(true);
        CompleteConstructor();
    }

    public SettingsView(SettingsWindow parentWindow)
    {
        InitializeComponent();
        ParentWindow = parentWindow;
        CompleteConstructor();
    }

    private void CompleteConstructor()
    {
        FillComboLocale();
        LoadTheme(ParentWindow.NewSettings.ChosenTheme);
        comboLocale.SelectedIndex = LanguageManager.GetLocaleIndex(ParentWindow.NewSettings);
    }

    // PROPS/STATICS
    private static readonly ThemeVariant DarkTheme = ThemeVariant.Dark;
    private static readonly ThemeVariant LightTheme = ThemeVariant.Light;
    private static readonly ThemeVariant SystemTheme = ThemeVariant.Default;

    #region Loading
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
    }
    #endregion

    // Appearance
    void LoadTheme(byte themeCode)
    {
        // Avalonia's Designer gets borked on this part; find an alternative do this on DEBUG, or a designer specific code
        switch (themeCode)
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

    public void SetCurrentTheme(byte themeCode) => LoadTheme(themeCode);

    void ThemeSwitch_CheckedChanged(object sender, RoutedEventArgs e)
    {
        // Avalonia's Designer gets borked on this part
        if (swtThemeSwitch.IsChecked.GetValueOrDefault())
        {
            Application.Current?.RequestedThemeVariant = DarkTheme;
            ParentWindow.NewSettings.ChosenTheme = 2;
        }
        else
        {
            Application.Current?.RequestedThemeVariant = LightTheme;
            ParentWindow.NewSettings.ChosenTheme = 1;
        }
    }

    void ThemeDefault_CheckedChanged(object sender, RoutedEventArgs e)
    {
        if (chkThemeDefault.IsChecked.GetValueOrDefault())
        {
            // Avalonia's Designer breaks on this part
            Application.Current!.RequestedThemeVariant = SystemTheme;
            swtThemeSwitch.IsEnabled = false;
            ParentWindow.NewSettings.ChosenTheme = 0;
        }
        else {
            swtThemeSwitch.IsEnabled = true;
            ThemeSwitch_CheckedChanged(sender, e);
        }
    }


    private void BtnLocale_OnClick(object? sender, RoutedEventArgs e) {
        // REWRITE: Rewrite so this button shouldn't be necessary
        var locale = LanguageManager.ResolveLocale(comboLocale.SelectedIndex);
        ParentWindow.NewSettings.LanguageLocale = locale.Culture.Name;
    }
}