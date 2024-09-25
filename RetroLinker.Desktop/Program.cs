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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Avalonia;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;

namespace RetroLinker.Desktop;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        StartStopLogging(true);
        var newArgs = new List<string>
        {
            AppName, AppVersion
        };
        newArgs.AddRange(args);
        Trace.WriteLine($"{AppName} v{AppVersion}", "[Info]");
        Debug.WriteLine($"LaunchTime: {DateTime.Now:HH:mm:ss.fff}", "[Time]");
        
        Debug.WriteLine("Starting AvaloniaApp", "[Debg]");
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(newArgs.ToArray());
        
        // App Closing
        StartStopLogging(false);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        IconProvider.Current.Register<FontAwesomeIconProvider>();
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }

    // Parameters
    private static readonly System.Reflection.AssemblyName AppAssembly = System.Reflection.Assembly.GetExecutingAssembly().GetName();
    private static readonly string AppName = AppAssembly.Name;
    private static readonly string AppVersion = AppAssembly.Version.ToString(3);
    
    // Logging
    private static ConsoleTraceListener ConsoleTracer;
    private static TextWriterTraceListener TextfileTracer;
    private static readonly string LogFileName = $"{AppName}.log";
    private static readonly string LogFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LogFileName);

    private static void StartStopLogging(bool mode)
    {
        // TODO: print to log even if an exception occured, it just makes sense...
        if (mode)
        {
            try {
                File.Delete(LogFile);
            }
            catch {
                Trace.WriteLine($"{LogFile} could not be deleted!", "[Erro]");
            }
            
            ConsoleTracer = new() {
                Name = "mainConsoleTracer", 
                TraceOutputOptions = TraceOptions.Timestamp 
            };
            TextfileTracer = new(LogFile, "mainTextTracer") {
                TraceOutputOptions = TraceOptions.DateTime,
            };

            Trace.Listeners.Add(ConsoleTracer);
            Trace.Listeners.Add(TextfileTracer);
        }
        else
        {
            Trace.Listeners.Remove(ConsoleTracer);
            Trace.Listeners.Remove(TextfileTracer);
            ConsoleTracer.Close();
            TextfileTracer.Close();
        }
    }
}
