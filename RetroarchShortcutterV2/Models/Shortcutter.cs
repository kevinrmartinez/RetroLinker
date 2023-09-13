namespace RetroarchShortcutterV2.Models
{
    public class Shortcutter
    {
        #region Object
        public string RAdir { get; set; }       // 0
        public string RApath { get; set; }      // 1
        public string ROMdir { get; set; }     // 2
        public string? ROMfile { get; set; }    // 3
        public string ROMcore { get; set; }     // 4
        public string? CONFfile { get; set; }   // 5
        public string? ICONfile { get; set; }   // 6
        public string Command { get; set; }     // 7
        public string? Desc { get; set; }       // 8
        public string LNKdir { get; set; }      // 9
        public bool VerboseB { get; set; }      // 10
        public bool FullscreenB { get; set; }   // 11
        public bool AccessibilityB { get; set; }// 12

        public Shortcutter()    //PENDIENTE: quizas sea mejor predefinir todo a string.Empty
        {
            RAdir = string.Empty;
            RApath = string.Empty;
            ROMdir = string.Empty;
            ROMcore = string.Empty;
            Command = string.Empty;
            LNKdir = string.Empty;
            VerboseB = false;
            FullscreenB = false;
            AccessibilityB = false;
        }
        #endregion


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

            shortcut = Commander.CommandBuilder(shortcut);

            //IList<object>? shortcut_props = CreateObjList(shortcut);       // Crea un nueva IList de objetos que pueden ser null

            var CreateShortcutArgs = new object[]
            {
                shortcut.RAdir, shortcut.RApath, shortcut.Command,
                shortcut.ICONfile, shortcut.Desc, shortcut.LNKdir
            };
            System.Diagnostics.Debug.WriteLine($"Creando {System.IO.Path.GetFileName(shortcut.LNKdir)} para Windows.", "Info");
            try { CreateShortcut.MInfo.Invoke(CreateShortcut.ObjInstance, CreateShortcutArgs); return true; }
            catch (System.Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"No se ha podido crear {System.IO.Path.GetFileName(shortcut.LNKdir)}!", "Erro");
                System.Diagnostics.Debug.WriteLine($"En {WinFuncImport.FuncLoader.WinFunc}, el elemento {e.Source} a retornado el error '{e.Message}'", "Erro");
                return false; 
            }
        }   // El metodo es bool; true si tuvo exito, false en caso contrario

        // Linux
        public static bool BuildLinShorcut(Shortcutter shortcut, bool OS)
        {
            if (OS) { return false; }

            shortcut = Commander.CommandBuilder(shortcut);
            
            try { LinFunc.LinShortcutter.CreateShortcutIni(shortcut); return true; }
            catch (System.Exception e)
            {
                System.Diagnostics.Trace.WriteLine($"No se ha podido crear {System.IO.Path.GetFileName(shortcut.LNKdir)}!", "Erro");
                System.Diagnostics.Debug.WriteLine($"En LinShortcutter, el elemento {e.Source} a retornado el error '{e.Message}'", "Erro");
                return false; 
            }
        }
    }
}
