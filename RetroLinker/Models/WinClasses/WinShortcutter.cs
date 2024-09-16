using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RetroLinker.Models.WinClasses;

public static class WinShortcutter
{
    private const string objShell = "$shell";
    private const string objLink = "$link";
    private const string objFSO = "fso";
    private const string objStdOut = "stdout";
    private static readonly string ps1Title = $"$host.UI.RawUI.WindowTitle = \"{App.AppName} Script Runner\"";
    
    public static void CreateShortcut(Shortcutter _shortcut, string _outputPath)
    {
        
        var fileStrings = new List<string>
        {
            ps1Title,
            $"{objShell} = New-Object -ComObject WScript.Shell",
            $"{objLink} = {objShell}.CreateShortcut(\"{_outputPath}\")",
            $"{objLink}.TargetPath = \"{_shortcut.RAdir}\"",
            $"{objLink}.Arguments = \"{_shortcut.Command}\"",
            $"{objLink}.Description = \"{_shortcut.Desc}\"",
            $"{objLink}.WorkingDirectory = \"{_shortcut.RApath}\"",
        };
        if (!string.IsNullOrEmpty(_shortcut.ICONfile)) fileStrings.Add($"{objLink}.IconLocation = \"{_shortcut.ICONfile}\"");
        fileStrings.Add($"{objLink}.Save()");
        
        FileOps.CreateLinkWriteScript(fileStrings.ToArray(), out var scriptFile);
        RunLinkWriteScript(scriptFile);
        Trace.WriteLine($"'{_outputPath}' file created successfully.", App.InfoTrace);
    }

    public static string[] ReadShortcut(string linkPath)
    {
        var fileStrings = new List<string>
        {
            ps1Title,
            $"{objShell} = New-Object -ComObject WScript.Shell",
            $"{objLink} = {objShell}.CreateShortcut(\"{linkPath}\")",
            $"Write-Output {objLink}.TargetPath",
            $"Write-Output {objLink}.Arguments",
            $"Write-Output {objLink}.Description",
            $"Write-Output {objLink}.WorkingDirectory",
            $"Write-Output {objLink}.IconLocation"
        };
        
        FileOps.CreateLinkReadScript(fileStrings.ToArray(), out string scriptFile);
        RunLinkReadScript(scriptFile, out string[] linkContent);
        Trace.WriteLine($"'{linkPath}' file read successfully.", App.InfoTrace);
        return linkContent;
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
    
    private static Process PowershellProcess(string file, out string stdout, out string stderr)
    {
        var powershell = new ProcessStartInfo()
        {
            FileName = "powershell",
            Arguments = $"-NoProfile -WindowStyle Hidden \"{file}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using var powershell_pro = Process.Start(powershell);
        powershell_pro.WaitForExit();
        stdout = powershell_pro.StandardOutput.ReadToEnd();
        stderr = powershell_pro.StandardError.ReadToEnd();
        return powershell_pro;
    }

    private static void RunLinkWriteScript(string file)
    {
        // using var cscript_pro= cscriptProcess(file, out string stdout, out string stderr);
        _ = PowershellProcess(file, out _, out string stderr);
        
        if (string.IsNullOrEmpty(stderr)) return;
        Trace.WriteLine(stderr, App.ErroTrace);
        var err = $"PowerShell process exited with an error:\n{stderr}";
        throw new ApplicationException(err);
    }

    private static void RunLinkReadScript(string file, out string[] stdoutArray)
    {
        _ = PowershellProcess(file, out string stdout, out string stderr);
        stdoutArray = stdout.Split("\r\n");
        if (string.IsNullOrEmpty(stderr)) return;
        
        Trace.WriteLine(stderr, App.ErroTrace);
        var err = $"PowerShell process exited with an error:\n{stderr}";
        throw new ApplicationException(err);
    }
}