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
using RetroarchShortcutterV2.Models;
using System.Collections.Generic;
using System.IO;

namespace LinFunc;

public static class LinShortcutter
{
    private const string lineComment = "# Creado con RetroLinker";  // No se como poner el comentario...
    private const string line1 = "";
    private const string line2 = "[Desktop Entry]";
    private const string notify = "StartupNotify=false";
    private const string cat = "Categories=Game";
    private const string linktype = "Type=Application";

    // Overload con un objeto Shortcut <- En Uso
    public static void CreateShortcut(Shortcutter _shortcut, string name, byte makeCopyIndex)
    {
        List<string> shortcut = new()
        {
            // line1,
            lineComment,
            line2,
            cat
        };

        shortcut.Add($"Comment={_shortcut.Desc}");

        shortcut.Add($"Exec={_shortcut.RAdir} {_shortcut.Command}");

        string _iconFile = (string.IsNullOrEmpty(_shortcut.ICONfile)) ? "retroarch" : _shortcut.ICONfile;
        shortcut.Add($"Icon={_iconFile}");   // No esta garantizado que funcione

        shortcut.Add("Name=" + name);
        // shortcut.Add(notify);

        string _terminal = (_shortcut.VerboseB) ? "true" : "false";
        shortcut.Add($"Terminal={_terminal}");

        shortcut.Add(linktype);

        string outputFile = (makeCopyIndex == byte.MaxValue) ? _shortcut.LNKdir : _shortcut.LNKcpy[makeCopyIndex];

        for (int i = 0; i < shortcut.Count; i++)
        {
            shortcut[i] = string.Concat(shortcut[i], "\n");
        }

        string fullOutputString = string.Concat(shortcut);
        var outputBytes = System.Text.Encoding.UTF8.GetBytes(fullOutputString);
        File.WriteAllBytes(outputFile, outputBytes);

        // TODO: Mover esta parte a FileOps.cs
        //File.WriteAllBytes();
        //File.WriteAllLines(outputFile, shortcut, Encoding.UTF8); 
        System.Diagnostics.Trace.WriteLine($"{outputFile} creado con exito.", "[Info]");
        SetExecPermissions(outputFile);
    }

    private static async System.Threading.Tasks.Task SetExecPermissions(string dir)
    {
        System.Diagnostics.Trace.WriteLine($"Intentando establecer permisos de ejecucion para {Path.GetFileName(dir)}.", "[Info]");
        var processStartInfo = new System.Diagnostics.ProcessStartInfo()
        {
            FileName = "chmod",
            Arguments = $"-c a+x {dir}",
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        var process = new System.Diagnostics.Process()
            { StartInfo = processStartInfo };

        System.Diagnostics.Trace.WriteLine($"Ejecutando chmod a+x {dir}...", "[Info]");
        process.Start();
        string error = process.StandardError.ReadToEnd();
        string output = process.StandardOutput.ReadToEnd();

        System.Diagnostics.Debug.WriteLine(error, "[Proc]");
        System.Diagnostics.Trace.WriteLine(output, "[Proc]");
    }
}