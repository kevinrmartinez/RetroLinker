using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RetroLinker.Models.WinClasses;

public static class WinShortcutter
{
    private const string objShell = "shell";
    private const string objLink = "link";
    private const string objFSO = "fso";
    private const string objStdOut = "stdout";
    
    public static void CreateShortcut(Shortcutter _shortcut, string _outputPath)
    {
        
        var fileStrings = new List<string>
        {
            $"Set {objShell} = CreateObject(\"WScript.Shell\")",
            $"Set {objLink} = {objShell}.CreateShortcut(\"{_outputPath}\")",
            $"{objLink}.TargetPath = \"{_shortcut.RAdir}\"",
            $"{objLink}.Arguments = \"{_shortcut.Command}\"",
            $"{objLink}.Description = \"{_shortcut.Desc}\"",
            $"{objLink}.WorkingDirectory = \"{_shortcut.RApath}\"",
        };
        if (!string.IsNullOrEmpty(_shortcut.ICONfile)) fileStrings.Add($"{objLink}.IconLocation = \"{_shortcut.ICONfile}\"");
        fileStrings.Add($"{objLink}.Save");
        FileOps.CreateLinkWriteScript(fileStrings.ToArray(), out var outputFile);
        RunLinkWriteScript(outputFile);
    }

    public static string[] ReadShortcut(string linkPath)
    {
        var fileStrings = new List<string>
        {
            $"Set {objFSO} = CreateObject(\"Scripting.FileSystemObject\")",
            $"Set {objStdOut} = {objFSO}.GetStandardStream(1)",
            $"Set {objShell} = CreateObject(\"WScript.Shell\")",
            $"Set {objLink} = {objShell}.CreateShortcut(\"{linkPath}\")",
            $"{objStdOut}.WriteLine {objLink}.TargetPath",
            $"{objStdOut}.WriteLine {objLink}.Arguments",
            $"{objStdOut}.WriteLine {objLink}.Description",
            $"{objStdOut}.WriteLine {objLink}.WorkingDirectory",
            $"{objStdOut}.WriteLine {objLink}.IconLocation"
        };
        
        FileOps.CreateLinkReadScript(fileStrings.ToArray(), out string outputFile);
        using var cscript_pro= cscriptProcess(outputFile, out string stdout, out string stderr);
        
        return stdout.Split("\r\n");
    }

    private static Process cscriptProcess(string file, out string stdout, out string stderr)
    {
        var cscript = new ProcessStartInfo()
        {
            FileName = "cscript",
            Arguments = $"/b /t:5 /NoLogo \"{file}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        using var cscript_pro = Process.Start(cscript);
        cscript_pro.WaitForExit();
        stdout = cscript_pro.StandardOutput.ReadToEnd();
        stderr = cscript_pro.StandardError.ReadToEnd();
        return cscript_pro;
    }

    private static void RunLinkWriteScript(string file)
    {
        using var cscript_pro= cscriptProcess(file, out string stdout, out string stderr);
        
        Debug.WriteLine($"cscript exit code: {cscript_pro.ExitCode}", App.DebgTrace);
        if (cscript_pro.ExitCode == 0) return;
        
        var validOutput = (string.IsNullOrWhiteSpace(stderr)) ? stdout : stderr;
        Trace.WriteLine(validOutput, App.ErroTrace);
        var err = $"cscript process exited with code {cscript_pro.ExitCode}\n{validOutput}";
        throw new ApplicationException(err);
    }
}