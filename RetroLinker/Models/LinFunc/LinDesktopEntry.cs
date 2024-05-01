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

namespace RetroLinker.Models.LinFunc;

public static class LinDesktopEntry
{
    private const string Ext = FileOps.LinLinkExt;
    public const string NamePlaceHolder = "[ROM]";
    public const string CorePlaceHolder = "[CORE]";

    public static string StdDesktopEntry(string friendlyName, string core)
    {
        core = (!string.IsNullOrEmpty(core)) ? core : CorePlaceHolder;
        friendlyName = (!string.IsNullOrEmpty(friendlyName)) ? friendlyName : NamePlaceHolder;
        return DesktopEntryName(friendlyName, core);
    }
    
    public static string DesktopEntryName(string fileName, string core)
    {
        const string appendix = "retroarch.";
        const string whiteSpace = " ";
        const string whiteSpaceReplacer = "_";
            
        fileName = fileName.Replace(whiteSpace, whiteSpaceReplacer);
        fileName = fileName.Insert(0, $"{core}.");
        fileName = fileName.Insert(0, appendix);
        return fileName;
    }

    public static ShortcutterOutput FixCoreNameForOutput(ShortcutterOutput outputToFix, string romCore)
    {
        if (outputToFix.FileName.Contains($"retroarch.{CorePlaceHolder}"))
        {
            outputToFix.FileName = outputToFix.FileName.Replace(CorePlaceHolder, romCore);
            var newPath = FileOps.GetDirAndCombine(outputToFix.FullPath, outputToFix.FileName);
            outputToFix.FullPath = newPath;
        }
        return outputToFix;
    }
}