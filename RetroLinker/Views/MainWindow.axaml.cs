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
using RetroLinker.Models.Avalonia;

namespace RetroLinker.Views;

public partial class MainWindow : Window
{
    // Props
    public Settings Settings { get; }
    public string[] CoresList { get; }
    public object[] IconsList { get; }
    
    // Fields
    private UserControl PermaView;
    public bool IsDesigner = true;
    // TODO: MainView should inherit this
    private readonly bool DesktopOS = System.OperatingSystem.IsWindows(); // temporal fix
    
    public MainWindow()
    {
        InitializeComponent();
        Settings = FileOps.LoadSettingsFO();
        CoresList = Operations.GetCoresArray();
        IconsList = FileOps.LoadIcons(DesktopOS);
        LanguageManager.SetLocale(Settings.LanguageLocale);
        PermaView = new MainView(this);
        MainCC1.Content = PermaView;
    }

    // Constructor for Designer
    public MainWindow(bool isDesigner)
    {
        InitializeComponent();
        IsDesigner = isDesigner;
        CoresList = [];
        IconsList = [];
        // Settings = FileOps.LoadDesignerSettingsFO(true);
        Settings = new Settings();
        PermaView = new RenameEntryView();
        if (isDesigner) return;
        
        // This is needed because of an edge case with the designer (can't remember witch)
        Settings = FileOps.LoadSettingsFO();
        LanguageManager.SetLocale(Settings.LanguageLocale);
        PermaView = new MainView(this);
        MainCC1.Content = PermaView;
    }

    public MainWindow(MainView mainViewDesigner) : this(true)
    {
        mainViewDesigner.Name = "MainViewDesigner";
        CoresList = Operations.GetCoresArray();
        IconsList = FileOps.LoadIcons(DesktopOS);
    }

    public enum ViewsTypes
    { MainView, PatchesView, SubsysView, AppendView }
    
    public void ChangeOut(ViewsTypes views, object[] currentValues)
    {
        MainCC1.Content = views switch
        {
            ViewsTypes.PatchesView => new PatchesView(this, (string)currentValues[0]),
            ViewsTypes.AppendView => new AppendView(this,  (string)currentValues[0]),
            _ => PermaView
        };
    }

    public void LocaleReload(string locale)
    {
        if (LanguageManager.SetLocale(locale)) return;
        MainCC1.Content = null;
        PermaView = new MainView(this);
        MainCC1.Content = PermaView;
    }
    
    // TODO: Find a way to dispose of previous views (Maybe is not necessary?)
    public void ReturnToMainView() => MainCC1.Content = PermaView;

    public void ReturnToMainView(PatchesView pView, string pString)
    {
        if (PermaView is not MainView permaView) return;
        permaView.UpdateLinkFromOutside(ViewsTypes.PatchesView, [pString]);
        MainCC1.Content = PermaView;
        // pView = null;
    }

    public void ReturnToMainView(AppendView aView, string aString)
    {
        if (PermaView is not MainView permaView) return;
        permaView.UpdateLinkFromOutside(ViewsTypes.AppendView, [aString]);
        MainCC1.Content = PermaView;
        // aView = null;
    }
}
