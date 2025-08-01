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
namespace RetroLinker.Models
{
    public static class Commander
    {
        public const string contentless = "Contentless";
        private const string verbose = "-v ";
        private const string fullscreen = "-f ";
        private const string accessibility = "--accessibility ";
        private const string appendConfig = "--appendconfig=";


        public static Shortcutter CommandBuilder(Shortcutter shortcut)
        {
            shortcut.Command = string.Empty;
            
            if (!string.IsNullOrEmpty(shortcut.CONFfile)) shortcut.Command += $"-c {shortcut.CONFfile} ";
            if (!string.IsNullOrEmpty(shortcut.CONFappend)) shortcut.Command += $"{shortcut.CONFappend} ";
            
            shortcut.Command += $"-L {shortcut.ROMcore}";
            if (shortcut.ROMdir != contentless)
            {
                shortcut.Command += $" {shortcut.ROMdir}";
                shortcut.Command += $" {shortcut.PatchArg}";
            }
            
            if (shortcut.AccessibilityB) shortcut.Command = shortcut.Command.Insert(0, accessibility);
            if (shortcut.FullscreenB)    shortcut.Command = shortcut.Command.Insert(0, fullscreen);
            if (shortcut.VerboseB)       shortcut.Command = shortcut.Command.Insert(0, verbose);

            shortcut.Command = shortcut.Command.TrimEnd();
            return shortcut;
        }
        
        //AppendConfig
        public static string ResolveAppendConfigArg(string arg)
        {
            if (!arg.StartsWith(appendConfig)) return string.Empty;
            var path = arg.Substring(appendConfig.Length);
            return Utils.ReverseFixUnusualPaths(path);
        }

        public static string GetAppendConfigArg(string configFile) {
            configFile = Utils.FixUnusualPaths(configFile);
            return appendConfig + configFile;
        }
    }
}
