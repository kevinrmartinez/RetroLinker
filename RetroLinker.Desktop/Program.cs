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
//using Avalonia.ReactiveUI;

namespace RetroLinker.Desktop;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        TimeSpan timeSpan = TimeSpan.FromTicks(DateTime.Now.Ticks);

        StartStopLogging(true);
        System.Diagnostics.Trace.WriteLine($"LaunchTime: {timeSpan}", "[Time]");

        System.Diagnostics.Debug.WriteLine("Starting AvaloniaApp", "[Debg]");
        BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);
        
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

    private static System.Diagnostics.ConsoleTraceListener ConsoleTracer;
    private static System.Diagnostics.TextWriterTraceListener TextfileTracer;
    private const string LogTxt = "trace.log";
    private static readonly string LogFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LogTxt);

    private static void StartStopLogging(bool period)
    {
        if (period)
        {
            try
            {   File.Delete(LogFile);   }
            catch
            {   System.Diagnostics.Trace.WriteLine($"{LogFile} could not be deleted", "[Erro]"); }
            
            ConsoleTracer = new()
            { Name = "mainConsoleTracer", TraceOutputOptions = System.Diagnostics.TraceOptions.Timestamp };
            TextfileTracer = new(LogFile, "mainTextTracer")
            { TraceOutputOptions = System.Diagnostics.TraceOptions.DateTime };
            
            System.Diagnostics.Trace.Listeners.AddRange(new System.Diagnostics.TraceListener[] { ConsoleTracer, TextfileTracer });
        }
        else
        {
            System.Diagnostics.Trace.Listeners.Remove(ConsoleTracer);
            ConsoleTracer.Close();
            System.Diagnostics.Trace.Listeners.Remove(TextfileTracer);
            TextfileTracer.Close();
        }
    }
}
