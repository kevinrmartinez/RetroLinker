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

using System.Diagnostics;
using RetroLinker.Models.DesktopLink;
using RetroLinker.Models.IO;
using RetroLinker.Models.Settings;
using SharpShellLink;

namespace RetroLinker.Models.Windows;

[SupportedOSPlatform(App.PlatformWin)]
public static class ShortcutManager
{
    public static async Task CreateShortcut(LinkParameters linkBase)
    {
        var link = (WindowsLinkParameters)linkBase;
        const int iconIndex = 0;    // This may be changeable in the future
        var outputFile = Shortcut.CreateShortcut(link.RaExecutable, link.RaArguments, link.IconPath, iconIndex);
        outputFile.StringData?.WorkingDir = link.RaWorkDir ?? string.Empty;
        outputFile.StringData?.NameString = link.Description ?? string.Empty;
        if (link.RaExecutable.Contains("powershell", StringComparison.OrdinalIgnoreCase) &&
            !link.RaArguments.Contains("-v"))   // I think is kind of useless
            outputFile.ShowCommand = SharpShellLink.Flags.ShowCommand.SW_SHOWMINNOACTIVE;
        
        await FileOps.WriteAllBytesToFileAsync(link.OutputPath, outputFile.GetBytes());
        Logger.LogInfo($"\"{link.OutputPath}\" file created successfully.");
    }
    
    public static async Task<LinkParameters> ReadShortcut(string linkPath)
    {
        var inputFile = await Task.Run(() => Shortcut.ReadFromFile(linkPath));
        return new WindowsLinkParameters(inputFile, linkPath);
    }
    
    public static async Task CreateTileIcoShortcut(LinkParameters linkBase)
    {
        var link = (WindowsLinkParameters)linkBase;
        var tileIcoArguments = new HashSet<TileicoArgument>();
        TileIcoOptions? tileIcoNameOpt = null;
        if (link.TileIcoNameVisible) {
            tileIcoNameOpt = (link.TileIcoNameDark) ? TileIcoOptions.name_on_tile_dark : TileIcoOptions.name_on_tile_light;
        }
        
        // Tileico commands
        tileIcoArguments.Add(new(TileIcoCommands.create, TileIcoSubCommands.custom));
        // Tileico arguments
        tileIcoArguments.Add(new(TileIcoOptions.name, link.FriendlyName));
        tileIcoArguments.Add(new(TileIcoOptions.target, link.RaExecutable));
        tileIcoArguments.Add(new(TileIcoOptions.arguments, link.RaArguments));
        tileIcoArguments.Add(new(TileIcoOptions.icon, link.IconPath));
        tileIcoArguments.Add(new(TileIcoOptions.all_users, link.TileIcoAllUsers));
        tileIcoArguments.Add(new(TileIcoOptions.overwrite, link.TileIcoOverwrite));
        
        if (!string.IsNullOrEmpty(link.TileIcoImagePath)) 
            tileIcoArguments.Add(new(TileIcoOptions.image, link.TileIcoImagePath));
        if (tileIcoNameOpt is { } opt) 
            tileIcoArguments.Add(new TileicoArgument(opt, true));
        
        // Execute Tileico
        await ExecuteTileIco(tileIcoArguments);
    }
    
    private static async Task ExecuteTileIco(HashSet<TileicoArgument> args)
    {
        var tileIcoPath = SettingsOps.GetCachedSettings().TileIcoPath;
        ArgumentException.ThrowIfNullOrEmpty(tileIcoPath);
        var psi = new ProcessStartInfo(tileIcoPath)
        {
            UseShellExecute = false,
            RedirectStandardError = true,
#if DEBUG
            CreateNoWindow = false,
#else
            CreateNoWindow = true,
#endif
        };
        foreach (var argument in args) {
            psi.ArgumentList.Add(argument.Option);
            psi.ArgumentList.Add(argument.Value);
        }
        using var proc = Process.Start(psi);
        
        if (proc is null) throw new ApplicationException($"'{psi.FileName}' failed to start");
        // var errorsTask = proc.StandardError.ReadToEndAsync();
        await proc.WaitForExitAsync();

        const int allGood = 0;
        if (proc.ExitCode == allGood) return;
        
        var errors = await proc.StandardError.ReadToEndAsync();
        throw new ApplicationException(errors);
    }

    
}

internal enum TileIcoCommands { create, delete }
internal enum TileIcoSubCommands { custom, custom_test }

internal enum TileIcoOptions
{
    name, 
    target, 
    arguments,
    icon,
    image,
    all_users,
    overwrite,
    name_on_tile_light,
    name_on_tile_dark
}

internal readonly struct TileicoArgument : IEquatable<TileicoArgument>
{
    public readonly string Option;
    public readonly string Value;

    public TileicoArgument(TileIcoCommands command, TileIcoSubCommands subCommand) {
        Option = EnumToString(command);
        Value = EnumToString(subCommand);
    }
    
    public TileicoArgument(TileIcoOptions option, object value)
    {
        var valueToString = string.Empty;
        switch (option)
        {
            case TileIcoOptions.all_users:
            case TileIcoOptions.overwrite:
            case TileIcoOptions.name_on_tile_light:
            case TileIcoOptions.name_on_tile_dark:
                if (value is bool boolValue) valueToString = boolValue.ToString().ToLower();
                break;
            default:
                if (value is string stringValue) valueToString = stringValue;
                break;
        }
        ArgumentException.ThrowIfNullOrEmpty(valueToString, nameof(value)); // Reconsider
        
        Option = $"--{EnumToString(option)}";
        Value = valueToString;
    }

    private static string EnumToString(Enum value) => value.ToString("G").Replace('_', '-');

    public bool Equals(TileicoArgument other) {
        return Option == other.Option;
    }
    public override int GetHashCode() => Option.GetHashCode();
    public override string ToString() => $"{Option} {Value}";
}