using IWshRuntimeLibrary; // > Ref > COM > Windows Script Host Object;

namespace WinFunc
{
    public class WinShhortcutter
    {
        public static void CreateShortcut(string Link, string Exec, string Path, string command, string Iconfile)
        {

            var shell = new WshShell();
            //var shortcut = shell.CreateShortcut(System.IO.Path.GetFullPath(Dest)) as IWshShortcut;
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(Link);
            if (Iconfile != null) { shortcut.IconLocation = Iconfile; }
            shortcut.TargetPath = Exec;
            shortcut.WorkingDirectory = Path;
            shortcut.Arguments = command;

            shortcut.Save();
        }

        public static void CreateShortcut(string Link, string Exec, string Path, string command, string Iconfile, string Desc)
        {

            var shell = new WshShell();
            //var shortcut = shell.CreateShortcut(System.IO.Path.GetFullPath(Dest)) as IWshShortcut;
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(Link);
            if (Iconfile != null) { shortcut.IconLocation = Iconfile; }
            shortcut.TargetPath = Exec;
            shortcut.Description = Desc;
            shortcut.WorkingDirectory = Path;
            shortcut.Arguments = command;
            shortcut.Save();
        }

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
    }
}