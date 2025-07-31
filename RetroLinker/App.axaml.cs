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
using System.Globalization;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using RetroLinker.Models;
using RetroLinker.Views;

namespace RetroLinker;

public class App : Application
{
    public string AppFullName { get; private set; } = string.Empty;
    public static string AppName { get; private set; } = string.Empty;
    public static string AppVersion { get; private set; } = string.Empty;
    public static DateTime? AppBuildDate { get; private set; }
    public static string? AppCommitHash { get; private set; }
    
    public const string TimeTrace = "[Time]";
    public const string DebgTrace = "[Debg]";
    public const string InfoTrace = "[Info]";
    public const string WarnTrace = "[Warn]";
    public const string ErroTrace = "[Erro]";
    public const string RetroBin = "retroarch";
    public static readonly System.Diagnostics.Stopwatch StopWatch = new();
    
    public static string[]? Args;
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        StopWatch.Start();
        System.Diagnostics.Debug.WriteLine($"App Launched at: {DateTime.Now:HH:mm:ss.fff}", TimeTrace);
        
    }

    public override void OnFrameworkInitializationCompleted()
    {
        const string defaultLocale = "en_US";
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Args = desktop.Args;
            LanguageManager.FixLocale(new CultureInfo(defaultLocale));
            System.Diagnostics.Debug.WriteLine("Starting MainWindow...", DebgTrace);
            desktop.MainWindow = new MainWindow
            {
                Title = $"{AppName} v{AppVersion}",
                DataContext = null,
                IsDesigner = false
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            // This is left out because it shouldn't happen
            // singleViewPlatform.MainView = new MainView
            // {
            //     DataContext = new MainViewModel()
            // };
            System.Diagnostics.Debug.WriteLine("How do I got here...?", DebgTrace);
            System.Diagnostics.Debug.WriteLine(singleViewPlatform.ToString());
            System.Diagnostics.Trace.WriteLine("Something unexpected happened, terminating...", ErroTrace);
        }

        base.OnFrameworkInitializationCompleted();
    }

    public void SetAppInfo(AppInfo appInfo)
    {
        AppFullName = appInfo.FullName;
        AppName = appInfo.Name;
        AppVersion = appInfo.Version;
        AppBuildDate = appInfo.BuildDate;
        AppCommitHash = appInfo.GitHash;
    }
}
