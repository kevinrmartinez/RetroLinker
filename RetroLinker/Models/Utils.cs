/*
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
using System.Diagnostics.CodeAnalysis;

namespace RetroLinker.Models
{
    public static class Utils
    {
        private const char DQ = '\"';
        private const char LinIllegaChar = '/';
        private static readonly char[] WinIllegalChars = [ '|', '\\', '/', '*', '?', '<', '>' ];
        
        public static string FixUnusualPaths(string path)
        {
            // TODO: This 'fix' only puts the path between double quotes
            // This deserves a remake
            if (!ContainsUnusualCharacters(path)) return path;
            
            const int firstElement = 0;
            int lastElement = path.Length - 1;
            if (path[firstElement] != DQ)
            { path = path.Insert(firstElement, DQ.ToString()); }
            if (path[lastElement] != DQ)
            { path += DQ; }
            return path;
        }
        
        private static bool ContainsUnusualCharacters(string path)
        {
            // Define a list of characters that are considered unusual
            char[] unusualCharacters = [ ' ', '$', '&', '`', '|', '\\', '*', '?', '<', '>', '^', '%'];

            // Check if the path contains any unusual characters
            foreach (var c in path)
            {
                if (Array.IndexOf(unusualCharacters, c) != -1) return true;
            }
            return false;
        }

        public static string ReverseFixUnusualPaths(string path)
        {
            if (!HasDoubleQuotes(path)) return path;
            var noDQ = path.Split(DQ);
            return noDQ[1];
        }

        public static string TwoDoubleQuotes(string text)
        {
            int y = 0;
            bool notEnded = true;
            while (notEnded)
            {
                if (y >= (text.Length - 1)) 
                    notEnded = false;
                for (int i = y; i < text.Length; i++)
                {
                    y = i;
                    if (text[i] != DQ)
                    {
                        y++; 
                        continue;
                    }
                    text = text.Insert(y, DQ.ToString());
                    y += 2;
                    break;
                }
            }
            
            return text;
        }

        private static bool HasDoubleQuotes(string path)
        {
            int lastCharIndex = path.Length - 1;
            return ((path[0] == DQ) && (path[lastCharIndex] == DQ));
        }

        public static List<string> ExtractClassProperties([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type type)
        {
            var props = type.GetProperties();
            var members = new List<string>();

            foreach (var member in props)
                members.Add(member.Name);
            
            return members;
        }

        public static string GetStringFromList(IEnumerable<string> list)
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
