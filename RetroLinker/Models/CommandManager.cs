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

using System.Collections.Generic;
using System.Linq;
using RetroLinker.Models.Generic;

namespace RetroLinker.Models
{
    public static class CommandManager
    {
        public const string contentless = "Contentless";
        private const char optValueSeparator = '=';
        private const string verbose = "-v ";
        private const string fullscreen = "-f ";
        private const string accessibility = "--accessibility ";
        private const string menuOnError = "--load-menu-on-error ";
        private const string appendConfig = "--appendconfig=";
        private const char appendConfigDeli = '|';
        private const string subsystem = "--subsystem=";
        
        public const string UpsExt = "ups";
        public const string BpsExt = "bps";
        public const string IpsExt = "ips";
        public const string XdExt = "xdelta";
        private const string UpsOpt = $"--{UpsExt}=";
        private const string BpsOpt = $"--{BpsExt}=";
        private const string IpsOpt = $"--{IpsExt}=";
        private const string XdOpt = $"--{XdExt}=";
        private const string NpOpt = "--no-patch";
        public static SoftPatch UpsPatch = new(UpsExt, UpsOpt, ROMPatchType.UPS);
        public static SoftPatch BpsPatch = new(BpsExt, BpsOpt,  ROMPatchType.BPS);
        public static SoftPatch IpsPatch = new(IpsExt, IpsOpt,  ROMPatchType.IPS);
        public static SoftPatch XdPatch = new(XdExt, XdOpt,   ROMPatchType.XDelta);
        public static SoftPatch NoPatch = new("no", string.Empty,  ROMPatchType.NoPatch);
        public static SoftPatch ExNoPatch = new("explicit-no", NpOpt, ROMPatchType.ExNoPatch);

        public static string CommandBuilder(Shortcutter shortcut)
        {
            var command = string.Empty;
            
            if (!string.IsNullOrEmpty(shortcut.CONFfile)) command += $"-c {shortcut.CONFfile} ";
            if (!string.IsNullOrEmpty(shortcut.CONFappend)) command += $"{shortcut.CONFappend} ";
            
            command += $"-L {shortcut.ROMcore}";
            if (shortcut.ROMdir != contentless) {
                command += $" {shortcut.ROMdir}";
                command += $" {shortcut.PatchArg}";
            }
            
            // Subsystem always goes last (Because I say so :p)
            if (!string.IsNullOrEmpty(shortcut.SubsysArg)) command += $" {shortcut.SubsysArg} "; 
            
            if (shortcut.AccessibilityB) command = command.Insert(0, accessibility);
            if (shortcut.MenuOnErrorB)   command = command.Insert(0, menuOnError);
            if (shortcut.FullscreenB)    command = command.Insert(0, fullscreen);
            if (shortcut.VerboseB)       command = command.Insert(0, verbose);

            return command.TrimEnd();
        }

        private static (string, string) GetOptionAndArgument(string arg)
        {
            var firstEqual = arg.IndexOf(optValueSeparator);
            if (firstEqual < 0) return (arg, string.Empty);
            
            var option = arg.Substring(0, firstEqual);
            var argument =  arg.Substring(firstEqual + 1);
            return (option, argument);
        }

        public static string GetArgumentNoOption(string arg) {
            var firstEqual = arg.IndexOf(optValueSeparator);
            return (firstEqual > 0) ? arg.Substring(firstEqual + 1) : arg;
        }
        
        // SoftPatching
        public static (string, SoftPatch) ResolveSoftPatchingArg(string arg)
        {
            var argException = new System.ArgumentException(@"Invalid patch argument: '" + arg + @"'",  nameof(arg));
            var (patchOpt, argument) = GetOptionAndArgument(arg);
            if (string.IsNullOrEmpty(argument)) 
                return (patchOpt == ExNoPatch.Option) ? (ExNoPatch.Option, ExNoPatch) : throw argException;
            
            
            var optEval = patchOpt + optValueSeparator;
            var argEval = Utils.ReversePutPathBetweenQuotes(argument);
            return optEval switch
            {
                UpsOpt => (argEval, UpsPatch),
                BpsOpt => (argEval, BpsPatch),
                IpsOpt => (argEval, IpsPatch),
                XdExt => (argEval, XdPatch), 
                _ => throw argException
            };
        }

