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

using RetroLinker.Models.IO;

namespace RetroLinker.Models.Linux;

public static class DesktopEntryOps
{
    // What a mess...
    // REWRITE: Remake this. Better yet, don't do this sh*t...
    
    // private const string Ext = FileOps.LinLinkExt;
    public const string NamePlaceHolder = "[ROM]";
    public const string CorePlaceHolder = "[CORE]";

    public static string StdDesktopEntry(string? friendlyName, string? core)
    {
        core = (!string.IsNullOrWhiteSpace(core)) ? core : CorePlaceHolder;
        friendlyName = (!string.IsNullOrWhiteSpace(friendlyName)) ? friendlyName : NamePlaceHolder;
        return DesktopEntryName(friendlyName, core);
    }
    
    public static string RenameForDesktopEntry(string linkPath, string? core, out string friendlyName)
    {
        _ = FileOps.SeparateFileNameFromPath(linkPath, out friendlyName, out var fileExt);
        var newFileName = StdDesktopEntry(friendlyName, core);
        newFileName += fileExt;
        var dir = FileOps.GetDirFromPath(linkPath);
        System.ArgumentException.ThrowIfNullOrEmpty(dir);
        return FileOps.CombineMultipleInputs(dir, newFileName);
    }
    
    private static string DesktopEntryName(string fileName, string core)
    {
        const char whiteSpaceReplacer = '_';
        const string prefix = $"{App.RetroBin}.";
            
        fileName = fileName.Replace(CommandLine.Manager.SepWhiteSpace, whiteSpaceReplacer);
        fileName = fileName.TrimEnd('.');
        
        var built = new System.Text.StringBuilder();
        built.Append(prefix);
        built.Append($"{core}.");
        built.Append(fileName);
        return built.ToString();
    }
}