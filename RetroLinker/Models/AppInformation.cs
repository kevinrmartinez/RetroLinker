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

namespace RetroLinker.Models;

public readonly struct AppInformation(
    string fullName,
    string name,
    string version,
    DateTime? buildDate = null,
    string? gitHash = null)
{
    public string FullName { get; } = fullName;
    public string Name { get; } = name;
    public string Version { get; } = version;
    public DateTime? BuildDate { get; } = buildDate;
    public string? GitHash { get; } = gitHash;


    public override string ToString() => $"{Name} v{Version}";

    public string ToStringLines()
    {
        const string noData = "No Data";
        var finalDate = BuildDate?.ToString("O") ?? noData;
        var finalGitHash = GitHash ?? noData;
        return $"""
                FullName  {FullName}
                Name      {Name}
                Version   {Version}
                BuildDate {finalDate}
                Git Hash  {finalGitHash}
               """;
    }
}