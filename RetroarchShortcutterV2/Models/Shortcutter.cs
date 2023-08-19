using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinFunc;

namespace RetroarchShortcutterV2.Models
{
    public  class Shortcutter
    {
        public string RAdir { get; set; }
        public string RApath { get; set; }
        public string? ROMdir { get; set; }
        public string? ROMfile { get; set; }
        public string ROMcore { get; set; }
        public string? CONFfile { get; set; }
        public string? ICONfile { get; set; }
        public string Command { get; set; }
        public string? Comment { get; set; }
        public string LNKdir { get; set; }

        public static void BuildShortcut(Shortcutter shortcut, bool DesktopOS = true)
        {
            const string comilla = "\"";
            

            shortcut.RApath = Path.GetDirectoryName(shortcut.RAdir);

            // Adicion de comillas para manejo no directorios inusuales
            if (shortcut.RApath.ElementAt(0) != comilla.ElementAt(0))
            { shortcut.RApath = shortcut.RApath.Insert(0, comilla); }
            if (shortcut.RApath.ElementAt(shortcut.RApath.Length - 1) != comilla.ElementAt(0))
            { shortcut.RApath = shortcut.RApath + comilla; }

            if (shortcut.RAdir.ElementAt(0) != comilla.ElementAt(0))
            { shortcut.RAdir = shortcut.RAdir.Insert(0, comilla); }
            if (shortcut.RAdir.ElementAt(shortcut.RAdir.Length - 1) != comilla.ElementAt(0))
            { shortcut.RAdir = shortcut.RAdir + comilla; }

            if (shortcut.ROMdir.ElementAt(0) != comilla.ElementAt(0))
            { shortcut.ROMdir = shortcut.ROMdir.Insert(0, comilla); }
            if (shortcut.ROMdir.ElementAt(shortcut.ROMdir.Length - 1) != comilla.ElementAt(0))
            { shortcut.ROMdir = shortcut.ROMdir + comilla; }

            shortcut.Command = Commander.CommandBuilder(shortcut.ROMcore, shortcut.ROMdir);
            WinShortcutter.CreateShortcut(shortcut);
            //try { WinShortcutter.CreateShortcut(LNKdir, RAdir, RApath, command, ICONfile); MessageBox.Show("El shortcut fue creado con exito.", "Listo"); }
            //catch { MessageBox.Show("Hubo un error al crear el shortcut. Verifique los campos que ha llenado.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); ; }

        }

        public static void BuildShortcut(Shortcutter shortcut)
        {
            const string comilla = "\"";


            shortcut.RApath = Path.GetDirectoryName(shortcut.RAdir);

            // Adicion de comillas para manejo no directorios inusuales
            if (shortcut.RApath.ElementAt(0) != comilla.ElementAt(0))
            { shortcut.RApath = shortcut.RApath.Insert(0, comilla); }
            if (shortcut.RApath.ElementAt(shortcut.RApath.Length - 1) != comilla.ElementAt(0))
            { shortcut.RApath = shortcut.RApath + comilla; }

            if (shortcut.RAdir.ElementAt(0) != comilla.ElementAt(0))
            { shortcut.RAdir = shortcut.RAdir.Insert(0, comilla); }
            if (shortcut.RAdir.ElementAt(shortcut.RAdir.Length - 1) != comilla.ElementAt(0))
            { shortcut.RAdir = shortcut.RAdir + comilla; }

            if (shortcut.ROMdir.ElementAt(0) != comilla.ElementAt(0))
            { shortcut.ROMdir = shortcut.ROMdir.Insert(0, comilla); }
            if (shortcut.ROMdir.ElementAt(shortcut.ROMdir.Length - 1) != comilla.ElementAt(0))
            { shortcut.ROMdir = shortcut.ROMdir + comilla; }

            shortcut.Command = Commander.CommandBuilder(shortcut.ROMcore, shortcut.ROMdir);
            WinShortcutter.CreateShortcut(shortcut);
            //try { WinShortcutter.CreateShortcut(LNKdir, RAdir, RApath, command, ICONfile); MessageBox.Show("El shortcut fue creado con exito.", "Listo"); }
            //catch { MessageBox.Show("Hubo un error al crear el shortcut. Verifique los campos que ha llenado.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); ; }

        }
    }
}
