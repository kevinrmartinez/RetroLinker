using System.Collections.Generic;
using System.IO;


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

            shortcut.RApath = Path.GetDirectoryName(shortcut.RAdir);

            // Adicion de comillas para manejo de directorios no inusuales...
            // para el WorkingDirectory de RetroArch
            Utils.FixUnusualDirectories(shortcut.RApath);

            // para el ejecutable de RetroArch
            Utils.FixUnusualDirectories(shortcut.RAdir);

            shortcut = Commander.CommandBuilder(shortcut);

            IList<object>? shortcut_props = CreateObjList(shortcut);       // Crea un nueva IList de objetos que pueden ser null
            //WinFunc.WinShortcutter.CreateShortcut(shortcut_members);
            try { WinFunc.WinShortcutter.CreateShortcut(shortcut_props); return true; }
            catch { return false; }                                     // El metodo es bool; true si tuvo exito, false en caso contrario
        }

        // Linux
        public static bool BuildLinShorcut(Shortcutter shortcut, bool OS)
        {
            if (OS) { return false; }

            shortcut = Commander.CommandBuilder(shortcut);
            // Llamar al creador de shortcut de Linux
            IList<object>? shortcut_props = CreateObjList(shortcut);       // Crea un nueva IList de objetos que pueden ser null
            try { LinFunc.LinShortcutter.CreateShortcut(shortcut_props); return true; }
            catch { return false; }
        }



        public static IList<object> CreateObjList(Shortcutter shortcut)
        {
            /* Como WinFunc es otro proyecto, no puedo mandar el objuto 'shortcut'
             * Asi que em vez lo arreglo en forma de lista, y mando la lsita a WinFunc*/

            IList<object>? props = new List<object>();
            var properties = typeof(Shortcutter).GetProperties();       // Consigue las propiedades de la clase, retorna PropertyInfo[]
            foreach (var propertyInfo in properties)                    // Recorre cada elemento del array 'properties', guardandolo en 'propertyInfo'
            {
                props.Add(propertyInfo.GetValue(shortcut));  // Añade a la lista 'shortcut_members' el valor obtenido del objeto 'shortcut' en la propiedad indicada por 'propertyInfo'
            }
            return props;
        }
    }
}
