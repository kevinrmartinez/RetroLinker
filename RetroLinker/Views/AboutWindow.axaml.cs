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
    private readonly Dictionary<string, Uri> ThirdPartyCredits = new();
    
    public AboutWindow()
    {
        InitializeComponent();
        LabelBuild.IsVisible = false;
        
        // About the App
        var title = (string.IsNullOrWhiteSpace(App.AppName)) ? nameof(App.AppName) : App.AppName;
        var version = (string.IsNullOrWhiteSpace(App.AppVersion)) ? nameof(App.AppVersion) : App.AppVersion;
        LabelTitle.Content = $"{title} v{version}";

#if !RELEASE
        
        var buildDate = (App.AppBuildDate.HasValue) ? App.AppBuildDate.Value.ToString("s") : "BuildDate";
        var gitCommitHash = (!string.IsNullOrEmpty(App.AppCommitHash)) ?  App.AppCommitHash : "GitCommitHash";
        LabelBuild.Text = $"{buildDate}; {gitCommitHash}";
        LabelBuild.IsVisible = true;
#endif
        
        // Contributors (In order of arrival)
        // TODO: Replace with HyperlinkButtons pointing to the github account (>= 0.8)
        Contributors = [ "" ];
        ListBoxContributors.ItemsSource = Contributors;
        
        // Attribution
        // ThirdPartyCredits.Add("Unknown Icon", new Uri("https://www.flaticon.es/iconos-gratis/formas-y-simbolos")); Don't remember what this was
        ThirdPartyCredits.Add("Image placeholder icons created by JC Icon - Flaticon", new Uri("https://www.flaticon.com/free-icons/image-placeholder"));
        ThirdPartyCredits.Add("Flag Icons", new Uri("http://www.iconbeast.com"));
        foreach (var key in ThirdPartyCredits.Keys)
        {
            var hyperlink = new HyperlinkButton()
            {
                NavigateUri = ThirdPartyCredits[key],
                Content = key,
                Padding = new Thickness(0)
            };
            StackPanelCredits.Children.Add(hyperlink);
        }
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e) => this.Close();
}