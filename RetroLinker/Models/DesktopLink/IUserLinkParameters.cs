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
using RetroLinker.Models.IO;

namespace RetroLinker.Models.DesktopLink;

public interface IUserLinkParameters
{
    public string RaRom { get; }
    public string RaRomName => GetRomName(this);
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
    public IReadOnlyList<EnvironmentVariable> EnvironmentVariables { get; }

    protected static string GetRomName(IUserLinkParameters u) => FileOps.GetFileNameNoExtFromPath(u.RaRom);
}

public interface IUserLinkBuilder : IUserLinkParameters
{
    // IUserLinkParameters but with setters
    public new string RaRom { get; set; }
    public new RomPatchArgs? RaRomPatch { get; set; }
    public new string RaCore { get; set; }
    public new SubsystemArgs? RaCoreSubsystem { get; set; }
    public new string? RaConfig { get; set; }
    public new IReadOnlyAppendedConfigs? RaAppendedConfig { get; set; }
    public new bool RaFullscreen { get; set; }
    public new bool RaMenuOnError { get; set; }
    public new bool RaAccessibility { get; set; }
    public new bool RaVerbose { get; set; }
    public new string? RaLogFile { get; set; }
    
    // IUserLinkBuilder
    public bool? CustomEntryName { get; set; }
    
    public bool AddAppendedConfig(string appendedConfigPath);
    public bool RemoveAppendedConfig(string appendedConfigPath);
    public bool ClearAppendedConfig();
    
    public bool AddEnvironmentVariable(EnvironmentVariable envVar);
    public bool RemoveEnvironmentVariable(EnvironmentVariable envVar);
    public bool ClearEnvironmentVariables();
}