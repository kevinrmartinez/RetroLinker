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

using System;
using System.Collections.Generic;

namespace RetroLinker.Models.LinuxClasses;

public static class LinShortcutter
{
    private static readonly string CommentLine = $"# Created with {App.AppName} v{App.AppVersion}";
    private const string EntryHeader = "[Desktop Entry]";
    // private const string Notify = "StartupNotify=false";
    private const string Category = "Categories=Game";
    private const string LinkType = "Type=Application";
    
    public static void CreateShortcut(Shortcutter link, ShortcutterOutput linkOutput)
    {
        List<string> shortcut = new()
        {
            CommentLine,
            EntryHeader,
            Category
        };

        shortcut.Add($"Comment={link.Desc}");

        shortcut.Add($"Exec={link.RAdir} {link.Command}");

        string _iconFile = (string.IsNullOrEmpty(link.ICONfile)) ? FileOps.DotDesktopRAIcon : link.ICONfile;
        shortcut.Add($"Icon={_iconFile}");

        shortcut.Add("Name=" + linkOutput.FriendlyName);
        // shortcut.Add(notify);

        string _terminal = (link.VerboseB) ? "true" : "false";
        shortcut.Add($"Terminal={_terminal}");

        shortcut.Add(LinkType);

        string outputFile = linkOutput.FullPath;

        for (int i = 0; i < shortcut.Count; i++)
        {
            // shortcut[i] = string.Concat(shortcut[i], "\n");
            shortcut[i] += "\n";
        }

        string fullOutputString = string.Concat(shortcut);
        var outputBytes = System.Text.Encoding.UTF8.GetBytes(fullOutputString);
        
        FileOps.WriteDesktopEntry(outputFile, outputBytes);
        // If file write is successful (doesn't throw), set execution permissions
        System.Threading.Tasks.Task.Run(() => SetExecPermissions(outputFile));
    }

    private static void SetExecPermissions(string filePath)
    {
        App.Logger?.LogDebg($"SetExecPermissions Thread ID: {Environment.CurrentManagedThreadId}");
        App.Logger?.LogInfo($"Trying to set executable permissions to \"{filePath}\".");

        try {
            FileOps.MakeFileExecutable(filePath);
            App.Logger?.LogInfo($"Executable permissions to \"{filePath}\" were set successfully.");
        }
        catch (Exception e) {
            App.Logger?.LogErro($"Failed to set executable permissions to \"{filePath}\".");
            App.Logger?.LogErro($"Error: {e.Message}");
        }
    }
}