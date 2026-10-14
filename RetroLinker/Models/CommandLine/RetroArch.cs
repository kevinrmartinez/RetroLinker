/*
    RetroLinker: A .NET GUI application to help create desktop links of games running on RetroArch.
    Copyright (C) 2026  Kevin Rafael Martinez Johnston

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
using CommandLine;
using RetroLinker.Models.DesktopLink;

namespace RetroLinker.Models.CommandLine;

public static partial class Manager
{
    /* Due to how CommandLine works, the options are formatted to text sorted by the 'longName' attribute (A->Z)
     * To export the way we want, we'll have to separete the operation in 6:
     * Each operation will just create the text for its group, and everything will be concatenated at the end.
     */
    private class RetroArch
    {
        // Toggles
        [Option('f', "fullscreen", Default = false,
            HelpText = "Start the program in fullscreen regardless of config setting.")]
        public bool G1_Fullscreen { get; set; }

        [Option("accessibility", Default = false,
            HelpText = "Enables accessibility for blind users using text-to-speech.")]
        public bool G1_Accessibility { get; set; }
        
        [Option("load-menu-on-error", Default = false,
            HelpText = "Open menu instead of quitting if specified core or content fails to load.")]
        public bool G1_MenuOnError { get; set; }
        
        [Option('v', "verbose", Default = false,
            HelpText = "Verbose logging.")]
        public bool G1_Verbose { get; set; }

        // Log
        [Option("log-file", 
            HelpText = "Log messages to a file.")]
        public string? G2_LogFile { get; set; }

        // Config
        [Option('c', "config", 
            HelpText = "Path for config file.")]
        public string? G3_Config { get; set; }

        [Option("appendconfig", Separator = sepPipe,
            HelpText = "Extra config files are loaded in, and take priority over config selected in -c (or default).")]
        public IEnumerable<string>? G3_AppendConfig { get; set; }
        
        // Core & Content
        [Option('L', "libretro", Required = false, Min  = 1, Max = 3,
            HelpText = "Path to libretro implementation. Overrides any config setting.")]
        public IEnumerable<string>? G4_Libretro { get; set; }
        
        // Patch
        [Option("no-patch", Default = false,
            HelpText = "Disables all forms of content patching.")]
        public bool G5_NoPatch { get; set; }

        [Option('U', "ups",
            HelpText = "Specifies path for UPS patch that will be applied to content.")]
        public string? G5_Ups { get; set; }
        
        [Option("bps", 
            HelpText = "Specifies path for BPS patch that will be applied to content.")]
        public string? G5_Bps { get; set; }
        
        [Option("ips", 
            HelpText = "Specifies path for IPS patch that will be applied to content.")]
        public string? G5_Ips { get; set; }
        
        [Option("xdelta", 
            HelpText = "Specifies path for Xdelta patch that will be applied to content.")]
        public string? G5_Xdelta { get; set; }
        
        // SubSystem
        [Option("subsystem", Separator = sepWhiteSpace,
            HelpText = "Use a subsystem of the libretro core.")]
        public IEnumerable<string>? G6_SubSystem { get; set; }
        
        
        // Methods
        public static RetroArch? GetGroup1(IUserLinkParameters? link)
        {
            if (link is null) return null;
            return new RetroArch() {
                G1_Accessibility = link.RaAccessibility,
                G1_Fullscreen = link.RaFullscreen,
                G1_MenuOnError = link.RaMenuOnError,
                G1_Verbose = link.RaVerbose
            };
        }

        public static RetroArch? GetGroup2(IUserLinkParameters? link) {
            if (link is not { RaLogFile: not null}) return null;
            return new RetroArch() { G2_LogFile = link.RaLogFile };
        }

        public static RetroArch? GetGroup3(IUserLinkParameters? link)
        {
            if (link is null) return null;
            return new RetroArch() {
                G3_Config = link.RaConfig,
                G3_AppendConfig = link.RaAppendedConfig?.ToArray()
            };
        }

        public static RetroArch? GetGroup4(IUserLinkParameters? link)
        {
            if (link is null) return null;
            List<string> coreAndRom = [link.RaCore];
            if (!string.IsNullOrEmpty(link.RaRom) && link.RaRom != Contentless) coreAndRom.Add(link.RaRom);  // TODO: Test
            
            return new RetroArch() {
                G4_Libretro = coreAndRom.ToArray()
            };
        }

        public static RetroArch? GetGroup5(IUserLinkParameters? link)
        {
            if (link is not { RaRomPatch: not null }) return null;
            var patchArgs = link.RaRomPatch;
            var retroArch = new RetroArch();
            switch (patchArgs.PatchType)
            {
                case RomPatchType.No_Patch:
                    retroArch.G5_NoPatch = true;
                    break;
                case RomPatchType.UPS:
                    retroArch.G5_Ups = patchArgs.PatchArg;
                    break;
                case RomPatchType.BPS:
                    retroArch.G5_Bps = patchArgs.PatchArg;
                    break;
                case RomPatchType.IPS:
                    retroArch.G5_Ips = patchArgs.PatchArg;
                    break;
                case RomPatchType.XDelta:
                    retroArch.G5_Xdelta = patchArgs.PatchArg;
                    break;
                default:            // <= Shouldn't happen, but just in case
                    return null;
            }
            
            return retroArch;
        }

        public static RetroArch? GetGroup6(IUserLinkParameters? link)
        {
            if (link is not { RaCoreSubsystem: not null}) return null;
            List<string> subsystemAndArgs = new();
            subsystemAndArgs.Add(link.RaCoreSubsystem.Subsystem);
            subsystemAndArgs.AddRange(link.RaCoreSubsystem.Args);
            return new RetroArch() { G6_SubSystem = subsystemAndArgs.ToArray() };
        }
    }
}