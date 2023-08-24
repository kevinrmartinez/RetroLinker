using System.Linq;

namespace RetroarchShortcutterV2.Models
{
    public class Utils
    {
        const string comilla = "\"";
        public static string FixUnusualDirectories(string dir)
        {
            string newdir = dir;
            if (newdir.ElementAt(0) != comilla.ElementAt(0))                    // Si la primera letra de newdir no es ", entonces se le agrega
            { newdir = newdir.Insert(0, comilla); }
            if (newdir.ElementAt(newdir.Length - 1) != comilla.ElementAt(0))    // Si la última letra de newdir no es ", entonces se le agrega
            { newdir = newdir + comilla; }
            return newdir;
        }
    }
}
