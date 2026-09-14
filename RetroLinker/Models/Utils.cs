/*
    RetroLinker: A .NET GUI application to help create desktop links of games running on RetroArch.
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
using System.Numerics;
using System.Text;

namespace RetroLinker.Models
{
    public static class Utils
    {
        private const char SQ = '\'';
        private const char DQ = '\"';
        private const char LinIllegaChar = '/';
        private static readonly char[] WinIllegalChars = [ '|', '\\', '/', '*', '?', '<', '>' ];

        private static bool HasSingleQuotes(string path) => path.StartsWith(SQ) && path.EndsWith(SQ);
        private static bool HasDoubleQuotes(string path) => path.StartsWith(DQ) && path.EndsWith(DQ);
        
        public static string PutPathBetweenQuotes(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || HasDoubleQuotes(path)) return path;
            path = DQ + path;
            path += DQ;
            return path;
        }

        public static string ReversePutPathBetweenQuotes(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !HasDoubleQuotes(path)) return path;
            path = path.TrimStart(DQ);
            path = path.TrimEnd(DQ);
            return path;
        }

        public static string TwoDoubleQuotes(string text) => text.Replace(DQ.ToString(), "\"\"");

        // public static List<string> ExtractClassProperties([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type type)
        // {
        //     var props = type.GetProperties();
        //     var members = new List<string>();
        //
        //     foreach (var member in props)
        //         members.Add(member.Name);
        //     
        //     return members;
        // }
        
        public static string GetSingleLineStringFromList(IEnumerable<string> list, char separator = ' ') {
            var result = string.Empty;
            foreach (var item in list)
                result += $"{item}{separator}";
            return result.TrimEnd(separator);
        }

        public static string GetMultiLineStringFromList(IEnumerable<string> list) {
            var result = string.Empty;
            foreach (var item in list)
                result += $"{item}\n";
            return result;
        }
        
        public static string GenerateBase64(string objectString)
        {   // Solution thanks to Kevin Driedger @ Stackoverflow.com
            var objectBytes = Encoding.UTF8.GetBytes(objectString);
            var object64 = Convert.ToBase64String(objectBytes);
            return object64;
        }
        
        public static List<string> ParseSingleLineArguments(string args, char separator = ' ')
        {
            var results = new List<string>();
            var sqStart = $"{separator}{SQ}";
            // var sqEnd = $"{SQ}{separator}";
            var dqStart = $"{separator}{DQ}";
            // var dqEnd = $"{DQ}{separator}";
            
            var wrk = args.Insert(0, separator.ToString());
            wrk += separator.ToString();

            if (wrk.Contains(sqStart) || wrk.Contains(dqStart))
            { 
               var newSep = $"-_{Random.Shared.Next().ToString()}_-";
               wrk = wrk.Replace(sqStart, newSep); 
               wrk = wrk.Replace(dqStart, newSep);
               var split = wrk.Split(newSep, StringSplitOptions.RemoveEmptyEntries);
               foreach (var s in split)
                   results.Add(s.TrimEnd(SQ, DQ, separator).TrimStart());
            }
            else {
                results.AddRange(wrk.Split(separator, StringSplitOptions.RemoveEmptyEntries));
            }

            return results;
        }
        
        // Source - https://stackoverflow.com/a/33776103
        // Posted by arviman, modified by community.
        // Retrieved 2026-08-26, License - CC BY-SA 4.0
        private static readonly HashSet<Type> NumericTypes =
        [
            typeof(int), typeof(long), typeof(Int128),
            typeof(short), typeof(sbyte), typeof(double), 
            typeof(decimal), typeof(float), typeof(Half), 
            typeof(uint), typeof(ulong), typeof(ushort), 
            typeof(byte), typeof(UInt128)
        ];
        
        public static bool IsNumericType(Type type) => NumericTypes.Contains(Nullable.GetUnderlyingType(type) ?? type);

        public static bool GetNumberFromObject<T>(object? obj, IFormatProvider? parserFormat, out T? numberOrDefault) where T : INumber<T?>
        {
            if (obj is not null)
            {
                if (obj is T number) {
                    numberOrDefault = number;
                    return true;
                }
                if (IsNumericType(obj.GetType())) {
                    numberOrDefault = (T?)obj;
                    return true;
                }
                if (obj.ToString() is { } str)
                    if (T.TryParse(str, parserFormat, out var result)) {
                        numberOrDefault = result;
                        return true;
                    } 
            }
            numberOrDefault = default;
            return false;
        }
    }
}
