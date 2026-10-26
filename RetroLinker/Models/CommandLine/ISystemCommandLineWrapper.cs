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

using RetroLinker.Models.DesktopLink;

namespace RetroLinker.Models.CommandLine;

/// <summary>
/// Provides the base interface for the abstraction of OS native script engines wrapping our executable of interest.
/// </summary>
public interface ISystemCommandLineWrapper
{
    public string RealTargetExecutable { get; }
    public string RealTargetArguments { get; }
    public HashSet<EnvironmentVariable> EnvironmentVariables { get; }
}