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

using System.Collections.Generic;
using RetroLinker.Models.Generic;

namespace RetroLinker.Models
{
    public static class Commander
    {
        public const string contentless = "Contentless";
        private const string verbose = "-v ";
        private const string fullscreen = "-f ";
        private const string accessibility = "--accessibility ";
        private const string menuOnError = "--load-menu-on-error ";
        private const string appendConfig = "--appendconfig ";
        private const char appendConfigDeli = '|';
        
        public enum PatchType {
            UPS, BPS, IPS,
            XDelta, NoPatch, ExNoPatch
        }
        public static SoftPatch UpsPatch = new("ups", "--ups=", PatchType.UPS);
        public static SoftPatch BpsPatch = new("bps", "--bps=",  PatchType.BPS);
        public static SoftPatch IpsPatch = new("ips", "--ips=",  PatchType.IPS);
        public static SoftPatch XdPatch = new("xdelta", "--xdelta=",   PatchType.XDelta);
        public static SoftPatch NoPatch = new("no", string.Empty,  PatchType.NoPatch);
        public static SoftPatch ExNoPatch = new("explicit-no", "--no-patch", PatchType.ExNoPatch);

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
            if (shortcut.MenuOnErrorB)   shortcut.Command = shortcut.Command.Insert(0, menuOnError);
            if (shortcut.FullscreenB)    shortcut.Command = shortcut.Command.Insert(0, fullscreen);
            if (shortcut.VerboseB)       shortcut.Command = shortcut.Command.Insert(0, verbose);

            shortcut.Command = shortcut.Command.TrimEnd();
            return shortcut;
        }
        
        // SoftPatching
        public static (string, SoftPatch) ResolveSoftPatchingArg(string arg)
        {
            var argException = new System.ArgumentException(@"Invalid patch argument",  nameof(arg));
            var split = arg.Split('=');
            if (split.Length != 2) {
                if ((split.Length == 1) && (split[0] == ExNoPatch.Argument)) return (ExNoPatch.Argument, ExNoPatch);
                throw argException;
            }
            
            var argSplit = split[0] + '=';
            var fileSplit = Utils.ReverseFixUnusualPaths(split[1]);
            if (argSplit == UpsPatch.Argument) return (fileSplit, UpsPatch);
            else if (argSplit == BpsPatch.Argument) return (fileSplit, BpsPatch);
            else if (argSplit == IpsPatch.Argument) return (fileSplit, IpsPatch);
            else if (argSplit == XdPatch.Argument) return (fileSplit, XdPatch);
            else throw argException;
        }

        public static string GetSoftPatchingArg(string patchFile, SoftPatch patchType)
        {
            switch (patchType.PatchType)
            {
                case PatchType.UPS:
                case PatchType.BPS:
                case PatchType.IPS:
                case PatchType.XDelta:
                    var file = Utils.FixUnusualPaths(patchFile);
                    return patchType.Argument + file;
                case PatchType.NoPatch:
                case PatchType.ExNoPatch:
                    return patchType.Argument;
                default:
                    throw new System.ArgumentOutOfRangeException();
            }
        }
        
        // AppendConfig
        public static (string, List<string>) ResolveAppendConfigArg(string arg)
        {
            if (!arg.StartsWith(appendConfig))
                throw new System.ArgumentException(@"Invalid append config argument", nameof(arg));
            var pathsCombined = arg.Substring(appendConfig.Length);
            pathsCombined = Utils.ReverseFixUnusualPaths(pathsCombined);
            var paths = pathsCombined.Split(appendConfigDeli);
            return (pathsCombined, new List<string>(paths));
        }

        public static string GetAppendConfigArg(List<string> configFiles) {
            string appendConfigArg;
            if (configFiles.Count == 1)
                appendConfigArg = Utils.FixUnusualPaths(configFiles[0]);
            else
            {
                appendConfigArg = string.Join(appendConfigDeli, configFiles);
                appendConfigArg = Utils.FixUnusualPaths(appendConfigArg);
            }
            return appendConfig + appendConfigArg;
        }
    }

    public record SoftPatch(string ExtName, string Argument, Commander.PatchType PatchType);
}
