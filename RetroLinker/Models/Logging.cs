using System.Diagnostics;

namespace RetroLinker.Models;

public class Logger
{
    public bool AutoFlush { get; set; }
    public string LogFile { get; init; }
    public TraceSource TraceDefault { get; init; }
    public TraceSource TraceError { get; init; }
    public TraceSource TraceDebug { get; init; }

    private const string _prefixInfo = "[Info]";
    private const string _prefixWarn = "[Warn]";
    private const string _prefixErro = "[Erro]";
    private const string _prefixCrit = "[Crit]";
    private const string _prefixDebg = "[Debg]";
    private const string _null = "NULL";
    
    public Logger(string logFile)
    {
        LogFile = logFile;

        TraceDefault = new TraceSource("Default", ~SourceLevels.Error);
        TraceError = new TraceSource("Error", SourceLevels.Error);
        TraceDebug = new TraceSource("Debug", SourceLevels.All);

        var ConsoleTracer = new ConsoleTraceListener(false) {
            Name = "mainConsoleTracer",
            TraceOutputOptions = TraceOptions.None
        };
        var ConsoleErrorTracer = new ConsoleTraceListener(true) {
            Name = "mainConsoleErrorTracer",
            TraceOutputOptions = TraceOptions.None
        };
        var TextfileTracer = new TextWriterTraceListener(LogFile) {
            Name = "mainTextfileTracer",
            TraceOutputOptions = TraceOptions.None
        };
        
        TraceDefault.Listeners.AddRange([ ConsoleTracer, TextfileTracer ]);
        TraceError.Listeners.AddRange([ ConsoleErrorTracer, TextfileTracer ]);
        TraceDebug.Listeners.AddRange([ ConsoleTracer, TextfileTracer ]);
        // Trace.AutoFlush = true;
    }
    
    private string ObjToString(object? obj) => obj?.ToString() ?? _null;
    
    public void LogInfo(string message) {
        foreach (TraceListener listener in TraceDefault.Listeners) {
            listener.WriteLine(message,  _prefixInfo);
            if (AutoFlush) listener.Flush();
        }
    }
    public void LogInfo(object? obj) => LogInfo(ObjToString(obj));

    public void LogWarn(string message) {
        foreach (TraceListener listener in TraceDefault.Listeners) {
            listener.WriteLine(message, _prefixWarn);
            if (AutoFlush) listener.Flush();
        }
    }
    public void LogWarn(object? obj) => LogWarn(ObjToString(obj));

    public void LogErro(string message) {
        foreach (TraceListener listener in TraceError.Listeners) {
            listener.WriteLine(message, _prefixErro);
            if (AutoFlush) listener.Flush();
        }
    }
    public void LogErro(object? obj) => LogErro(ObjToString(obj));

    public void LogCrit(string message) {
        foreach (TraceListener listener in TraceError.Listeners) {
            listener.WriteLine(message, _prefixCrit);
            if (AutoFlush) listener.Flush();
        }
    }
    public void LogCrit(object? obj) => LogCrit(ObjToString(obj));

    public void LogDebg(string message) {
        foreach (TraceListener listener in TraceDebug.Listeners) {
            listener.WriteLine(message, _prefixDebg);
            if (AutoFlush) listener.Flush();
        }
    }
    public void LogDebg(object? obj) => LogDebg(ObjToString(obj));

    public void Close() {
        TraceDefault.Close();
        TraceError.Close();
        TraceDebug.Close();
    }
}