/*
    RetroLinker: A .NET GUI application to help create desktop links of games running on RetroArch.
    Copyright (C) 2023-2026 Kevin Rafael Martinez Johnston

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

using System.Linq;
using RetroLinker.Models.DesktopLink;

namespace RetroLinker.Models.CommandLine;

public static partial class Manager
{
    private class ParsedRetroArch : IUserLinkParameters
    {
        public string RaRom { get; } = string.Empty;
        public RomPatchArgs? RaRomPatch { get; }
        public string RaCore { get; }
        public SubsystemArgs? RaCoreSubsystem { get; }
        public string? RaConfig { get; }
        public IReadOnlyAppendedConfigs? RaAppendedConfig { get; }
        public bool RaFullscreen { get; }
        public bool RaMenuOnError { get; }
        public bool RaAccessibility { get; }
        public bool RaVerbose { get; }
        public string? RaLogFile { get; }
        
        public IReadOnlyList<EnvironmentVariable> EnvironmentVariables => [];   // Prop not used

        
        public ParsedRetroArch(RetroArch retroArch)
        {
            var libretro = retroArch.G4_Libretro.ToList();
            RaCore = libretro[0];
            if (libretro.Count > 1) RaRom = libretro[1];    // <= by default
            RaConfig = retroArch.G3_Config;
            RaFullscreen = retroArch.G1_Fullscreen;
            RaMenuOnError = retroArch.G1_MenuOnError;
            RaAccessibility = retroArch.G1_Accessibility;
            RaVerbose = retroArch.G1_Verbose;
            RaLogFile = retroArch.G2_LogFile;
            
            if (retroArch.G5_NoPatch) RaRomPatch = new RomPatchArgs(RomPatchType.No_Patch, string.Empty);
            else
            {
                if (retroArch.G5_Bps is not null) RaRomPatch = new RomPatchArgs(RomPatchType.BPS, retroArch.G5_Bps);
                if (retroArch.G5_Ips is not null) RaRomPatch = new RomPatchArgs(RomPatchType.IPS, retroArch.G5_Ips);
                if (retroArch.G5_Ups is not null) RaRomPatch = new RomPatchArgs(RomPatchType.UPS, retroArch.G5_Ups);
                if (retroArch.G5_Xdelta is not null) RaRomPatch = new RomPatchArgs(RomPatchType.XDelta, retroArch.G5_Xdelta);
            }

            if (retroArch.G6_SubSystem is not null && retroArch.G6_SubSystem.Any()) {
                var subSystemList = retroArch.G6_SubSystem.ToList();
                RaCoreSubsystem = new SubsystemArgs(subSystemList[0], subSystemList[1..]);
            }

            if (retroArch.G3_AppendConfig is not null && retroArch.G3_AppendConfig.Any()) {
                RaAppendedConfig = new AppendedConfigs(retroArch.G3_AppendConfig);
            }
        }
    }
}