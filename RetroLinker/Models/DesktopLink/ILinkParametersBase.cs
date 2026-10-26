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

using RetroLinker.Models.IO;

namespace RetroLinker.Models.DesktopLink;

public interface ILinkParametersBase
{
    /// <summary>
    /// RetroArch's executable, should be a path to a file, or a command
    /// </summary>
    public string RaExecutable { get; }
    public string RaArguments { get; }
    public string? Description { get; }
    public string IconPath { get; }
    public string FriendlyName { get; }
    public string OutputFileName { get; }
    public string OutputPath { get; }
    public string OutputDir => GetOutputDir(this);
    public IReadOnlyList<EnvironmentVariable> EnvironmentVariables { get; }

    protected static string GetOutputDir(ILinkParametersBase l) => FileOps.GetDirFromPath(l.OutputPath) ?? string.Empty;
}

public interface IWindowsLinkParameters : ILinkParametersBase
{
    public string? RaWorkDir { get; }
    public bool TileIcoNameVisible { get; }
    public bool TileIcoNameDark { get; }
    public bool TileIcoAllUsers { get; }
    public bool TileIcoOverwrite { get; }
    public string? TileIcoImagePath { get; }
}

public interface ILinuxLinkParameters : ILinkParametersBase
{
    public bool RaVerbose { get; }
}

public interface IShortcutBase : IWindowsLinkParameters, ILinuxLinkParameters;