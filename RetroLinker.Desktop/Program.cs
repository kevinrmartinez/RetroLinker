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
using System.Linq;
using System.Reflection;
using Avalonia;
using Optris.Icons.Avalonia;
using Optris.Icons.Avalonia.FontAwesome7;
using RetroLinker.Models;

namespace RetroLinker.Desktop;

class Program
{
    // https://anthonysimmon.com/programmatically-elevate-dotnet-app-on-any-platform/
    // pkexec
    // Fields
    private const string KeyBuildDate = "BuildDateUTC";
    private static readonly Assembly AppAssembly = typeof(Program).Assembly;
    private static readonly AssemblyName AppAssemblyName = AppAssembly.GetName();
    private static readonly string AppName = AppAssemblyName.Name ?? "N/A";
    // private static readonly string AppVersion = AppAssemblyName.Version?.ToString(3) ?? "N/A";
    private static readonly string AppVersionFull = AppAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "N/A";
    private static readonly string[] AppVersionSplit = AppVersionFull.Split('+');
    private static readonly string AppVersion = (AppVersionSplit.Length > 1)
        ? AppVersionSplit[Index.Start]
        : AppAssemblyName.Version?.ToString(3) ?? "N/A";
    
    private static readonly string LogFileName = $"{AppName}.log";
    // private static readonly string LogFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LogFileName);
    private static readonly string LogFileBak = $"{LogFileName}.bak";
    private const string LogDebug = "debug";

    #region APP

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        SetUpLogger();
        Logger.LogInfo($"{AppName} v{AppVersion}");
        
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
            Logger.LogErro(e);
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

    private static void AppCallback(AppBuilder obj) {
        var instance = (App?)obj.Instance;
        instance?.SetAppInfo(GetAppInfo());
    }

    #endregion
    
    // Logging
    private static void SetUpLogger()
    {
        if (FileOps.LogFileIsWritable(LogFileName))
        {
            Logger.SetLogFile(LogFileName);
            var bakFullName =
                FileOps.CombineMultipleInputs(FileOps.GetDirFromPath(Logger.LogFile) ?? FileOps.BaseDir, LogFileBak);
            if (FileOps.PathAlreadyExistsAndNotEmpty(Logger.LogFile))
            {
                if (FileOps.PathAlreadyExists(bakFullName)) FileOps.DeleteFile(bakFullName);
                FileOps.MoveFile(Logger.LogFile, bakFullName);
            }
        }

#if DEBUG
        Logger.DebugTracing = true;
#else
        Logger.DebugTracing = IsDebugTracing();
#endif
        Logger.AutoFlush = true;
    }

    private static bool IsDebugTracing() {
        var files = FileOps.GetFilesInDirectory(FileOps.BaseDir, $"{LogDebug}");
        if (files.Count == 0) return false;
        string[] names = [LogDebug, $"{LogDebug}.txt"];
        return files.Any(fsi => names.Contains(fsi.Name.ToLowerInvariant()));
    }
    
    // AppInfo
    private static DateTime? GetBuildDateOfAssembly()
    {
        /* Solution thanks to Gérald Barré (aka. meziantou)
         * https://www.meziantou.net/getting-the-date-of-build-of-a-dotnet-assembly-at-runtime.htm
         */
        var buildDateAtt = AppAssembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .First(a => a.Key == KeyBuildDate).Value;
        if (long.TryParse(buildDateAtt, out var buildDateTicks)) {
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
        var isRunningAdmin = OperatingSystem.IsWindows() 
            ? Models.Windows.NativeAccess.IsWindowsProcessElevated()
            : Models.Linux.NativeAccess.IsUnixProcessElevated();
        var fullName = AppAssemblyName.FullName;
        var buildDate = GetBuildDateOfAssembly();
        var gitHash = GetGitHashOfRepo();
        return new AppInformation(fullName, AppName, AppVersion, isRunningAdmin, buildDate, gitHash);
    }
}
