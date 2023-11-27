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
namespace RetroarchShortcutterV2.Models
{
    public class Commander
    {
        public const string contentless = "Contentless";
        const string verbose = "-v ";
        const string fullscreen = "-f ";
        const string accessibility = "--accessibility ";


        public static Shortcutter CommandBuilder(Shortcutter shortcut)
        {
            shortcut.Command = $"-L {shortcut.ROMcore}";
            if (!string.IsNullOrEmpty(shortcut.CONFfile)) 
            { shortcut.Command = shortcut.Command.Insert(0, $"-c {shortcut.CONFfile} "); }
            if (shortcut.ROMdir != contentless) 
            { shortcut.Command += $" {shortcut.ROMdir}"; }

            // Parametros extras comunes
            if (shortcut.AccessibilityB) { shortcut.Command = shortcut.Command.Insert(0, accessibility); }
            if (shortcut.FullscreenB) { shortcut.Command = shortcut.Command.Insert(0, fullscreen); }
            if (shortcut.VerboseB) { shortcut.Command = shortcut.Command.Insert(0, verbose); }

            return shortcut;
        }
    }
}
