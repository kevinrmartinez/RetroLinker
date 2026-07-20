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
using System.Reflection;
using Avalonia;
using Optris.Icons.Avalonia;
using Optris.Icons.Avalonia.FontAwesome7;
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
        IconProvider.Current.Register<FontAwesome7IconProvider>();
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
    private static readonly Assembly AppAssembly = typeof(Program).Assembly;
    private static readonly AssemblyName AppAssemblyName = AppAssembly.GetName();
    private static readonly string AppName = AppAssemblyName.Name ?? "N/A";
    // private static readonly string AppVersion = AppAssemblyName.Version?.ToString(3) ?? "N/A";
    private static readonly string AppVersionFull = AppAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "N/A";
    private static readonly string[] AppVersionSplit = AppVersionFull.Split('+');
    private static readonly string AppVersion = (AppVersionSplit.Length > 1)
                                                ? AppVersionSplit[Index.Start]
                                                : AppAssemblyName.Version?.ToString(3) ?? "N/A";
    
    
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
        /* Solution thanks to Gérald Barré (aka. meziantou)
         * https://www.meziantou.net/getting-the-date-of-build-of-a-dotnet-assembly-at-runtime.htm
         */
        var BuildDateAtt = AppAssembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .First(a => a.Key == "BuildDateUTC").Value;
        if (long.TryParse(BuildDateAtt, out var buildDateTicks)) {
            return DateTime.FromBinary(buildDateTicks);     // Date is set in UTC
        }
        
        Logger.LogWarn("App build bate was requested, but it could not be accessed");
        return null;
    }

    private static string? GetGitHashOfRepo() {
        if (AppVersionSplit.Length > 1) return AppVersionSplit[1];
        
        Logger.LogWarn("Hash of git repo was requested, but it could not be accessed");
        return null;
    }

    private static AppInformation GetAppInfo()
    {
        var fullName = AppAssemblyName.FullName;
        var buildDate = GetBuildDateOfAssembly();
        var gitHash = GetGitHashOfRepo();
        return new AppInformation(fullName, AppName, AppVersion, buildDate, gitHash);
    }
}
