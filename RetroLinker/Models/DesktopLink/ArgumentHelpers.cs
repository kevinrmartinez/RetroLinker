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
using System.Collections.ObjectModel;
using RetroLinker.Models.CommandLine;

namespace RetroLinker.Models.DesktopLink;

public interface IExtraArguments
{
    public T? GetArgument<T>()  where T : IExtraArguments {
        if (this is T specificArgument) 
            return specificArgument;
        return default;
    }
}

public interface IReadOnlyAppendedConfigs : IReadOnlyList<string>, IExtraArguments;

public class RomPatchArgs(RomPatchType patchType, string PatchPath) : IExtraArguments
{
    public RomPatchType PatchType { get; } = patchType;
    public string PatchArg { get; } = PatchPath;

    public override string ToString()
    {
        return PatchType switch
        {
            RomPatchType.ExNoPatch => PatchType.ToString("G").ToLower(),
            RomPatchType.NoPatch => string.Empty,
            _ => $"{PatchType}={PatchArg}"
        };
    }
}

public class SubsystemArgs(string subsystem, IReadOnlyList<string> args) : IExtraArguments
{
    public string Subsystem { get; } = subsystem;
    public IReadOnlyList<string> Args { get; }  = args;

    public override string ToString() => $"{Subsystem} {string.Join(" ", Args)}";
}

public class ObservableAppendedConfigs : ObservableCollection<string>, IReadOnlyAppendedConfigs
{
    public override string ToString() => string.Join("|", this);
}

public record EnvironmentVariable(string Key, string Value) {
    public override string ToString() =>  $"{Key}={Value}"; // Compatible with Windows and POSIX
    public override int GetHashCode() => Key.GetHashCode();
}