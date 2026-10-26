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

using System.Runtime.InteropServices;

namespace RetroLinker.Models.Linux;

internal static partial class NativeMethods {
    [UnsupportedOSPlatform(App.PlatformWin)]
    [LibraryImport("libc")]
    public static partial uint geteuid();
}

public static class NativeAccess {
    private const uint RootUID = 0;
    [UnsupportedOSPlatform(App.PlatformWin)]
    public static bool IsUnixProcessElevated() => NativeMethods.geteuid() == RootUID;
}
