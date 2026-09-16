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

using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using RetroLinker.Models;
using RetroLinker.Views;

namespace RetroLinker;

public class App : Application
{
    public static AppInformation LocalInformation { get; private set; } 
        = new(string.Empty, string.Empty,  string.Empty, false);
    
    public const string RetroBin = "retroarch";
    public const string PlatformWin = "windows";
    public const string PlatformLin = "linux";
    
    public static string[]? Args;
    
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            CompleteStartup();
            Args = desktop.Args;
            desktop.MainWindow = new MainWindow(false)
            {
                Title = $"{LocalInformation.Name} v{LocalInformation.Version}",
                DataContext = null,
            };
        }
        else {
            // This is left out because it shouldn't happen
            // singleViewPlatform.MainView = new MainView
            // {
            //     DataContext = new MainViewModel()
            // };
            Logger.LogCrit($"{nameof(App)}.{nameof(OnFrameworkInitializationCompleted)}");
            Logger.LogCrit($"Type of {nameof(ApplicationLifetime)} is {ApplicationLifetime?.GetType().Name ?? "NULL"}!");
        }

        base.OnFrameworkInitializationCompleted();
    }

    // Added
    public void SetAppInfo(AppInformation appInformation) {
        LocalInformation = appInformation;
        Logger.LogDebg("'AppInfo' has been set with the following properties:\n" + LocalInformation.ToStringLines());
    }

    private void CompleteStartup() {
        LanguageManager.FixLocale(LanguageManager.ENLocale);
        UpdateLicenseUri();
    }
    
    private void UpdateLicenseUri()
    {
        if (Current is not { } app) return;
        var licenseUri = app.Resources["LicenseUri"] as Uri;
        if (licenseUri is null) return;
        var localLicenseUri = licenseUri;
        try
        {
            var licenseFiles = FileOps.LicensesFiles;
            string? licenseFile = null;
            foreach (var file in licenseFiles) {
                if (!file.Name.Contains("COPYING")) continue;
                licenseFile = file.FullName;
                break;
            }
            if (!string.IsNullOrEmpty(licenseFile)) localLicenseUri = new Uri(licenseFile);
        }
        catch (Exception ex)
        {
            Logger.LogErro($"{nameof(App)}.{nameof(UpdateLicenseUri)}:)");
            Logger.LogErro(ex);
            localLicenseUri = new Uri("https://github.com/liberationfonts/liberation-fonts");
        }
        
        app.Resources["LicenseUri"] = localLicenseUri;
    }
}
