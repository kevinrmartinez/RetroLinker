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

using System.Collections.Generic;
using System.IO;

namespace RetroLinker.Models.LinFunc;

public static class LinShortcutter
{
    private const string CommentLine = "# Creado con RetroLinker";
    private const string BlankLine = "";
    private const string EntryHeader = "[Desktop Entry]";
    private const string Notify = "StartupNotify=false";
    private const string Categ = "Categories=Game";
    private const string LinkType = "Type=Application";

    // Overload con un objeto Shortcut <- En Uso
    public static void CreateShortcut(Shortcutter _shortcut, string name, byte makeCopyIndex)
    {
        List<string> shortcut = new()
        {
            // line1,
            CommentLine,
            EntryHeader,
            Categ
        };

        shortcut.Add($"Comment={_shortcut.Desc}");

        shortcut.Add($"Exec={_shortcut.RAdir} {_shortcut.Command}");

        string _iconFile = (string.IsNullOrEmpty(_shortcut.ICONfile)) ? FileOps.DotDesktopRAIcon : _shortcut.ICONfile;
        shortcut.Add($"Icon={_iconFile}");

        shortcut.Add("Name=" + name);
        // shortcut.Add(notify);

        string _terminal = (_shortcut.VerboseB) ? "true" : "false";
        shortcut.Add($"Terminal={_terminal}");

        shortcut.Add(LinkType);

        string outputFile = (makeCopyIndex == byte.MaxValue) ? _shortcut.LNKdir : _shortcut.LNKcpy[makeCopyIndex];

        for (int i = 0; i < shortcut.Count; i++)
        {
            shortcut[i] = string.Concat(shortcut[i], "\n");
        }

        string fullOutputString = string.Concat(shortcut);
        var outputBytes = System.Text.Encoding.UTF8.GetBytes(fullOutputString);
        // TODO: Mover esta parte a FileOps.cs with a try-catch
        File.WriteAllBytes(outputFile, outputBytes);
        
        
        System.Diagnostics.Trace.WriteLine($"{outputFile} created successfully", App.InfoTrace);
        SetExecPermissions(outputFile);
    }

    private static async System.Threading.Tasks.Task SetExecPermissions(string dir)
    {
        System.Diagnostics.Trace.WriteLine($"Trying to set executable permissions to '{dir}'.", App.InfoTrace);
        var processStartInfo = new System.Diagnostics.ProcessStartInfo()
        {
            FileName = "chmod",
            Arguments = $"-c a+x {dir}",
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        var process = new System.Diagnostics.Process()
            { StartInfo = processStartInfo };

        System.Diagnostics.Trace.WriteLine($"Executing chmod a+x {dir}...", App.InfoTrace);
        process.Start();
        string error = process.StandardError.ReadToEnd();
        string output = process.StandardOutput.ReadToEnd();

        System.Diagnostics.Debug.WriteLine(error, App.DebgTrace);
        System.Diagnostics.Trace.WriteLine(output, App.ErroTrace);
    }
}