/*
    RetroLinker: A .NET GUI application to help create desktop links of games running on RetroArch.
    Copyright (C) 2023-2026 Kevin Rafael Martinez Johnston

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

using System.Diagnostics;
using System.Runtime.CompilerServices;
using RetroLinker.Models.IO;

namespace RetroLinker;

public static class Logger
{
    // Props
    public static string LogFile { get; private set; }
    public static bool AutoFlush { get; set; }
    public static bool DebugTracing { get; private set; }

    // Public Fields


    // Internal Fields
    private const string _placeholder = "PLACEHOLDER.log";
    private const string PrefixInfo = "[Info]";
    private const string PrefixWarn = "[Warn]";
    private const string PrefixErro = "[Erro]";
    private const string PrefixCrit = "[Crit]";
    private const string PrefixDebg = "[Debug]";
    private const string PrefixTrac = "[Trace]";
    private const string _null = "NULL";

    private static readonly TraceSource Source = new("RetroLinkerLogger", SourceLevels.All);
    private static readonly System.Threading.Lock Lock = new();
    private static bool _isFileInitialized;
    private static readonly Dictionary<RetroLinkerLogLevel, RetroLinkerLogType[]> LogTypesMap;

    static Logger()
    {
        LogFile = _placeholder;

        LogTypesMap = new()
        {
            { RetroLinkerLogLevel.Information, [RetroLinkerLogType.Information, RetroLinkerLogType.Warning] },
            { RetroLinkerLogLevel.Error, [RetroLinkerLogType.Error, RetroLinkerLogType.Critical] },
            { RetroLinkerLogLevel.Debug, [RetroLinkerLogType.Debug, RetroLinkerLogType.Trace] }
        };

        var consoleTracer = new ConsoleTraceListener(useErrorStream: false) {
            Name = "mainConsoleTracer",
            Filter = new RetroLinkerTraceFilter(RetroLinkerLogLevel.Information)
        };
        var consoleErrorTracer = new ConsoleTraceListener(useErrorStream: true) {
            Name = "mainConsoleErrorTracer",
            Filter = new RetroLinkerTraceFilter(RetroLinkerLogLevel.Error)
        };
        var consoleDebugTracer = new ConsoleTraceListener(useErrorStream: false) {
            Name = "mainConsoleDebugTracer",
            Filter = new RetroLinkerTraceFilter(RetroLinkerLogLevel.Debug)
        };

        Source.Listeners.Clear();
        Source.Listeners.Add(consoleTracer);
        Source.Listeners.Add(consoleErrorTracer);
        Source.Listeners.Add(consoleDebugTracer);
    }

    public static void SetDebugTracing(bool value) => DebugTracing = value;

    public static void SetLogFile(string logFileName)
    {
        if (_isFileInitialized) throw new InvalidOperationException("Logger file is already initialized");

        _isFileInitialized = true;
        LogFile = FileOps.CombineMultipleInputs(FileOps.BaseDir, logFileName);
        var textFileTracer = new TextWriterTraceListener(LogFile) {
            Name = "mainTextfileTracer",
            Filter = new RetroLinkerTraceFilter(RetroLinkerLogLevel.All)
        };

        lock (Lock) {
            Source.Listeners.Add(textFileTracer);
        }
    }

    private static string ObjToString(object? obj) => obj?.ToString() ?? _null;

    private static void WriteToSource(RetroLinkerLogType logType, string message, string category, string? caller)
    {
        var fullMessage = (DebugTracing) ? $"[{caller}] {message}" : message;
        lock (Lock) {
            foreach (TraceListener listener in Source.Listeners)
            {
                var shouldNotLogThis = !((RetroLinkerTraceFilter?)listener.Filter)?.ShouldLog(logType);
                if (shouldNotLogThis.GetValueOrDefault(defaultValue: true)) continue;
                listener.WriteLine(fullMessage, category);
                if (AutoFlush) listener.Flush();
            }
        }
    }

    public static void Info(string? message, [CallerMemberName] string? caller = null) =>
        WriteToSource(RetroLinkerLogType.Information, message ?? _null, PrefixInfo, caller);
    public static void Info(object? obj, [CallerMemberName] string? caller = null) =>
        WriteToSource(RetroLinkerLogType.Information, ObjToString(obj), PrefixInfo, caller);


    public static void Warn(string? message, [CallerMemberName] string? caller = null) =>
        WriteToSource(RetroLinkerLogType.Warning, message ?? _null, PrefixWarn, caller);
    public static void Warn(object? obj, [CallerMemberName] string? caller = null) =>
        WriteToSource(RetroLinkerLogType.Warning, ObjToString(obj), PrefixWarn, caller);


    public static void Error(string? message, [CallerMemberName] string? caller = null) =>
        WriteToSource(RetroLinkerLogType.Error, message ?? _null, PrefixErro, caller);
    public static void Error(object? obj, [CallerMemberName] string? caller = null) =>
        WriteToSource(RetroLinkerLogType.Error, ObjToString(obj), PrefixErro, caller);


    public static void Crit(string? message, [CallerMemberName] string? caller = null) =>
        WriteToSource(RetroLinkerLogType.Critical, message ?? _null, PrefixCrit, caller);
    public static void Crit(object? obj, [CallerMemberName] string? caller = null) =>
        WriteToSource(RetroLinkerLogType.Critical, ObjToString(obj), PrefixCrit, caller);


    public static void Debug(string? message, [CallerMemberName] string? caller = null) =>
        WriteToSource(RetroLinkerLogType.Debug, message ?? _null, PrefixDebg, caller);
    public static void Debug(object? obj, [CallerMemberName] string? caller = null) =>
        WriteToSource(RetroLinkerLogType.Debug, ObjToString(obj), PrefixDebg, caller);


    public static void Trace(string? message, [CallerMemberName] string? caller = null) =>
        WriteToSource(RetroLinkerLogType.Trace, message ?? _null, PrefixTrac, caller);
    public static void Trace(object? obj, [CallerMemberName] string? caller = null) =>
        WriteToSource(RetroLinkerLogType.Trace, ObjToString(obj), PrefixTrac, caller);


    public static void Close()
    {
        lock (Lock)
        {
            Source.Flush();
            Source.Close();
        }
    }


    private class RetroLinkerTraceFilter (RetroLinkerLogLevel filterLevel) : TraceFilter
    {
        public RetroLinkerLogLevel TraceFilterLevel { get; } = filterLevel;

        public override bool ShouldTrace(
            TraceEventCache? cache,
            string source,
            TraceEventType eventType,
            int id,
            [StringSyntax("CompositeFormat")] string? formatOrMessage,
            object?[]? args,
            object? data1,
            object?[]? data) => eventType == TraceEventType.Verbose;    // <= WriteLine() checks for the Verbose type

        public bool ShouldLog(RetroLinkerLogType logType)
        {
            switch (TraceFilterLevel)
            {
                case RetroLinkerLogLevel.All:
                    return (logType is not RetroLinkerLogType.Debug and not RetroLinkerLogType.Trace) || DebugTracing;
                case RetroLinkerLogLevel.Debug:
                    return DebugTracing && (logType is RetroLinkerLogType.Debug or RetroLinkerLogType.Trace);
                default:
                    return LogTypesMap[TraceFilterLevel].Contains(logType);
            }
        }
    }
}