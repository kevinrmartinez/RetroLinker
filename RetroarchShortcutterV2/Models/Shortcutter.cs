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
        public string ROMdir { get; set; }
        public string ROMfile { get; set; }
        public string ROMcore { get; set; }
        public string CONFfile { get; set; }
        public string ICONfile { get; set; }
        public string LNKdir { get; set; }

        public void BuildShortcut(Shortcutter shortcut, bool DesktopOS = true)
        {
            const string comilla = "\"";
            string command;

            RApath = Path.GetDirectoryName(RAdir);

            // Adicion de comillas para manejo no directorios inusuales
            if (RApath.ElementAt(0) != comilla.ElementAt(0))
            { RApath = RApath.Insert(0, comilla); }
            if (RApath.ElementAt(RApath.Length - 1) != comilla.ElementAt(0))
            { RApath = RApath + comilla; }

            if (RAdir.ElementAt(0) != comilla.ElementAt(0))
            { RAdir = RAdir.Insert(0, comilla); }
            if (RAdir.ElementAt(RAdir.Length - 1) != comilla.ElementAt(0))
            { RAdir = RAdir + comilla; }

            if (ROMdir.ElementAt(0) != comilla.ElementAt(0))
            { ROMdir = ROMdir.Insert(0, comilla); }
            if (ROMdir.ElementAt(ROMdir.Length - 1) != comilla.ElementAt(0))
            { ROMdir = ROMdir + comilla; }

            command = Commander.CommandBuilder(ROMcore, ROMdir);
            WinShortcutter.CreateShortcut(LNKdir, RAdir, RApath, command, null);
            //try { WinShortcutter.CreateShortcut(LNKdir, RAdir, RApath, command, ICONfile); MessageBox.Show("El shortcut fue creado con exito.", "Listo"); }
            //catch { MessageBox.Show("Hubo un error al crear el shortcut. Verifique los campos que ha llenado.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); ; }

        }
    }
}
