/*
    RetroLinker: A .NET GUI application to help create desktop links of games running on RetroArch.
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
    public MainView? PermaView { get; private set; }
    public Settings Settings { get; private set; }
    public string[] CoresList { get; }
    public (List<string>, string?) IconsListEx { get; }
    
    // Fields
    public static readonly string OsDirSeparator = FileOps.OsDirSeparator.ToString();
    public bool IsDesigner = true;
    private readonly bool DesktopOS = System.OperatingSystem.IsWindows(); // temporal fix
    
    public MainWindow()
    {
        InitializeComponent();
        Settings = FileOps.LoadSettingsFO();
        CoresList = Operations.GetCoresArray();
        IconsListEx = FileOps.LoadIcons(DesktopOS);
        LanguageManager.SetLocale(Settings.LanguageLocale);
        PermaView = new MainView(this);
        ContBotton.Content = PermaView;
    }

    // Constructor for Designer
    public MainWindow(bool isDesigner)
    {
        InitializeComponent();
        IsDesigner = isDesigner;
        CoresList = [];
        IconsListEx = (new List<string>(), null);
        // Settings = FileOps.LoadDesignerSettingsFO(true);
        Settings = new Settings();
        PermaView = null;
        if (isDesigner) return;
        
        // This is needed because of an edge case with the designer (can't remember witch)
        Settings = FileOps.LoadSettingsFO();
        LanguageManager.SetLocale(Settings.LanguageLocale);
        PermaView = new MainView(this);
        ContBotton.Content = PermaView;
    }

    public MainWindow(MainView mainViewDesigner) : this(true) {
        mainViewDesigner.Name = "MainViewDesigner";
        CoresList = Operations.GetCoresArray();
        IconsListEx = FileOps.LoadIcons(DesktopOS);
    }

    public void SetSettings(Settings settings) => Settings = settings;
    
    public void ChangeOut(MainViewTypes views)
    {
        ContBotton.IsTransitionReversed = false;
        try
        {
            ContBotton.Content = views switch {
                MainViewTypes.AppendView => new AppendView(this),
                MainViewTypes.PatchesView => new PatchesView(this),
                MainViewTypes.SubsysView => new SubsystemsView(this),
                _ => PermaView
            };
        }
        catch (System.InvalidCastException ex) {
            _ = this.PopUpGenericError(ex);
            GoBackToMainView();
        }
    }

    public void LocaleReload(string locale)
    {
        // If ContBotton doesn't drop its transition during locale refresh, both transitions play at the same time
        if (LanguageManager.SetLocale(locale)) return;
        
        var ogTransition = ContBotton.PageTransition;
        ContTop.Content = null;
        PermaView = new MainView(this);
        ContBotton.PageTransition = null;
        ContBotton.Content = PermaView;
        ContTop.Content = ContBotton;
        ContBotton.PageTransition = ogTransition;
    }
    
    private void GoBackToMainView() {
        // Disposal of views only required if the view has native resources
        // https://github.com/AvaloniaUI/Avalonia/discussions/6556
        ContBotton.IsTransitionReversed = true;
        ContBotton.Content = PermaView;
    }
    
    public void ReturnToMainView() => GoBackToMainView();

    public void ReturnToMainView(UserControl view, string args)
    {
        if (PermaView is not { } mainView) return;
        
        GoBackToMainView();
        switch (view)
        {
            case AppendView:
                mainView.BuildingLink.CONFappend = args;
                break;
            case PatchesView:
                mainView.BuildingLink.PatchArg = args;
                break;
            case SubsystemsView:
                mainView.BuildingLink.SubsysArg = args;
                break;
            default:
                // Should not happen
                var viewType = view.GetType();
                Logger.LogWarn($"A view of type '{viewType}' tried to update arguments");
                break;
        }
    }
}

public enum MainViewTypes
{ MainView, PatchesView, SubsysView, AppendView }

public enum TextBoxActions { Restore, Clear }
