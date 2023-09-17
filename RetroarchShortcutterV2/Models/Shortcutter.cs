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
        public static bool BuildWinShortcut(Shortcutter link, bool OS)
        {
            if (!OS) { return false; }
            WinFuncImport.WinFuncMethods CreateShortcut = WinFuncImport.FuncLoader.GetShortcutMethod();

            link.RApath = FileOps.GetDirFromPath(link.RAdir);

            // Adicion de comillas para manejo de directorios no inusuales...
            // para el WorkingDirectory de RetroArch
            link.RApath = Utils.FixUnusualDirectories(link.RApath);

            // para el ejecutable de RetroArch
            link.RAdir = Utils.FixUnusualDirectories(link.RAdir);

            link = Commander.CommandBuilder(link);

            //IList<object>? shortcut_props = CreateObjList(shortcut);       // Crea un nueva IList de objetos que pueden ser null

            var CreateShortcutArgs = new object[]
            {
                link.RAdir, link.RApath, link.Command,
                link.ICONfile, link.Desc, link.LNKdir
            };
            System.Diagnostics.Trace.WriteLine($"Creando {System.IO.Path.GetFileName(link.LNKdir)} para Windows.", "[Info]");
            try { CreateShortcut.MInfo.Invoke(CreateShortcut.ObjInstance, CreateShortcutArgs); return true; }
            catch (System.Exception e)
            {
                System.Diagnostics.Trace.WriteLine($"No se ha podido crear {System.IO.Path.GetFileName(link.LNKdir)}!", "[Erro]");
                System.Diagnostics.Debug.WriteLine($"En {WinFuncImport.FuncLoader.WinFunc}, el elemento {e.Source} a retornado el error '{e.Message}'", "[Erro]");
                return false; 
            }
        }   // El metodo es bool; true si tuvo exito, false en caso contrario

        // Linux
        public static bool BuildLinShorcut(Shortcutter link, bool OS)
        {
            if (OS) { return false; }

            link = Commander.CommandBuilder(link);
            //link.Command = link.Command.Replace("\"", "\'");
            if (string.IsNullOrEmpty(link.ICONfile)) /*{ link.ICONfile = FileOps.GetRAIcons(); }*/
            { link.ICONfile = FileOps.DotDesktopRAIcon; }
            string[] NameFix = FileOps.FixLinkName(link.LNKdir);
            link.LNKdir = NameFix[0];
            
            try { LinFunc.LinShortcutter.CreateShortcutIni(link, NameFix[1]); return true; }
            catch (System.Exception e)
            {
                System.Diagnostics.Trace.WriteLine($"No se ha podido crear '{link.LNKdir}'!", "[Erro]");
                System.Diagnostics.Debug.WriteLine($"En LinShortcutter, el elemento {e.Source} a retornado el error '{e.Message}'", "[Erro]");
                return false; 
            }
        }
    }
}
