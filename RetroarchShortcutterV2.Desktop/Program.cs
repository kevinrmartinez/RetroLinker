using System;

using Avalonia;
//using Avalonia.ReactiveUI;
using Projektanker.Icons.Avalonia.FontAwesome;
using Projektanker.Icons.Avalonia.MaterialDesign;
using Projektanker.Icons.Avalonia;

namespace RetroarchShortcutterV2.Desktop;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        var ConsoleTracer = new System.Diagnostics.ConsoleTraceListener()
        { Name = "mainConsoleTracer", TraceOutputOptions = System.Diagnostics.TraceOptions.DateTime };
        var TextfileTracer = new System.Diagnostics.TextWriterTraceListener("log.txt", "mainTextTracer")
        { TraceOutputOptions = System.Diagnostics.TraceOptions.DateTime };
        System.Diagnostics.Trace.Listeners.AddRange(new System.Diagnostics.TraceListener[] { ConsoleTracer, TextfileTracer });

        System.Diagnostics.Trace.WriteLine("Iniciando AvaloniaApp", "[Info]");
        Console.WriteLine("[Info] Iniciando AvaloniaApp");
        BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);
        System.Diagnostics.Trace.Listeners.Remove(ConsoleTracer);
        ConsoleTracer.Close();
        System.Diagnostics.Trace.Listeners.Remove(TextfileTracer);
        TextfileTracer.Close();
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        IconProvider.Current
            .Register<FontAwesomeIconProvider>()
            .Register<MaterialDesignIconProvider>();
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            /*.UseReactiveUI()*/;
    }
}
