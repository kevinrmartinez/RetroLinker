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
    
    public static string[]? Args;
    
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Args = desktop.Args;
            LanguageManager.FixLocale(LanguageManager.ENLocale);
            desktop.MainWindow = new MainWindow
            {
                Title = $"{LocalInformation.Name} v{LocalInformation.Version}",
                DataContext = null,
                IsDesigner = false
            };
        }
        else {
            // This is left out because it shouldn't happen
            // singleViewPlatform.MainView = new MainView
            // {
            //     DataContext = new MainViewModel()
            // };
        }

        base.OnFrameworkInitializationCompleted();
    }

    // Added
    public void SetAppInfo(AppInformation appInformation) {
        LocalInformation = appInformation;
        Logger.LogDebg("'AppInfo' has been set with the following properties:\n" + LocalInformation.ToStringLines());
    }
}
