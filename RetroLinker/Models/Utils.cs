﻿/*
    A .NET GUI application to help create desktop links of games running on RetroArch.
    Copyright (C) 2023  Kevin Rafael Martinez Johnston

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    any later version.

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

namespace RetroLinker.Models
{
    public static class Utils
    {
        const string DoubleQuotes = "\"";
        public static string FixUnusualDirectories(string dir)
        {
            string newdir = dir;
            const int FirstElement = 0;
            int LastElement = newdir.Length - 1;
            
            if (newdir.ElementAt(FirstElement) != DoubleQuotes.ElementAt(0))
            { newdir = newdir.Insert(FirstElement, DoubleQuotes); }
            if (newdir.ElementAt(LastElement) != DoubleQuotes.ElementAt(0))
            { newdir += DoubleQuotes; }
            return newdir;
        }

        public static List<string> ExtractClassProperties(Type type)
        {
            //var _type = _class.GetType();
            var props = type.GetProperties();
            var members = new List<string>();

            foreach (var member in props)
            {
                members.Add(member.Name);
            }
            
            return members;
        }

        public static string GetStringFromList(List<string> list)
        {
            string result = "";
            foreach (var item in list)
            {
                result += $"{item}\n";
            }
            return result;
        }
    }
}
