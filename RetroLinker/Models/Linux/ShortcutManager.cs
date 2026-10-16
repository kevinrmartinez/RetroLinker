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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using RetroLinker.Models.DesktopLink;
using RetroLinker.Models.IO;
using TG.INI;
using TG.INI.Serialization;

namespace RetroLinker.Models.Linux;

[SupportedOSPlatform(App.PlatformLin)]
public static partial class ShortcutManager
{
    // FreeDesktop Spec: https://specifications.freedesktop.org/desktop-entry/latest/
    // TODO: a desktop-entry is basically a .ini file, so this could be made with a INI parser (>=0.9) 
    
    private static readonly string CommentLine = $"# Created with {App.LocalInformation.Name} v{App.LocalInformation.Version}";
    private const string EntryHeader = "[Desktop Entry]";
    // private const string Notify = "StartupNotify=false";
    private const string Category = "Categories=Game";
    private const string LinkType = "Type=Application";

    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(DesktopEntry))]
    public static async Task CreateShortcut(LinkParameters linkBase)
    {
        var link = linkBase as LinuxLinkParameters;
        ArgumentNullException.ThrowIfNull(link);
        
        var desktopEntry = new DesktopEntry()
        {
            Categories = ["Game", "Emulator", " "],
            Comment = link.Description,
            Exec = $"{link.RaExecutable} {link.RaArguments}",
            Icon = link.IconPath,
            Name = link.FriendlyName
        };
        
        IniDocument doc = IniSerialization.SerializeObjectToNewDocument(desktopEntry);
        // Some Desktop Environments don't recognize bool values that start with a capital letter
        doc[MainEntry][nameof(DesktopEntry.StartupNotify)].Value = false.ToString().ToLowerInvariant();
        doc[MainEntry][nameof(DesktopEntry.Terminal)].Value = link.RaVerbose.ToString().ToLowerInvariant();
        
        await FileOps.WriteIniDocument(doc, link.OutputPath);
        // If file write is successful (doesn't throw), set execution permissions
        _ = SetExecPermissions(link.OutputPath);
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