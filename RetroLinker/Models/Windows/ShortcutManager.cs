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
using System.Threading.Tasks;
#if WINDOWS
using Microsoft.ClearScript.Windows.Core;
#endif

namespace RetroLinker.Models.Windows;

public static class ShortcutManager
{
    private const string objShell = "shell";
    private const string objLink = "link";
    private const string objArray = "valueArray";
    private const string createLink = "CreateLink";
    private const string readLink = "ReadLink";
    private static readonly string commentLine = $"' {App.LocalInformation.Name} v{App.LocalInformation.Version}";
    private static readonly string scriptTitle = $"{App.LocalInformation.Name} Script Runner";
    
    public static async Task CreateShortcut(LinkParameters link)
    {
        var scriptStrings = $"""
                             {commentLine}
                             Function {createLink}()
                               Set {objShell} = CreateObject("WScript.Shell")
                               Set {objLink} = {objShell}.CreateShortcut("{link.OutputPath}")
                               {objLink}.TargetPath = "{link.RaExecutable}"
                               {objLink}.WorkingDirectory = "{link.RaWorkDir}"
                               {objLink}.Arguments = "{link.RaArguments}"
                               {objLink}.Description = "{link.Description}"
                               {objLink}.IconLocation = "{link.IconPath}"
                               {objLink}.Save
                               {createLink} = 0
                             End Function
                             """;
        
        await Task.Run(() => RunLinkWriteScript(scriptStrings));
        Logger.LogInfo($"\"{link.OutputPath}\" file created successfully.");
    }

    // Return a Shortcutter type
    public static async Task<string?[]> ReadShortcut(string linkPath)
    {
        // Why does creating an Array(4) is VBS results in an array with 5 positions?
        var scriptStrings = $"""
                            {commentLine}
                            Function {readLink}()
                                Dim {objArray}(4)
                                Set {objShell} = CreateObject("WScript.Shell")
                                Set {objLink} = {objShell}.CreateShortcut("{linkPath}")
                                {objArray}(0) = {objLink}.TargetPath
                                {objArray}(1) = {objLink}.WorkingDirectory
                                {objArray}(2) = {objLink}.Arguments
                                {objArray}(3) = {objLink}.Description
                                {objArray}(4) = {objLink}.IconLocation
                                {readLink} = {objArray}
                            End Function
                            """;
        
        var linkContent = await Task.Run(() => RunLinkReadScript(scriptStrings));
        Logger.LogInfo($"\"{linkPath}\" file read successfully.");
        var linkStrings = new string?[linkContent.Length];
        for (int i = 0; i < linkContent.Length; i++)
            linkStrings[i] = linkContent[i].ToString();
        return linkStrings;
    }
    
#if WINDOWS
    private static VBScriptEngine RunScriptEngine(string script)
    {
        var engine = new VBScriptEngine($"{scriptTitle}", NullSyncInvoker.Instance);
        engine.Execute(script);
        return engine;
    }

    
    private static void RunLinkWriteScript(string script)
    {
        var engine = RunScriptEngine(script);
        // VBS returns Int16 (short) instead of Int32 (int)
        var result = (short)engine.Invoke($"{createLink}");
        
        if (result == 0) return;
        var err = $"The LinkWrite script was not executed properly! Error code {result}.";
        Logger.LogErro(err);
        throw new ApplicationException(err);
    }
    
    private static object[] RunLinkReadScript(string script)
    {
        var engine = RunScriptEngine(script);
        var values = (object[])engine.Invoke($"{readLink}");
        return values;
    }
#else
    private static void RunLinkWriteScript(string _) => throw new PlatformNotSupportedException();
    private static object[] RunLinkReadScript(string _) => throw new PlatformNotSupportedException();
#endif

    public static async Task CreateTileIcoShortcut(LinkParameters link)
    {
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
            new(TileIcoOptions.all_users, link.TileIcoAllUsers)
        ]);
        if (!string.IsNullOrEmpty(link.TileIcoImagePath)) 
            tileIcoArguments.Add(new(TileIcoOptions.image, link.TileIcoImagePath));
        if (tileIcoNameOpt is { } opt) 
            tileIcoArguments.Add(new TileicoArgument(opt, true));
        
        await ExecuteTileIco(tileIcoArguments);
    }

#if WINDOWS
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
        
        if (proc.ExitCode == 0) return;
        
        var errors = await proc.StandardError.ReadToEndAsync();
        throw new ApplicationException(errors);
    }
#else
    private static async Task ExecuteTileIco(TileicoArgumentList _) => throw new PlatformNotSupportedException();
#endif

    
}

internal enum TileIcoCommands { create, delete }
internal enum TileIcoSubCommands { custom, custom_test }

internal enum TileIcoOptions
{
    name, 
    target, 
    arguments,
    all_users,
    icon,
    image,
    name_on_tile_light,
    name_on_tile_dark
}

internal struct TileicoArgument 
{
    public readonly string Option;
    public readonly string Value;

    private TileicoArgument(string option,  string value) {
        Option = option;
        Value = value;
    }

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