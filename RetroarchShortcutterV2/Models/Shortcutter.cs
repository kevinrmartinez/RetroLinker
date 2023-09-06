

namespace RetroarchShortcutterV2.Models
{
    public class Shortcutter
    {
        public string RAdir { get; set; }       // 0
        public string RApath { get; set; }      // 1
        public string? ROMdir { get; set; }     // 2
        public string? ROMfile { get; set; }    // 3
        public string ROMcore { get; set; }     // 4
        public string? CONFfile { get; set; }   // 5
        public string? ICONfile { get; set; }   // 6
        public string Command { get; set; }     // 7
        public string? Desc { get; set; }       // 8
        public string LNKdir { get; set; }      // 9
        public bool verboseB = false;           // 10
        public bool fullscreenB = false;        // 11
        public bool accessibilityB = false;     // 12

        
        // Windows
        public static bool BuildWinShortcut(Shortcutter shortcut, bool OS)
        {
            if (!OS) { return false; }
            WinFuncImport.WinFuncMethods CreateShortcut = WinFuncImport.FuncLoader.GetShortcutMethod();

            shortcut.RApath = FileOps.GetDirFromPath(shortcut.RAdir);

            // Adicion de comillas para manejo de directorios no inusuales...
            // para el WorkingDirectory de RetroArch
            shortcut.RApath = Utils.FixUnusualDirectories(shortcut.RApath);

            // para el ejecutable de RetroArch
            shortcut.RAdir = Utils.FixUnusualDirectories(shortcut.RAdir);

            // para el icono del link
            //if (shortcut.ICONfile != null) 
            //{ shortcut.ICONfile = Utils.FixUnusualDirectories(shortcut.ICONfile); }

            shortcut = Commander.CommandBuilder(shortcut);

            //IList<object>? shortcut_props = CreateObjList(shortcut);       // Crea un nueva IList de objetos que pueden ser null

            var CreateShortcutArgs = new object[]
            {
                shortcut.LNKdir, shortcut.ICONfile, shortcut.RAdir,
                shortcut.Desc, shortcut.RApath, shortcut.Command
            };
            
            try { CreateShortcut.mInfo.Invoke(CreateShortcut.objInstance, CreateShortcutArgs); return true; }
            catch { return false; }                                     // El metodo es bool; true si tuvo exito, false en caso contrario
        }

        // Linux
        public static bool BuildLinShorcut(Shortcutter shortcut, bool OS)
        {
            if (OS) { return false; }

            shortcut = Commander.CommandBuilder(shortcut);
            
            try { LinFunc.LinShortcutter.CreateShortcutIni(shortcut); return true; }
            catch { return false; }
        }
    }
}
