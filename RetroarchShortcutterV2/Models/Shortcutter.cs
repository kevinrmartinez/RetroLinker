using System.Collections.Generic;
using System.IO;
using System.Linq;
using WinFunc;


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
        public string? Comment { get; set; }    // 8
        public string LNKdir { get; set; }      // 9

        const string comilla = "\"";


        public static bool BuildWinShortcut(Shortcutter shortcut, bool OS)
        {
            if (!OS) { return false; }

            shortcut.RApath = Path.GetDirectoryName(shortcut.RAdir);

            // Adicion de comillas para manejo de directorios no inusuales...
            // para el WorkingDirectory de RetroArch
            if (shortcut.RApath.ElementAt(0) != comilla.ElementAt(0))
            { shortcut.RApath = shortcut.RApath.Insert(0, comilla); }
            if (shortcut.RApath.ElementAt(shortcut.RApath.Length - 1) != comilla.ElementAt(0))
            { shortcut.RApath = shortcut.RApath + comilla; }

            // para el ejecutable de RetroArch
            if (shortcut.RAdir.ElementAt(0) != comilla.ElementAt(0))
            { shortcut.RAdir = shortcut.RAdir.Insert(0, comilla); }
            if (shortcut.RAdir.ElementAt(shortcut.RAdir.Length - 1) != comilla.ElementAt(0))
            { shortcut.RAdir = shortcut.RAdir + comilla; }

            // para el directorio de la ROM
            if (shortcut.ROMdir.ElementAt(0) != comilla.ElementAt(0))
            { shortcut.ROMdir = shortcut.ROMdir.Insert(0, comilla); }
            if (shortcut.ROMdir.ElementAt(shortcut.ROMdir.Length - 1) != comilla.ElementAt(0))
            { shortcut.ROMdir = shortcut.ROMdir + comilla; }

            shortcut.Command = Commander.CommandBuilder(shortcut.ROMcore, shortcut.ROMdir);

            /* Como WinFunc es otro proyecto, no puedo mandar el objuto 'shortcut'
             * Asi que em vez lo arreglo en forma de lista, y mando la lsita a WinFunc*/

            IList<object>? shortcut_members = new List<object>();       // Crea un nueva IList de objetos que pueden ser null
            var properties = typeof(Shortcutter).GetProperties();       // Consigue las propiedades de la clase, retorna PropertyInfo[]
            foreach (var propertyInfo in properties)                    // Recorre cada elemento del array 'properties', guardandolo en 'propertyInfo'
            {
                shortcut_members.Add(propertyInfo.GetValue(shortcut));  // Añade a la lista 'shortcut_members' el valor obtenido del objeto 'shortcut' en la propiedad indicada por 'propertyInfo'
            }
            //WinShortcutter.CreateShortcut(shortcut_members);
            try { WinShortcutter.CreateShortcut(shortcut_members); return true; }
            catch { return false; }                                     // El metodo es bool; true si tuvo exito, false en caso contrario
        }


        public static bool BuildLinShorcut(Shortcutter shortcut, bool OS)
        {
            if (OS) { return false; }

            // para el directorio de la ROM
            if (shortcut.ROMdir.ElementAt(0) != comilla.ElementAt(0))
            { shortcut.ROMdir = shortcut.ROMdir.Insert(0, comilla); }
            if (shortcut.ROMdir.ElementAt(shortcut.ROMdir.Length - 1) != comilla.ElementAt(0))
            { shortcut.ROMdir = shortcut.ROMdir + comilla; }

            shortcut.Command = Commander.CommandBuilder(shortcut.ROMcore, shortcut.ROMdir);
            // Llamar al creador de shortcut de Linux
            return true;
        }
    }
}
