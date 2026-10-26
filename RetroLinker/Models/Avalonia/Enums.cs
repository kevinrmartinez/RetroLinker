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

namespace RetroLinker.Models.Avalonia;

public enum OpenOpts { Other, WinExe, RAroms, RAcfg, WINico, LinBin, LINico }

public enum OpenFolderOpts { Other, UserAssets, ROMParent, IconParent, IcoOutput, LinkCopy, DefOutput }

public enum SaveOpts { WINlnk, LINdesktop }

public enum PatchOpts { Auto, UPS, BPS, IPS, XD }

public enum GenericPopUpType { Error, Warning, Question, Info, Success }