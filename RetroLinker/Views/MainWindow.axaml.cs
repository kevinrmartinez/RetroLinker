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

using System.Collections.Generic;
using Avalonia.Controls;
using RetroLinker.Models;
using RetroLinker.Models.Avalonia;

namespace RetroLinker.Views;

public partial class MainWindow : Window
{
    // Props
    public Settings Settings { get; }
    public string[] CoresList { get; }
    public (List<string>, System.Exception?) IconsList { get; }
    
    // Fields
    private UserControl PermaView;
    public bool IsDesigner = true;
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
        IconsList = (new List<string>(), null);
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

    // TODO: Move outside of MainWindow, can be this same file
    public enum ViewsTypes
    { MainView, PatchesView, SubsysView, AppendView }
    
    public void ChangeOut(ViewsTypes views, object currentValue)
    {
        try
        {
            MainCC1.Content = views switch {
                ViewsTypes.AppendView => new AppendView(this,  (string)currentValue),
                ViewsTypes.PatchesView => new PatchesView(this, (string)currentValue),
                ViewsTypes.SubsysView => new SubsystemsView(this, (SubsystemReq)currentValue),
                _ => PermaView
            };
        }
        catch (System.InvalidCastException ex) {
            App.Logger?.LogErro(ex.Message);
            // TODO: Show an error PopUp
            MainCC1.Content = PermaView;
        }
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

    public void ReturnToMainView(UserControl view, string args)
    {
        var viewType = view switch {
            AppendView => ViewsTypes.AppendView,
            PatchesView => ViewsTypes.PatchesView,
            SubsystemsView => ViewsTypes.SubsysView,
            _ => ViewsTypes.MainView
        };
        if (PermaView is not MainView permaView) return;
        
        MainCC1.Content = PermaView;
        permaView.UpdateLinkFromOutside(viewType, args);
        
        // view = null;
    }
}
