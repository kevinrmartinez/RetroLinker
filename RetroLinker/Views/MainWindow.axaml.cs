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
    // Props
    public Settings Settings { get; set; }
    public string[] CoresList { get; set; }
    public object[] IconsList { get; set; }
    
    // Fields
    private UserControl PermaView;
    public bool IsDesigner = true;
    private readonly bool DesktopOS = System.OperatingSystem.IsWindows(); // temporal fix
    
    public MainWindow()
    {
        InitializeComponent();
        System.Diagnostics.Debug.WriteLine("Starting MainView...", App.DebgTrace);
        Settings = FileOps.LoadSettingsFO();
        CoresList = AvaloniaOps.GetCoresArray();
        IconsList = FileOps.LoadIcons(DesktopOS);
        PermaView = new MainView(this);
        MainCC1.Content = PermaView;
    }

    // Constructor for Designer
    public MainWindow(bool isDesigner)
    {
        InitializeComponent();
        IsDesigner = isDesigner;
        CoresList = AvaloniaOps.GetCoresArray();
        IconsList = FileOps.LoadIcons(DesktopOS);
        PermaView = new RenameEntryView();
        if (isDesigner) return;
        Settings = FileOps.LoadSettingsFO();
        PermaView = new MainView(this);
        MainCC1.Content = PermaView;
    }

    public enum ViewsTypes
    { MainView, PatchesView, SubsysView, AppendView }
    
    public void ChangeOut(ViewsTypes views, object[] currentValues)
    {
        MainCC1.Content = views switch
        {
            ViewsTypes.PatchesView => new PatchesView(this, (string)currentValues[0]),
            _ => PermaView
        };
    }

    public void LocaleReload(Settings locale)
    {
        if (LanguageManager.SetLocale(locale)) return;
        MainCC1.Content = null;
        PermaView = new MainView(this);
        MainCC1.Content = PermaView;
    }
    
    // TODO: Find a way to dispose of previous views (Maybe is not necessary?)
    public void ReturnToMainView(UserControl view)
    {
        MainCC1.Content = PermaView;
        view = null;
    }

    public void ReturnToMainView(PatchesView pView, string pString)
    {
        if (PermaView is not MainView permaView) return;
        permaView.UpdateLinkFromOutside(ViewsTypes.PatchesView, [pString]);
        MainCC1.Content = PermaView;
        pView = null;
    }
}
