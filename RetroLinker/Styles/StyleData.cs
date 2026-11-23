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

using Avalonia.Media;

namespace RetroLinker.Styles;

/// <summary> Wrapper for commonly used colors/brushes </summary>
internal static class StyleData
{
    public static readonly IImmutableSolidColorBrush BrushBlueLight1 = Brushes.Aqua;
    public static readonly IImmutableSolidColorBrush BrushBlueLight2 = Brushes.LightSkyBlue;
    public static readonly IImmutableSolidColorBrush BrushGreen1 = Brushes.ForestGreen;
    public static readonly IImmutableSolidColorBrush BrushGreen2 = Brushes.LimeGreen;
    public static readonly IImmutableSolidColorBrush BrushRed1 = Brushes.Firebrick;
    public static readonly IImmutableSolidColorBrush BrushRed2 = Brushes.Crimson;
}