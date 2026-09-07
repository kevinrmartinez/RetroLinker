/*
    RetroLinker: A .NET GUI application to help create desktop links of games running on RetroArch.
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
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace RetroLinker.Models.Linux;

[SupportedOSPlatform(App.PlatformLin)]
public static class ShortcutManager
{
    // FreeDesktop Spec: https://specifications.freedesktop.org/desktop-entry/latest/
    // TODO: a desktop-entry is basically a .ini file, so this could be made with a INI parser (>=0.9) 
    
    private static readonly string CommentLine = $"# Created with {App.LocalInformation.Name} v{App.LocalInformation.Version}";
    private const string EntryHeader = "[Desktop Entry]";
    // private const string Notify = "StartupNotify=false";
    private const string Category = "Categories=Game";
    private const string LinkType = "Type=Application";
    
    public static async Task CreateShortcut(LinkParameters link)
    {
        List<string> shortcut = new()
        {
            CommentLine,
            EntryHeader,
            Category
        };

        shortcut.Add($"Comment={link.Description}");

        shortcut.Add($"Exec={link.RaExecutable} {link.RaArguments}");
        
        shortcut.Add($"Icon={link.IconPath}");

        shortcut.Add("Name=" + link.FriendlyName);
        // shortcut.Add(notify);
        
        shortcut.Add($"Terminal={link.Verbose.ToString().ToLower()}");

        shortcut.Add(LinkType);

        string outputFile = link.OutputPath;

        for (int i = 0; i < shortcut.Count; i++)
        {
            // shortcut[i] = string.Concat(shortcut[i], "\n");
            shortcut[i] += "\n";
        }

        string fullOutputString = string.Concat(shortcut);
        var outputBytes = System.Text.Encoding.UTF8.GetBytes(fullOutputString);
        
        await FileOps.WriteDesktopEntry(outputFile, outputBytes);
        // If file write is successful (doesn't throw), set execution permissions
        _ = SetExecPermissions(outputFile);
    }

    private static async Task SetExecPermissions(string filePath)
    {
        Logger.LogInfo($"Trying to set executable permissions to \"{filePath}\".");

        try {
            await Task.Run(() => FileOps.MakeLinuxFileExecutable(filePath));
            Logger.LogInfo($"Executable permissions to \"{filePath}\" were set successfully.");
        }
        catch (Exception e) {
            Logger.LogWarn($"Failed to set executable permissions to \"{filePath}\".");
            Logger.LogErro(e);
        }
    }
}