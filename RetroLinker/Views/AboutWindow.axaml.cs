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
    public Contributor[] Contributors { get; }
    public ThirdParty[] ThirdPartyLibs { get; }
    public ThirdParty[] ThirdPartyRes { get; }
    public ThirdParty[] ThirdPartyThanks { get; }
    
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
            new Contributor("kevinrmartinez", new Uri("https://github.com/kevinrmartinez")),
            new Contributor("kevinrmartinez", new Uri("https://github.com/kevinrmartinez")),
            new Contributor("kevinrmartinez", new Uri("https://github.com/kevinrmartinez")),
            new Contributor("kevinrmartinez", new Uri("https://github.com/kevinrmartinez")),
            new Contributor("kevinrmartinez", new Uri("https://github.com/kevinrmartinez")),
            new Contributor("kevinrmartinez", new Uri("https://github.com/kevinrmartinez")),
            new Contributor("kevinrmartinez", new Uri("https://github.com/kevinrmartinez")),
            new Contributor("kevinrmartinez", new Uri("https://github.com/kevinrmartinez")),
        ];
        
        // Used Libraries
        ThirdPartyLibs =
        [
            new ThirdParty("AvaloniaUI", new Uri("https://avaloniaui.net/"), "Cross-platform UI framework"),
            new ThirdParty("Magick.NET", new Uri("https://github.com/dlemstra/Magick.NET"), "Powerful image manipulation library"),
            new ThirdParty("SkiaSharp", new Uri("https://github.com/mono/SkiaSharp"), "2D graphics library; image processing"),
            new ThirdParty("MessageBox.Avalonia", new Uri("https://github.com/AvaloniaCommunity/MessageBox.Avalonia"), "Messagebox for AvaloniaUI"),
            new ThirdParty("Microsoft.ClearScript.Windows", new Uri("https://github.com/ClearFoundry/ClearScript")),
            new ThirdParty("Optris.Icons.Avalonia", new Uri("https://github.com/Optris/Optris.Icons.Avalonia"), "A library to easily display icons in an Avalonia App")
        ];
        
        // Attribution
        ThirdPartyRes = [
            // new ThirdParty("Unknown Icon", new Uri("https://www.flaticon.es/iconos-gratis/formas-y-simbolos")),
            new ThirdParty("Image placeholder icons created by JC Icon - Flaticon", new Uri("https://www.flaticon.com/free-icons/image-placeholder")),
            new ThirdParty("Flag Icons - IconBeast", new Uri("https://www.iconbeast.com/free-download-world-flag-icons/"))
        ];
        
        // Special Thanks
        ThirdPartyThanks = [
            new ThirdParty("Zeronia.Diagnostics", new Uri("https://github.com/CrashInLine/Zeronia.Diagnostics"), "DevTool for Avalonia v12"),
        ];
        
        // =Update Bindings=
        DataContext = this;
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e) => this.Close();
}

public record Contributor(string DisplayName, Uri? OnlinePageUrl);
public record ThirdParty(string DisplayName, Uri OnlinePageUrl, string? Comment = null);