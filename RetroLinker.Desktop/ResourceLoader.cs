/*
    A .NET GUI application to help create desktop links of games running on RetroArch.
    Copyright (C) 2025  Kevin Rafael Martinez Johnston

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

using System;
using System.IO;
using System.Reflection;

namespace RetroLinker.Desktop;

public static class ResourceLoader
{
    // Manual resource navigation thanks to Denis Beurive and Arekadiusz on stackoverflow
    //https://stackoverflow.com/questions/70707947/how-to-set-a-file-as-an-embedded-resource-in-rider-intellij
    public static string[] test1(Assembly assembly) => assembly.GetManifestResourceNames();

    public static string[] GetTextFromResource(Assembly assembly, string resourceName)
    {
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) throw new Exception($"Resource not found: {resourceName}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd().Split('\n');
    }
}