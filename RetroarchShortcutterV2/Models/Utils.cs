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
            const int FirstElement = 0;
            int LastElement = newdir.Length - 1;
            
            if (newdir.ElementAt(FirstElement) != comilla.ElementAt(0))
            { newdir = newdir.Insert(FirstElement, comilla); }
            if (newdir.ElementAt(LastElement) != comilla.ElementAt(0))
            { newdir += comilla; }
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
