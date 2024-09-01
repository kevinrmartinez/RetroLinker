using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RetroLinker.Models.WinClasses;

public static class WinShortcutter
{
    private const string objShell = "Set objShell = CreateObject(\"WScript.Shell\")";
    private const string objShortcutSave = "objShortcut.Save";
    
    public static void CreateShortcut(Shortcutter _shortcut, string _outputPath)
    {
        
        var fileString = new List<string>
        {
            objShell,
            $"Set objShortcut = objShell.CreateShortcut(\"{_outputPath}\")",
            $"objShortcut.TargetPath = \"{_shortcut.RAdir}\"",
            $"objShortcut.Arguments = \"{_shortcut.Command}\"",
            $"objShortcut.Description = \"{_shortcut.Desc}\"",
            $"objShortcut.WorkingDirectory = \"{_shortcut.RApath}\"",
        };
        if (!string.IsNullOrEmpty(_shortcut.ICONfile)) fileString.Add($"objShortcut.IconLocation = \"{_shortcut.ICONfile}\"");
        fileString.Add(objShortcutSave);
        if (FileOps.WriteShortcutVBS(fileString.ToArray(), out var outputFile)) RunShortcutScript(outputFile);
    }

    private static void RunShortcutScript(string file)
    {
        var cscript = new ProcessStartInfo()
        {
            FileName = "cscript",
            Arguments = $"//NoLogo \"{file}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using var cscript_pro = Process.Start(cscript);
        cscript_pro.WaitForExit();
        var csout = cscript_pro.StandardOutput.ReadToEnd();
        var cserr = cscript_pro.StandardError.ReadToEnd();

        Debug.WriteLine($"cscript exit code: {cscript_pro.ExitCode}", App.DebgTrace);
        if (cscript_pro.ExitCode == 0) return;
        var validOutput = (string.IsNullOrWhiteSpace(cserr)) ? csout : cserr;
        Trace.WriteLine(validOutput, App.ErroTrace);
        var err = $"cscript process exited with code {cscript_pro.ExitCode}\n{validOutput}";
        throw new ApplicationException(err);
    }
}