/*
    RetroLinker: A .NET GUI application to help create desktop links of games running on RetroArch.
    Copyright (C) 2024  Kevin Rafael Martinez Johnston

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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
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
        
        await FileOps.WriteAllBytesToFileAsync(link.OutputPath, outputFile.GetBytes());
        Logger.LogInfo($"\"{link.OutputPath}\" file created successfully.");
    }
    
    public static async Task<LinkParameters> ReadShortcut(string linkPath)
    {
        var inputFile = Shortcut.ReadFromFile(linkPath);
        return new WindowsLinkParameters(inputFile, linkPath);
    }
    
    public static async Task CreateTileIcoShortcut(LinkParameters linkBase)
    {
        var link = (WindowsLinkParameters)linkBase;
        var tileIcoArguments = new TileicoArgumentList();
        TileIcoOptions? tileIcoNameOpt = null;
        if (link.TileIcoNameVisible) {
            tileIcoNameOpt = link.TileIcoNameDark ? TileIcoOptions.name_on_tile_dark : TileIcoOptions.name_on_tile_light;
        }
        
        tileIcoArguments.Add(new(TileIcoCommands.create, TileIcoSubCommands.custom));
        tileIcoArguments.AddRange([
            new(TileIcoOptions.name, link.FriendlyName),
            new(TileIcoOptions.target, link.RaExecutable),
            new(TileIcoOptions.arguments, link.RaArguments),
            new(TileIcoOptions.icon, link.IconPath),
            new(TileIcoOptions.all_users, link.TileIcoAllUsers),
            new(TileIcoOptions.overwrite, link.TileIcoOverwrite)
            // TODO: Update tileico to reflect this changes!!
        ]);
        if (!string.IsNullOrEmpty(link.TileIcoImagePath)) 
            tileIcoArguments.Add(new(TileIcoOptions.image, link.TileIcoImagePath));
        if (tileIcoNameOpt is { } opt) 
            tileIcoArguments.Add(new TileicoArgument(opt, true));
        
        await ExecuteTileIco(tileIcoArguments);
    }
    
    private static async Task ExecuteTileIco(TileicoArgumentList args)
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

internal readonly struct TileicoArgument 
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

    public override string ToString() => $"{Option} {Value}";
    
    public bool SameArgument(TileicoArgument other) => (Option == other.Option);
}

internal class TileicoArgumentList : List<TileicoArgument>
{
    /// <summary>Adds an object to the end of the <see cref="List{TileicoArgument}" />.</summary>
    /// <param name="item">The <see cref="TileicoArgument"/> to be added to the end of the <see cref="List{TileicoArgument}" />. If an element with the same '<see cref="TileicoArgument.Option"/>' member already exist, then <paramref name="item"/> won't be added.</param>
    public new void Add(TileicoArgument item) {
        if (!this.Any(i => i.SameArgument(item))) base.Add(item);
    }

    /// <summary>Adds the elements of the specified collection to the end of the <see cref="List{TileicoArgument}" />.</summary>
    /// <param name="collection">The collection whose elements should be added to the end of the <see cref="List{TileicoArgument}" />.</param>
    /// <remarks>The object to add is conditioned, see <see cref="Add"/>.</remarks>
    public new void AddRange(IEnumerable<TileicoArgument> collection) {
        // I believe this is expensive in the long run, but it is called with very few (~10 at most) elements in practice
        foreach (var item in collection) Add(item);
    }
}