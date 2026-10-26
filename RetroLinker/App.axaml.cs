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

global using System;                            // General usage methods, exceptions, types like Date or URI...
global using System.Collections.Generic;        // List<T>
global using System.Collections.ObjectModel;    // Collection<T>
global using System.Diagnostics.CodeAnalysis;   // Advance members attributes like DynamicDependency and MemberNotNullWhen
global using System.Runtime.Versioning;         // SupportedOSPlatformAttribute
global using System.Threading.Tasks;            // Async operations

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using RetroLinker.Models.Avalonia;
using RetroLinker.Models.IO;
using RetroLinker.Models.Settings;
using RetroLinker.Views;

namespace RetroLinker;

public class App : Application
{
    // TODO: Give the singleton treatment?
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
                Title = LocalInformation.ToString(),
                DataContext = null,
            };
        }
        else {
            // This is left out because it shouldn't happen
            // singleViewPlatform.MainView = new MainView
            // {
            //     DataContext = new MainViewModel()
            // };
            Logger.LogCrit($"On '{nameof(App)}.{nameof(OnFrameworkInitializationCompleted)}'");
            Logger.LogCrit($"\tType of {nameof(ApplicationLifetime)} is {ApplicationLifetime?.GetType().Name ?? "NULL"}!");
        }

        base.OnFrameworkInitializationCompleted();
    }

    // Added
    public void SetAppInfo(AppInformation appInformation) {
        LocalInformation = appInformation;
        Logger.LogDebg("'AppInfo' has been set with the following properties:\n" + LocalInformation.ToStringLines());
    }

    private void CompleteStartup()
    {
        Dispatcher.UnhandledException += App_OnUnhandledException;
        LanguageManager.FixLocale(LanguageManager.ENLocale);
        UpdateLicenseUri();
    }

    private Exception? _catchException;
    private async void App_OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
        e.Handled = true;
        _catchException = e.Exception;
        await Dispatcher.UIThread.InvokeAsync(ShowCriticalAndQuit, DispatcherPriority.MaxValue);
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

    private async Task ShowCriticalAndQuit()
    {
        try
        {
            if (_catchException is not { } criticalException) {
                var errMess = $"'{nameof(ShowCriticalAndQuit)}' was called, but '{nameof(_catchException)}' is not set!";
                throw new ArgumentException(errMess);
            }
            await this.PopUpCriticalError(criticalException);
        }
        catch (Exception ex)
        {
            Logger.LogCrit("An error occured while trying to show a critical message:");
            Logger.LogCrit(ex);
        }
        finally { Environment.Exit(1); }
    }

    public async Task ShowCriticalAndQuit(Exception criticalException) {
        _catchException = criticalException;
        await ShowCriticalAndQuit();
    }
}
