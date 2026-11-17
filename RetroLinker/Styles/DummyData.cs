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
using RetroLinker.Models.IO;

namespace RetroLinker.Styles;

public static class DummyData
{
    private const string DummyFileName = "dummy.bin";

    public static ObservableCollection<string> DummyStrings { get; } = new()
    {
        "Lorem",
        "ipsum",
        "dolor",
        "amet",
        "consectetur",
        "adipiscing"
    };

    public static ObservableCollection<EnvironmentVariable> DummyEnvVars { get; } = new()
    {
        new EnvironmentVariable("Key_A", "Value_A"),
        new EnvironmentVariable("Key_B", "Value_B"),
        new EnvironmentVariable("LongKey_A", "LongValue_A"),
        new EnvironmentVariable("LongKey_B", "LongValue_B"),
        new EnvironmentVariable("ReallyLongKey_A", "ReallyLongValue_A"),
        new EnvironmentVariable("ReallyLongKey_B", "ReallyLongValue_B"),
    };

    public static ObservableCollection<string> DummyPathsList { get; } = FillPathsList();

    public static ObservableCollection<string> FillPathsList(string fileNameTemplete = DummyFileName)
    {
        var wrkTemplate = string.Empty;
        string fileName;
        string fileExt;
        do
        {
            if (fileNameTemplete == wrkTemplate) fileNameTemplete = DummyFileName;
            fileName = FileOps.GetFileNameNoExtFromPath(fileNameTemplete);
            fileExt = FileOps.GetFileExtFromPath(fileNameTemplete);
            wrkTemplate = fileNameTemplete;
        } while (string.IsNullOrEmpty(fileExt));

        var arguments = new ObservableCollection<string>();
        var pfx = FileOps.CombineMultipleInputs(FileOps.UserDesktop, "testing");
        for (int i = 0; i < 16; i++) {
            var testPath = FileOps.CombineMultipleInputs(pfx, $"{fileName}{i}{fileExt}");
            arguments.Add(testPath);
        }
        return arguments;
    }
}