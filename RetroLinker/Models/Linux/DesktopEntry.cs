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

namespace RetroLinker.Models.Linux;

public static class DesktopEntry
{
    // What a mess...
    // TODO: Remake this. Better yet, don't do this sh*t...
    
    // private const string Ext = FileOps.LinLinkExt;
    public const string NamePlaceHolder = "[ROM]";
    public const string CorePlaceHolder = "[CORE]";

    public static string StdDesktopEntry(string? friendlyName, string? core)
    {
        core = (!string.IsNullOrWhiteSpace(core)) ? core : CorePlaceHolder;
        friendlyName = (!string.IsNullOrWhiteSpace(friendlyName)) ? friendlyName : NamePlaceHolder;
        return DesktopEntryName(friendlyName, core);
    }
    
    private static string DesktopEntryName(string fileName, string core)
    {
        const string prefix = "retroarch.";
        const string whiteSpace = " ";
        const string whiteSpaceReplacer = "_";
        bool dotAtTheEnd = true;
            
        fileName = fileName.Replace(whiteSpace, whiteSpaceReplacer);
        do
        {
            int lastChar = fileName.Length - 1;
            if (fileName[lastChar] == '.') fileName = fileName.Remove(lastChar);
            else dotAtTheEnd = false;
        } while (dotAtTheEnd);
        
        fileName = fileName.Insert(0, $"{core}.");
        fileName = fileName.Insert(0, prefix);
        return fileName;
    }
}