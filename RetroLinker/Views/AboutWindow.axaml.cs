/*
    RetroLinker: A .NET GUI application to help create desktop links of games running on RetroArch.
    Copyright (C) 2023-2026 Kevin Rafael Martinez Johnston

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

using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RetroLinker.Models.IO;

namespace RetroLinker.Views;

public partial class AboutWindow : Window
{
    public AppInformation AppInfo { get; } = App.LocalInformation;
    public Contributor[] Contributors    => _contributors;
    public ThirdParty[] ThirdPartyLibs   => _thirdPartyLibs;
    public ThirdParty[] ThirdPartyRes    => _thirdPartyRes;
    public ThirdParty[] ThirdPartyThanks => _thirdPartyThanks;
    
    public AboutWindow()
    {
        InitializeComponent();
        BorderDev.IsVisible = false;
        
#if !RELEASE
        BorderDev.IsVisible = true;
#endif
        // Set Bindings
        DataContext = this;
    }

    private static readonly Uri LiberationUri = GetLiberationMonoUri();     // It's static because I just need to set this once per runtime
    private static Uri GetLiberationMonoUri()
    {
        try
        {
            var licenseFile = FileOps.LicensesFiles.First(fi => fi.Name.Contains("LiberationMono-"));
            var uriPath = new Uri(licenseFile.FullName);
            return uriPath;
        }
        catch (InvalidOperationException) {
            Logger.LogWarn("The local licence file for LiberationMono was not found, using online link");
            return new Uri("https://github.com/liberationfonts/liberation-fonts");
        }
        catch (Exception ex) {
            Logger.LogErro(ex);
            return new Uri("https://github.com/liberationfonts/liberation-fonts");
        }
    }
    
    // These fields below do not change on runtime, so they can be static
    
    // Contributors (In order of arrival)
    // Contributors feel free to add their names and social media/contact/GitHub in this record array
    private static readonly Contributor[] _contributors = [ 
#if DEBUG
        new Contributor("kevinrmartinez", new Uri("https://github.com/kevinrmartinez")),
        new Contributor("kevinrmartinez", new Uri("https://github.com/kevinrmartinez")),
        new Contributor("kevinrmartinez", new Uri("https://github.com/kevinrmartinez")),
        new Contributor("kevinrmartinez", new Uri("https://github.com/kevinrmartinez")),
        new Contributor("kevinrmartinez", new Uri("https://github.com/kevinrmartinez")),
        new Contributor("kevinrmartinez", new Uri("https://github.com/kevinrmartinez")),
        new Contributor("kevinrmartinez", new Uri("https://github.com/kevinrmartinez")),
        new Contributor("kevinrmartinez", new Uri("https://github.com/kevinrmartinez")),
#endif
    ];

    private static readonly ThirdParty[] _thirdPartyLibs =
    [
        new ThirdParty("AvaloniaUI", new Uri("https://avaloniaui.net/"), "Cross-platform UI framework"),
        new ThirdParty("MessageBox.Avalonia", new Uri("https://github.com/AvaloniaCommunity/MessageBox.Avalonia"), "Messagebox for AvaloniaUI"),
        new ThirdParty("SkiaSharp", new Uri("https://github.com/mono/SkiaSharp"), "2D graphics library; image processing"),
        new ThirdParty("SharpShellLink", new Uri("https://github.com/JeremyAnsel/SharpShellLink"), "A .NET Class Library for processing ShellLink (LNK) files"),
        new ThirdParty("Optris.Icons.Avalonia", new Uri("https://github.com/Optris/Optris.Icons.Avalonia"), "A library to easily display icons in an Avalonia App"),
        new ThirdParty("CommandLineParser", new Uri("https://github.com/mono/SkiaSharp"), "Terse syntax C# command line parser for .NET"),
        new ThirdParty("TG.INI", new Uri("https://github.com/mono/SkiaSharp"), "Reads and writes INI files and make navigating them easy"),
        new ThirdParty("Svg.Skia", new Uri("https://github.com/wieslawsoltes/Svg.Skia"), "An SVG rendering library"),
    ];
    
    private static readonly ThirdParty[] _thirdPartyRes =
    [
        // new ThirdParty("Unknown Icon", new Uri("https://www.flaticon.es/iconos-gratis/formas-y-simbolos")), // Can't remember what this was
        new ThirdParty("Libretro invader logo", new Uri("https://github.com/libretro/retroarch-assets")),
        new ThirdParty("Image placeholder icons created by JC Icon - Flaticon", new Uri("https://www.flaticon.com/free-icons/image-placeholder")),
        new ThirdParty("Flag Icons - IconBeast", new Uri("https://www.iconbeast.com/free-download-world-flag-icons/")),
        new ThirdParty("Liberation Mono Fonts", LiberationUri)
    ];
    
    private static readonly ThirdParty[] _thirdPartyThanks = [
        new ThirdParty("Zeronia.Diagnostics", new Uri("https://github.com/CrashInLine/Zeronia.Diagnostics"), "Legacy Avalonia DevTool for AvaloniaUI v12"),
        new ThirdParty("Xylocopadream.IconTool", new Uri("https://github.com/Zergoma/Xylocopadream.IconTool"), "Makes the icons of an app from one drawing (Makes a .ico with SkiaSharp)"),
    ];
}