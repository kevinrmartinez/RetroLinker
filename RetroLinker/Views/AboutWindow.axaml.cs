/*
    A .NET GUI application to help create desktop links of games running on RetroArch.
    Copyright (C) 2025  Kevin Rafael Martinez Johnston

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
using RetroLinker.Models;

namespace RetroLinker.Views;

public partial class AboutWindow : Window
{
    // TODO: Move some things into Bindings (0.8)
    private readonly AppInformation? appInfo;
    private readonly Contribs[] Contributors;
    private readonly Dictionary<string, Uri> ThirdPartyCredits = new();
    
    public AboutWindow()
    {
        InitializeComponent();
        appInfo = App.LocalInformation;
        BorderDev.IsVisible = false;
        
        // About the App
        var title = (string.IsNullOrWhiteSpace(appInfo?.Name)) ? nameof(AppInformation.Name) : appInfo?.Name;
        var version = (string.IsNullOrWhiteSpace(appInfo?.Version)) ? nameof(AppInformation.Version) : appInfo?.Version;
        LabelTitle.Content = $"{title} v{version}";

#if !RELEASE
        var buildDate = (appInfo?.BuildDate is not null) ? appInfo?.BuildDate.Value.ToString("s") : nameof(AppInformation.BuildDate);
        var gitCommitHash = (!string.IsNullOrEmpty(appInfo?.GitHash)) ?  appInfo?.GitHash : nameof(AppInformation.GitHash);
        LabelBuild.Text = $"{buildDate}; {gitCommitHash}";
        BorderDev.IsVisible = true;
#endif
        
        // Contributors (In order of arrival)
        // Contributors feel free to add their names and social media/contact/GitHub in this record array
        Contributors = [ 
            new Contribs("kevinrmartinez", new Uri("https://github.com/kevinrmartinez")),
            new Contribs("kevinrmartinez", new Uri("https://github.com/kevinrmartinez")),
            new Contribs("kevinrmartinez", new Uri("https://github.com/kevinrmartinez")),
            new Contribs("kevinrmartinez", new Uri("https://github.com/kevinrmartinez")),
        ];
        ItemsControlContributors.ItemsSource = Contributors;
        
        // Attribution
        // ThirdPartyCredits.Add("Unknown Icon", new Uri("https://www.flaticon.es/iconos-gratis/formas-y-simbolos")); Don't remember what this was
        ThirdPartyCredits.Add("Image placeholder icons created by JC Icon - Flaticon", new Uri("https://www.flaticon.com/free-icons/image-placeholder"));
        ThirdPartyCredits.Add("Flag Icons - IconBeast", new Uri("https://www.iconbeast.com/free-download-world-flag-icons/"));
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

public record Contribs(string DisplayName, Uri? OnlinePageUrl);