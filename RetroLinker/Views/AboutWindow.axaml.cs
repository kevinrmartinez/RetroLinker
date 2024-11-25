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

using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace RetroLinker.Views;

public partial class AboutWindow : Window
{
    private readonly string[] Contributors;
    private Dictionary<string, Uri> OutsideCredits = new();
    
    public AboutWindow()
    {
        InitializeComponent();
        
        // About the App
        var title = (string.IsNullOrWhiteSpace(App.AppName)) ? nameof(App.AppName) : App.AppName;
        var version = (string.IsNullOrWhiteSpace(App.AppVersion)) ? nameof(App.AppVersion) : App.AppVersion;
        LabelTitle.Content = $"{title} v{version}";

        var buildDate = "BuildDate";
        // TODO: https://stackoverflow.com/questions/1600962/displaying-the-build-date
        var gitCommitHash = "GitCommitHash";
        // TODO: https://github.com/GitTools/GitVersion
        LabelBuild.Content = $"{buildDate}; {gitCommitHash}";
        
        // Contributors (In order of arrival) 
        // TODO: Replace with HyperlinkButtons pointing to the github account
        Contributors = ["kevinrmartinez"];
        ListBoxContributors.ItemsSource = Contributors;
        
        // Attribution
        OutsideCredits.Add("Unknown Icons", new Uri("https://www.flaticon.es/iconos-gratis/formas-y-simbolos"));
        OutsideCredits.Add("Flags", new Uri("http://www.iconbeast.com"));
        foreach (var key in OutsideCredits.Keys)
        {
            var hyperlink = new HyperlinkButton()
            {
                NavigateUri = OutsideCredits[key],
                Content = $"{key}: {OutsideCredits[key]}",
                Padding = new Thickness(0)
            };
            StackPanelCredits.Children.Add(hyperlink);
        }
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e) => this.Close();
}