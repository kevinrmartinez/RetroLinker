using IWshRuntimeLibrary; // > Ref > COM > Windows Script Host Object
using System;
using System.Collections.Generic;

namespace WinFunc
{
    public class WinShortcutter
    {
        public static void CreateShortcut(string Exec, string path, string command, string? Iconfile, string? Desc, string Link)
        {

            var shell = new WshShell();
            //var shortcut = shell.CreateShortcut(System.IO.Path.GetFullPath(Dest)) as IWshShortcut;
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(Link);
            shortcut.TargetPath = Exec;
            shortcut.Arguments = command;
            shortcut.WorkingDirectory = path;
            if (Iconfile != null) { shortcut.IconLocation = Iconfile; }
            if (Desc != null) { shortcut.Description = Desc; }
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