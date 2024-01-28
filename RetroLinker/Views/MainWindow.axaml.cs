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

using Avalonia.Controls;
using RetroLinker.Models;

namespace RetroLinker.Views;

public partial class MainWindow : Window
{
    private MainView PermaView;
    public MainWindow()
    {
        InitializeComponent();
        PermaView = new MainView(this);
        MainCC1.Content = PermaView;
    }

    public enum Window1Views1
    {
        mainView, SettingsView1, SettingsView2
    }
    
    public enum Window1Views2
    {
        PatchesView
    }
    
    private void ChangeContent(Window1Views1 views)
    {
        
    }

    public void LocaleReload(Settings locale)
    {
        if (LanguageManager.SetLocale(locale)) return;
        MainCC1.Content = null;
        PermaView = new MainView(this);
        MainCC1.Content = PermaView;
    }

    public void ReturnToMainView(UserControl view)
    {
        MainCC1.Content = PermaView;
        view = null;
    }
}
