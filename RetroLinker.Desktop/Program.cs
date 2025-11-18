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
using System.IO;
using Avalonia;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using RetroLinker.Models;

namespace RetroLinker.Desktop;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        SetUpLogger();
        appLogger?.LogInfo($"{AppName} v{AppVersion}");
        appLogger?.LogDebg($"Launch Time: {DateTime.Now:HH:mm:ss.fff}");
        
        appLogger?.LogDebg("Starting AvaloniaApp");
        #if DEBUG
        // If the Try-Catch is used during debugging, the program will successfully exit whenever something crashes,
        // Invalidating the purpose of the debugger lol
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
        #else
        // Try-Catch is used to print the Exception to log, and then close the log.
        try {
            // I think that every exception that happens while the app is running can be capture here, thrusting that 
            // the 'Program' class doesn't cause exceptions.
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e) {
            Trace.WriteLine(e, "[Erro]");
        }
        #endif
        
        // App Closing
        appLogger?.Close();
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        IconProvider.Current.Register<FontAwesomeIconProvider>();
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .AfterSetup(AppCallback)
#if DEBUG
            .LogToTrace()
#endif
            .WithInterFont();
    }

    private static void AppCallback(AppBuilder obj)
    {
        var instance = (App?)obj.Instance;
        instance?.SetAppInfo(GetAppInfo());
        instance?.SetLogger(appLogger);
    }

    // Parameters
    private static readonly System.Reflection.Assembly AppAssembly1 = typeof(Program).Assembly;
    private static readonly System.Reflection.AssemblyName AppAssembly2 = AppAssembly1.GetName();
    private static readonly string AppName = AppAssembly2.Name!;
    private static readonly string AppVersion = AppAssembly2.Version!.ToString(3);
    
    // Logging
    private static Logger? appLogger;
    private static readonly string LogFileName = $"{AppName}.log";
    private static readonly string LogFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LogFileName);
    private static readonly string LogFileBak = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{LogFileName}.bak");

    private static void SetUpLogger()
    {
        if (File.Exists(LogFile))
        {
            if (File.Exists(LogFileBak)) File.Delete(LogFileBak);
            File.Move(LogFile, LogFileBak);
        }
        
        appLogger = new Logger(LogFile) {
            AutoFlush =  true
        };
    }
    
    private static DateTime? GetBuildDateOfAssembly()
    {
        try
        {
            var assemblyFile = new FileInfo(AppAssembly1.Location);
            return assemblyFile.LastWriteTime;
        }
        catch (Exception e) {
            // TODO: Redirect to log
            Console.WriteLine(e);
            return null;
        }
    }

    private static string? GetGitHashOfRepo()
    {
        const string resourceName = "RetroLinker.Desktop.git-hash";
        const int sha1Length = 40;
        try
        {
            var result = ResourceLoader.GetTextFromResource(AppAssembly1, resourceName);
            string hash = string.Empty;
            foreach (var line in result) {
                if (line.Length != sha1Length) continue;
                hash = line;
                break;
            }
            return hash;
        }
        catch (Exception e) {
            // TODO: Redirect to log
            Console.WriteLine(e);
            return null;
        }
    }

    private static AppInfo GetAppInfo()
    {
        var fullName = AppAssembly2.FullName;
        var buildDate = GetBuildDateOfAssembly();
        var gitHash = GetGitHashOfRepo();
        return new AppInfo(fullName, AppName, AppVersion,  buildDate, gitHash);
    }
}
