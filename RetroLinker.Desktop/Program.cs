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
using System.Linq;
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
        Logger.LogInfo($"{AppName} v{AppVersion}");
        // Logger.LogDebg($"Launch Time: {DateTime.Now:HH:mm:ss.fff}");
        
        Logger.LogDebg("Starting AvaloniaApp");
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
            Logger.LogErro($"{AppName} has crashed to desktop with the following error:");
            Logger.LogError(e);
        }
        #endif
        
        // App Closing
        Logger.Close();
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
        // The 'LocalInformation' prop of the 'App' class should be filled before using FileOps from here
        instance?.SetAppInfo(GetAppInfo());
        SetUpLogger();
    }

    // Parameters
    private static readonly System.Reflection.Assembly AppAssembly = typeof(Program).Assembly;
    private static readonly System.Reflection.AssemblyName AppAssemblyName = AppAssembly.GetName();
    private static readonly string AppName = AppAssemblyName.Name ?? "N/A";
    private static readonly string AppVersion = AppAssemblyName.Version?.ToString(3) ?? "N/A";
    
    // Logging
    private static readonly string LogFileName = $"{AppName}.log";
    // private static readonly string LogFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LogFileName);
    private static readonly string LogFileBak = $"{LogFileName}.bak";

    private static void SetUpLogger()
    {
        Logger.SetLogFile(LogFileName);
        var bakFullName = FileOps.CombineMultipleInputs(FileOps.GetDirFromPath(Logger.LogFile) ?? FileOps.BaseDir, LogFileBak);
        if (FileOps.PathAlreadyExists(Logger.LogFile)) {
            if (FileOps.PathAlreadyExists(bakFullName)) FileOps.FileDeletion(bakFullName);
            FileOps.FileMoving(Logger.LogFile, bakFullName);
        }

        Logger.AutoFlush = true;
    }
    
    private static DateTime? GetBuildDateOfAssembly()
    {
        try
        {
            var assemblyFile = FileOps.GetFileInfo(AppAssembly.Location);
            return assemblyFile.LastWriteTime;
        }
        catch (Exception e) {
            Logger.LogWarn("App Build Date was requested, but it could not be accessed");
            Logger.LogErro(e);
            return null;
        }
    }

    private static string? GetGitHashOfRepo()
    {
        const string resourceName = "RetroLinker.Desktop.git-hash";
        const int sha1Length = 40;
        try
        {
            var result = ResourceLoader.GetTextLinesFromResource(AppAssembly, resourceName);
            var hash = result.First(line => line.Length == sha1Length);
            return hash;
        }
        catch (Exception e) {
            Logger.LogWarn("The hash of the git repo this build is based on was requested, but it could not be accessed");
            Logger.LogErro(e);
            return null;
        }
    }

    private static AppInfo GetAppInfo()
    {
        var fullName = AppAssemblyName.FullName;
        var buildDate = GetBuildDateOfAssembly();
        var gitHash = GetGitHashOfRepo();
        return new AppInfo(fullName, AppName, AppVersion,  buildDate, gitHash);
    }
}
