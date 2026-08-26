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
using System.Text.RegularExpressions;

namespace RetroLinker.Models.Generic
{
    public static class Utils
    {
        private const char DQ = '\"';
        private const char LinIllegaChar = '/';
        private static readonly char[] WinIllegalChars = [ '|', '\\', '/', '*', '?', '<', '>' ];

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

        public static List<string> ParseArguments(string args)
        {
            var results = new List<string>();
            if (string.IsNullOrWhiteSpace(args)) return results;

            // Just wanted to mention that I HATE regular expressions, but is the most direct solution I found...
            /* Regex Explanation:
             * "([^"]*)"    : Matches content inside double quotes
             * '([^']*)'    : Matches content inside single quotes
             * [^\s'"]+     : Matches sequences of characters that aren't spaces or quotes
             */
            var pattern = @" ""([^""]*)"" | '([^']*)' | ([^\s'"" ]+) ";
            var matches = Regex.Matches(args, pattern, RegexOptions.IgnorePatternWhitespace);

            foreach (Match match in matches) {
                // TODO: Try another foreach
                if (match.Groups[1].Success) results.Add(match.Groups[1].Value);        // Group 1: Double quoted
                else if (match.Groups[2].Success) results.Add(match.Groups[2].Value);   // Group 2: Single quoted
                else if (match.Groups[3].Success) results.Add(match.Groups[3].Value);   // Group 3: Unquoted
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
