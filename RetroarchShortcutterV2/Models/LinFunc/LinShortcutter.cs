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
using SharpConfig;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LinFunc;

public static class LinShortcutter
{
    private const string lineComment = "# Creado con RetroarchShortcutter V2";  // No se como poner el comentario...
    private const string line1 = "";
    private const string line2 = "[Desktop Entry]";
    private const string notify = "StartupNotify=false";
    private const string cat = "Game";
    private const string type = "Type=Application";

    // Overload original, con una lista de objetos
    public static void CreateShortcut(IList<object> _shortcut, string name, bool verbose)
    {
        string dir = _shortcut[9].ToString();

        List<string> shortcut = new()
        {
            line1,
            line2
        };

        if (_shortcut[8] != null) { shortcut.Add("Comment=" + _shortcut[8].ToString()); }
        else { shortcut.Add("Comment="); }

        shortcut.Add("Exec=" + _shortcut[0].ToString() + " " + _shortcut[7].ToString());

        if (_shortcut[6] != null) { shortcut.Add("Icon=" + _shortcut[6].ToString()); }
        else { shortcut.Add("Icon=retroarch"); }    // No esta garantizado que funcione

        shortcut.Add("Name=" + name);
        shortcut.Add(notify);

        if (verbose) { shortcut.Add("Terminal=true"); }
        else { shortcut.Add("Terminal=false"); }

        shortcut.Add(type);

        File.WriteAllLines( dir, shortcut, Encoding.UTF8);
    }

    // Overload con un objeto Shortcut
    public static void CreateShortcut(Shortcutter _shortcut)
    {
        //string dir = _shortcut.LNKdir;
        string name = Path.GetFileNameWithoutExtension(_shortcut.LNKdir);

        List<string> shortcut = new()
        {
            line1,
            line2
        };

        if (_shortcut.Desc != null) { shortcut.Add("Comment=" + _shortcut.Desc.ToString()); }
        else { shortcut.Add("Comment="); }

        shortcut.Add("Exec=" + _shortcut.RAdir + " " + _shortcut.Command);

        if (_shortcut.ICONfile != null) { shortcut.Add("Icon=" + _shortcut.ICONfile); }
        else { shortcut.Add("Icon=retroarch"); }    // No esta garantizado que funcione

        shortcut.Add("Name=" + name);
        shortcut.Add(notify);

        if (_shortcut.VerboseB) { shortcut.Add("Terminal=true"); }
        else { shortcut.Add("Terminal=false"); }

        shortcut.Add(type);

        File.WriteAllLines(_shortcut.LNKdir, shortcut, Encoding.UTF8);
    }

    // Version con SharpConfig <- IN USE
    public static void CreateShortcutIni(Shortcutter _shortcut, string name)
    { 
        System.Diagnostics.Trace.WriteLine($"Creando {_shortcut.LNKdir} para Linux.", "[Info]");
        Configuration.OutputRawStringValues = true;
        Configuration desktop_file = new();
            
        Section DesktopEntry = desktop_file["Desktop Entry"];
        DesktopEntry.Comment = lineComment;
        DesktopEntry["Categories"].StringValue = cat;
        DesktopEntry["Comment"].StringValue = _shortcut.Desc;
        DesktopEntry["Exec"].StringValue = $"{_shortcut.RAdir} {_shortcut.Command}" + "\""; // SharpConfig elimina el ultima ", hay que volverla a añadir
        DesktopEntry["Icon"].StringValue = _shortcut.ICONfile;
        DesktopEntry["Name"].StringValue = name;
        //DesktopEntry["StartupNotify"].StringValue = "true";
        DesktopEntry["Terminal"].StringValue = (_shortcut.VerboseB) ? "true" : "false";
        DesktopEntry["Type"].StringValue = "Application";

        desktop_file.SaveToFile(_shortcut.LNKdir);
        System.Diagnostics.Trace.WriteLine($"{_shortcut.LNKdir} creado con exito.", "[Info]");
        SetExecPermissions(_shortcut.LNKdir);
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