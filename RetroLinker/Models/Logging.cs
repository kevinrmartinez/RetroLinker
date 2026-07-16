using System.Diagnostics;

namespace RetroLinker.Models;

public static class Logger
{
    /*
     * TODO: I think this method is too expensive for logging, consider alternatives (=0.9)
     * https://stackoverflow.com/a/68363461
     */
    
    // Props
    public static string LogFile { get; private set; }
    public static TraceSource TraceDefault { get; }
    public static TraceSource TraceError { get; }
    public static TraceSource TraceDebug { get; }
    public static bool AutoFlush { get; set; }

    // Public Fields
    
    
    // Internal Fields
    private const string _placeholder = "PLACEHOLDER.log";
    private const string _prefixInfo = "[Info]";
    private const string _prefixWarn = "[Warn]";
    private const string _prefixErro = "[Erro]";
    private const string _prefixCrit = "[Crit]";
    private const string _prefixDebg = "[Debg]";
    private const string _null = "NULL";
    
    static Logger()
    {
        LogFile = _placeholder;

        TraceDefault = new TraceSource("Default", ~SourceLevels.Error);
        TraceError = new TraceSource("Error", SourceLevels.Error);
        TraceDebug = new TraceSource("Debug", SourceLevels.All);

        var consoleTracer = new ConsoleTraceListener(false) {
            Name = "mainConsoleTracer",
            TraceOutputOptions = TraceOptions.None
        };
        var consoleErrorTracer = new ConsoleTraceListener(true) {
            Name = "mainConsoleErrorTracer",
            TraceOutputOptions = TraceOptions.None
        };
        
        TraceDefault.Listeners.Add(consoleTracer);
        TraceError.Listeners.Add(consoleErrorTracer);
        TraceDebug.Listeners.Add(consoleTracer);
        // Trace.AutoFlush = true;
    }

    public static void SetLogFile(string logFileName)
    {
        LogFile = FileOps.CombineMultipleInputs(FileOps.BaseDir, logFileName);
        var textfileTracer = new TextWriterTraceListener(LogFile) {
            Name = "mainTextfileTracer",
            TraceOutputOptions = TraceOptions.Timestamp | TraceOptions.ThreadId
        };
        TraceDefault.Listeners.Add(textfileTracer);
        TraceError.Listeners.Add(textfileTracer);
        TraceDebug.Listeners.Add(textfileTracer);
    }
    
    private static string ObjToString(object? obj) => obj?.ToString() ?? _null;
    
    public static void LogInfo(string message) {
        
        foreach (TraceListener listener in TraceDefault.Listeners) {
            listener.WriteLine(message,  _prefixInfo);
            if (AutoFlush) listener.Flush();
        }
    }
    public static void LogInfo(object? obj) => LogInfo(ObjToString(obj));

    public static void LogWarn(string message) {
        foreach (TraceListener listener in TraceDefault.Listeners) {
            listener.WriteLine(message, _prefixWarn);
            if (AutoFlush) listener.Flush();
        }
    }
    public static void LogWarn(object? obj) => LogWarn(ObjToString(obj));

    public static void LogErro(string message) {
        foreach (TraceListener listener in TraceError.Listeners) {
            listener.WriteLine(message, _prefixErro);
            if (AutoFlush) listener.Flush();
        }
    }
    public static void LogErro(object? obj) => LogErro(ObjToString(obj));

    public static void LogCrit(string message) {
        foreach (TraceListener listener in TraceError.Listeners) {
            listener.WriteLine(message, _prefixCrit);
            if (AutoFlush) listener.Flush();
        }
    }
    public static void LogCrit(object? obj) => LogCrit(ObjToString(obj));

    public static void LogDebg(string message) {
        foreach (TraceListener listener in TraceDebug.Listeners) {
            listener.WriteLine(message, _prefixDebg);
            if (AutoFlush) listener.Flush();
        }
    }
    public static void LogDebg(object? obj) => LogDebg(ObjToString(obj));

    public static void Close() {
        TraceDefault.Close();
        TraceError.Close();
        TraceDebug.Close();
    }
}