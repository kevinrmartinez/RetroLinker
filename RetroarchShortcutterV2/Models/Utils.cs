using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.JavaScript;

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

        public static List<string> ExtractClassProperties(Type _type)
        {
            //var _type = _class.GetType();
            var _props = _type.GetProperties();
            List<string> members = new List<string>();

            foreach (var member in _props)
            {
                members.Add(member.Name);
            }
            
            return members;
        }
    }
}