        public static string CreateSoftPatchingArg(string patchFile, SoftPatch patchType)
        {
            switch (patchType.PatchType)
            {
                case ROMPatchType.UPS:
                case ROMPatchType.BPS:
                case ROMPatchType.IPS:
                case ROMPatchType.XDelta:
                    var file = Utils.PutPathBetweenQuotes(patchFile);
                    return patchType.Option + file;
                case ROMPatchType.NoPatch:
                case ROMPatchType.ExNoPatch:
                    return patchType.Option;
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(patchType));
            }
        }
        
        
        // AppendConfig
        public static (string, List<string>) ResolveAppendConfigArg(string arg)
        {
            if (!arg.StartsWith(appendConfig))
                throw new System.ArgumentException(@"Invalid append config argument: '" + arg + @"'", nameof(arg));
            
            var pathsCombined = GetArgumentNoOption(arg);
            pathsCombined = Utils.ReversePutPathBetweenQuotes(pathsCombined);
            var paths = pathsCombined.Split(appendConfigDeli);
            return (pathsCombined, new List<string>(paths));
        }

        public static string CreateAppendConfigArg(List<string> configFiles) {
            string appendConfigArg;
            if (configFiles.Count == 1)
                appendConfigArg = Utils.PutPathBetweenQuotes(configFiles.First());
            else {
                appendConfigArg = string.Join(appendConfigDeli, configFiles);
                appendConfigArg = Utils.PutPathBetweenQuotes(appendConfigArg);
            }
            return appendConfig + appendConfigArg;
        }
        
        // Subsystem
        public static (string, List<string>) ResolveSubsystemArg(string arg)
        {
            if (!arg.StartsWith(subsystem))
                throw new System.ArgumentException(@"Invalid subsystem argument: '" + arg + @"'", nameof(arg));
            
            var noOption = GetArgumentNoOption(arg);
            var subsys = noOption.Split(' ').First();
            var fullSubsysArgs = noOption.Substring(subsys.Length + 1);
            var subsysArgs = Utils.ParseArguments(fullSubsysArgs);
            return (subsys,  subsysArgs);
        }

        public static string CreateSubsystemArg(string subsys, ICollection<string> subsysArgs)
        {
            var arg = subsystem + subsys;
            var fixedArgs = new List<string>();
            foreach (var subsysArg in subsysArgs)
                fixedArgs.Add(Utils.PutPathBetweenQuotes(subsysArg));
            arg += " " + Utils.GetSingleLineStringFromList(fixedArgs);
            return arg;
        }

        public static void TestAllOptions()
        {
            var patchArg = "--ups=\"path/to/rom.bin\"";
            var subsysArg = "--subsystem=abc \"path/to/rom1.bin\" \"path/to/rom 2.bin\"";
            var CONFappend = "--appendconfig=\"/path/to/config1.conf|/path/to/config 2.conf|/path/to/config3.conf\"";
            
            var patchResolved = ResolveSoftPatchingArg(patchArg);
            var subsysResolved = ResolveSubsystemArg(subsysArg);
            var confResolved = ResolveAppendConfigArg(CONFappend);
            
            Logger.LogDebg($"{patchResolved.Item1}, {patchResolved.Item2.PatchType}");
            Logger.LogDebg($"{subsysResolved.Item1}, {Utils.GetMultiLineStringFromList(subsysResolved.Item2)}");
            Logger.LogDebg($"{confResolved.Item1}, {Utils.GetMultiLineStringFromList(confResolved.Item2)}");
            
            Logger.LogDebg(CreateSoftPatchingArg(patchResolved.Item1, patchResolved.Item2));
            Logger.LogDebg(CreateSubsystemArg(subsysResolved.Item1, subsysResolved.Item2));
            Logger.LogDebg(CreateAppendConfigArg(confResolved.Item2));
        }
    }
    
    public enum ROMPatchType {
        UPS, BPS, IPS,
        XDelta, NoPatch, ExNoPatch
    }

    public record SoftPatch(string ExtName, string Option, ROMPatchType PatchType);
}
