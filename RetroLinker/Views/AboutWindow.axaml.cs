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
    public AppInformation AppInfo { get; }
    public Contribs[] Contributors { get; }
    public Dictionary<string, Uri> ThirdPartyCredits { get; } = new();
    
    private readonly Dictionary<string, Uri> _thirdPartyCredits = new();
    
    public AboutWindow()
    {
        InitializeComponent();
        AppInfo = App.LocalInformation;
        BorderDev.IsVisible = false;
        
#if !RELEASE
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
        // ItemsControlContributors.ItemsSource = _contributors;
        
        // Attribution
        // TODO: Move to Binding
        // ThirdPartyCredits.Add("Unknown Icon", new Uri("https://www.flaticon.es/iconos-gratis/formas-y-simbolos")); Don't remember what this was
        _thirdPartyCredits.Add("Image placeholder icons created by JC Icon - Flaticon", new Uri("https://www.flaticon.com/free-icons/image-placeholder"));
        _thirdPartyCredits.Add("Flag Icons - IconBeast", new Uri("https://www.iconbeast.com/free-download-world-flag-icons/"));
        foreach (var key in _thirdPartyCredits.Keys)
        {
            var hyperlink = new HyperlinkButton()
            {
                NavigateUri = _thirdPartyCredits[key],
                Content = key,
                Padding = new Thickness(0)
            };
            StackPanelCredits.Children.Add(hyperlink);
        }
        DataContext = this;
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e) => this.Close();
}

public record Contribs(string DisplayName, Uri? OnlinePageUrl);