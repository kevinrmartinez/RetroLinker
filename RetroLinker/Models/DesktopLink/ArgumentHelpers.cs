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

public class RomPatchArgs(RomPatchType patchType, string patchPath) : IExtraArguments
{
    public RomPatchType PatchType { get; } = patchType;
    public string PatchArg { get; } = patchPath;

    public override string ToString()
    {
        return PatchType switch
        {
            RomPatchType.No_Patch => PatchType.ToString("G").ToLower().Replace("_", "-"),
            RomPatchType.None => string.Empty,
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

public class AppendedConfigs : List<string>, IReadOnlyAppendedConfigs
{
    public AppendedConfigs() : base() {}
    public AppendedConfigs(IEnumerable<string> collection) : base(collection) {}
    public AppendedConfigs(int capacity) : base(capacity) {}
    
    public override string ToString() => string.Join("|", this);
}

public class ObservableAppendedConfigs : ObservableCollection<string>, IReadOnlyAppendedConfigs
{
    public ObservableAppendedConfigs() : base() {}
    public ObservableAppendedConfigs(IEnumerable<string> collection) : base(collection) {}
    public ObservableAppendedConfigs(List<string> list) : base(list) {}
    
    public override string ToString() => string.Join("|", this);
}

public record EnvironmentVariable(string Key, string Value) {
    public override string ToString() =>  $"{Key}{EnvSeparator}{Value}"; // Compatible with Windows and POSIX
    public string ToStringPws() =>  $"{Manager.PwsEnvSetter}{Key}{EnvSeparator}{Utils.ConditionallyQuoteString(Value, os: true, doubleQuotes: false)}";
    public string ToStringBash() =>  $"{Key}{EnvSeparator}{Utils.ConditionallyQuoteString(Value, os: false, doubleQuotes: false)}";
    public override int GetHashCode() => Key.GetHashCode();

    private const char EnvSeparator = '=';
    private const int KeyIndex = 0;
    private const int ValueIndex = 1;
    
    public static EnvironmentVariable? FromPwsArgument(string pwsArgument)
    {
        pwsArgument = pwsArgument.Replace(Manager.PwsEnvSetter, "").Trim();
        var env = pwsArgument.Split(EnvSeparator);
        if (env.Length != 2)
        {
            // error or warning?
            return null;
        }

        env[ValueIndex] = env[ValueIndex].UnwrapStringBetweenQuotes(doubleQuotes: false);
        return new EnvironmentVariable(env[KeyIndex], env[ValueIndex]);
    }

    public static EnvironmentVariable? FromBashArgument(string bashArgument)
    {
        var env = bashArgument.Trim().Split(EnvSeparator);
        if (env.Length != 2)
        {
            // error or warning?
            return null;
        }

        env[ValueIndex] = env[ValueIndex].UnwrapStringBetweenQuotes(doubleQuotes: false);
        return new EnvironmentVariable(env[KeyIndex], env[ValueIndex]);
    }
}