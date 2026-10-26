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
using RetroLinker.Models.Settings;

namespace RetroLinker.Models.Windows;

[SupportedOSPlatform(App.PlatformWin)]
public class WindowsLinkParameters : LinkParameters, IWindowsLinkParameters
{
    public string? RaWorkDir { get; }
    public bool TileIcoNameVisible { get; }
    public bool TileIcoNameDark { get; }
    public bool TileIcoAllUsers { get; }
    public bool TileIcoOverwrite { get; }
    public string? TileIcoImagePath { get; }
    private WindowsBuildOptions BuildOption { get; set; }
    
    private string _iconFallabckPath;
    private enum WindowsBuildOptions { Shell, TileIco }
    
    public WindowsLinkParameters(IShortcutBase linkWriter)
    {
        EnvironmentVariables = linkWriter.EnvironmentVariables;
        UpdateHashedVariables();
        
        if (HashedEnvironmentVariables.Count < 1) {
            RaExecutable = linkWriter.RaExecutable;
            RaArguments = linkWriter.RaArguments;
        }
        else {
            RaExecutable = CommandLine.Manager.GetPowershellExecutable();
            RaArguments = CommandLine.Manager.CreatePowershellArguments(linkWriter, HashedEnvironmentVariables);
        }
        
        RaWorkDir = linkWriter.RaWorkDir ?? FileOps.GetDirFromPath(linkWriter.RaExecutable); // <= By default
        Description = linkWriter.Description;
        IconPath = linkWriter.IconPath;
        FriendlyName = linkWriter.FriendlyName;
        OutputFileName = linkWriter.OutputFileName;
        OutputPath = linkWriter.OutputPath;
        _ogOutputPath = linkWriter.OutputPath;
        TileIcoNameVisible = linkWriter.TileIcoNameVisible;
        TileIcoNameDark = linkWriter.TileIcoNameDark;
        TileIcoAllUsers = linkWriter.TileIcoAllUsers;
        TileIcoOverwrite = linkWriter.TileIcoOverwrite;
        TileIcoImagePath = linkWriter.TileIcoImagePath;
        BuildOption = (SettingsOps.GetCachedSettings().TileIcoPath is null) 
            ? WindowsBuildOptions.Shell : WindowsBuildOptions.TileIco;
        
        _iconFallabckPath = linkWriter.RaExecutable;
    }

    public WindowsLinkParameters(SharpShellLink.Shortcut shortcut, string linkPath)
    {
        System.ArgumentException.ThrowIfNullOrEmpty(shortcut.StringData?.CommandLineArguments);
        string targetPath;
        if (shortcut.LinkTargetIDList is not null) targetPath = shortcut.LinkTargetIDList.Path;
        else if (shortcut.ExtraData.EnvironmentVariableDataBlock is not null)
            targetPath = shortcut.ExtraData.EnvironmentVariableDataBlock.TargetUnicode;
        else throw new System.ArgumentException("Can't get target program from shortcut");

        var rawArguments = shortcut.StringData.CommandLineArguments;
        if (!rawArguments.Contains($"-{CommandLine.Manager.PwsCommandOpt}")) {
            RaExecutable = targetPath;
            RaArguments = rawArguments;
        }
        else
        {
            var pwsWrapper = CommandLine.Manager.ResolvePowershellArguments(rawArguments);
            RaExecutable = pwsWrapper.RealTargetExecutable;
            RaArguments = pwsWrapper.RealTargetArguments.UndoTwoDoubleQuotes();
            HashedEnvironmentVariables = pwsWrapper.EnvironmentVariables;
            EnvironmentVariables = new List<EnvironmentVariable>(HashedEnvironmentVariables);
        }
        
        Description = shortcut.StringData.NameString;
        IconPath = shortcut.StringData.IconLocation;
        OutputFileName = shortcut.LinkTargetIDList?.DisplayName ?? FileOps.GetFileNameFromPath(linkPath);
        FriendlyName = FileOps.GetFileNameNoExtFromPath(OutputFileName);
        OutputPath = linkPath;
        _ogOutputPath = linkPath;
        TileIcoImagePath = null; // <= It can be done, but this app can't handle all the things a Tile Icon has
        
        _iconFallabckPath = RaExecutable;
    }

    public override void SetToCopyOutputPath(string outputPath) {
        // Only the first output uses 'tileico'
        BuildOption = WindowsBuildOptions.Shell;
        base.SetToCopyOutputPath(outputPath);
    }

    public override async Task<LinkWriterResult> CreateOsShortcut() {
        return BuildOption switch {
            WindowsBuildOptions.Shell => await BuildWinShortcut_WindowsShell(),
            WindowsBuildOptions.TileIco => await BuildWinShortcut_TileIco(),
            _ => throw new System.ArgumentOutOfRangeException(nameof(BuildOption), BuildOption, null)
        };
    }

    private async Task<LinkWriterResult> BuildWinShortcut_WindowsShell() {
        if (string.IsNullOrEmpty(IconPath)) IconPath = _iconFallabckPath;
        return await CreateShortcut(ShortcutManager.CreateShortcut);
    }

    private async Task<LinkWriterResult> BuildWinShortcut_TileIco() 
        => await CreateShortcut(ShortcutManager.CreateTileIcoShortcut);
}