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
using IWshRuntimeLibrary; // > Ref > COM > Windows Script Host Object

namespace RetroLinkerWin
{
    public class WinShortcutter
    {
        public static void CreateShortcut(string Exec, string path, string command, string Iconfile, string Desc, string Link)
        {
            var shell = new WshShell();
            //var shortcut = shell.CreateShortcut(System.IO.Path.GetFullPath(Dest)) as IWshShortcut;
            var shortcut = (IWshShortcut)shell.CreateShortcut(Link);
            shortcut.TargetPath = Exec;
            shortcut.Arguments = command;
            shortcut.WorkingDirectory = path;
            shortcut.Description = Desc;
            if (!string.IsNullOrEmpty(Iconfile)) { shortcut.IconLocation = Iconfile; }
            shortcut.Save();
        }

        public static void CreateShortcut(IList<object> _shortcut)
        {
            // Los indices estan relacionados a las propiedades del objeto 'shortcut'
            // Estos se pueden ver comentado en la clase 'Shortcutter'
            var shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(_shortcut[9].ToString());
            if (_shortcut[6] != null) { shortcut.IconLocation = _shortcut[6].ToString(); }
            shortcut.TargetPath = _shortcut[0].ToString();
            if (_shortcut[8] != null) { shortcut.Description = _shortcut[8].ToString(); }
            shortcut.WorkingDirectory = _shortcut[1].ToString();
            shortcut.Arguments = _shortcut[7].ToString();
            shortcut.Save();
        }

#if DEBUG
        public static void NotepadShortcut()
        {
            object shDesktop = (object)"Desktop";
            WshShell shell = new WshShell();
            string shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + @"\Notepad.lnk";
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
            shortcut.Description = "New shortcut for a Notepad";
            shortcut.Hotkey = "Ctrl+Shift+N";
            shortcut.TargetPath = Environment.GetFolderPath(Environment.SpecialFolder.System) + @"\notepad.exe";
            shortcut.Save();
        }
#endif
    }
}