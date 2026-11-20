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

namespace RetroLinker;

internal enum RetroLinkerLogType
{
    Information,
    Warning,
    Error,
    Critical,
    Debug,
    Trace
}

internal enum RetroLinkerLogLevel
{
    /// <summary> Allows all <see cref="T:RetroLinkerLogType"/> logs. </summary>
    All,
    /// <summary> Allows the <see cref="E:RetroLinkerLogType.Information"/> and <see cref="E:RetroLinkerLogType.Warning"/> logs. </summary>
    Information,
    /// <summary> Allows the <see cref="E:RetroLinkerLogType.Error"/> and <see cref="E:RetroLinkerLogType.Critical"/> logs. </summary>
    Error,
    /// <summary> Allows the <see cref="E:RetroLinkerLogType.Debug"/> and <see cref="E:RetroLinkerLogType.Trace"/> logs. </summary>
    Debug
}

public enum RomPatchType {
    None,
    UPS, BPS, IPS,
    XDelta, No_Patch,
    Any
}

public enum ArgFileType { Invalid, Rom, Link }