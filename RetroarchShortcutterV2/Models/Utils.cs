/*
    A .NET GUI application to help create desktop links of games running on RetroArch.
    Copyright (C) 2023  Kevin Rafael Martinez Johnston

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
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
