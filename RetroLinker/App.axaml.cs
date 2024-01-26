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

using System.Globalization;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using RetroLinker.Models;
using RetroLinker.ViewModels;
using RetroLinker.Views;

namespace RetroLinker;

public partial class App : Application
{
    public const string TimeTrace = "[Time]";
    public const string DebgTrace = "[Debg]";
    public const string InfoTrace = "[Info]";
    public const string WarnTrace = "[Warn]";
    public const string ErroTrace = "[Erro]";
    public const string RetroBin = "retroarch";
    public static System.DateTime LaunchTime = System.DateTime.Now;
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        System.TimeSpan launchTime = System.TimeSpan.FromTicks(LaunchTime.Ticks);
        System.Diagnostics.Trace.WriteLine($"AppLoadTime: {launchTime}", TimeTrace);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            LanguageManager.FixLocale(new CultureInfo("en-US"));
            System.Diagnostics.Debug.WriteLine("Iniciando MainWindow", DebgTrace);
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
