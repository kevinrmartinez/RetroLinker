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

using System.Runtime.Versioning;
using System.Threading.Tasks;
using RetroLinker.Models.DesktopLink;
using RetroLinker.Models.IO;
using RetroLinker.Models.Settings;

namespace RetroLinker.Models.Windows;

[SupportedOSPlatform(App.PlatformWin)]
public class WindowsLinkParameters : LinkParameters, IWindowsLinkParameters
{
    public string? RaWorkDir { get; init; }
    public bool TileIcoNameVisible { get; init; }
    public bool TileIcoNameDark { get; init; }
    public bool TileIcoAllUsers { get; init; }
    public bool TileIcoOverwrite { get; init; }
    public string? TileIcoImagePath { get; init; }
    
    public WindowsLinkParameters(LinkWriter linkWriter)
    {
        RaExecutable = linkWriter.RaExecutable;
        RaWorkDir = linkWriter.RaWorkDir ?? FileOps.GetDirFromPath(RaExecutable); // <= By default
        RaArguments = linkWriter.RaArguments;
        Description = linkWriter.Description;
        IconPath = linkWriter.IconPath;
        FriendlyName = linkWriter.FriendlyName;
        OutputFileName = linkWriter.OutputFileName;
        OutputPath = linkWriter.OutputPath;
        EnvironmentVariables = linkWriter.EnvironmentVariables;
        TileIcoNameVisible = linkWriter.TileIcoNameVisible;
        TileIcoNameDark = linkWriter.TileIcoNameDark;
        TileIcoAllUsers = linkWriter.TileIcoAllUsers;
        TileIcoOverwrite = linkWriter.TileIcoOverwrite;
        TileIcoImagePath = linkWriter.TileIcoImagePath;
        UpdateHashedVariables();
    }

    public WindowsLinkParameters(SharpShellLink.Shortcut shortcut, string linkPath)
    {
        System.ArgumentException.ThrowIfNullOrEmpty(shortcut.StringData?.CommandLineArguments);
        // TODO: if target path is cmd, then the environment variables are set
        //  And our target is actually inside CommandLineArguments
        string targetPath;
        if (shortcut.LinkTargetIDList is not null) targetPath = shortcut.LinkTargetIDList.Path;
        else if (shortcut.ExtraData.EnvironmentVariableDataBlock is not null)
            targetPath = shortcut.ExtraData.EnvironmentVariableDataBlock.TargetUnicode;
        else throw new System.ArgumentException("Can't get target from shortcut");
            
        RaExecutable = targetPath;
        RaArguments = shortcut.StringData.CommandLineArguments;
        Description = shortcut.StringData.NameString;
        IconPath = shortcut.StringData.IconLocation;
        OutputFileName = shortcut.LinkTargetIDList?.DisplayName ?? FileOps.GetFileNameFromPath(linkPath);
        FriendlyName = FileOps.GetFileNameNoExtFromPath(OutputFileName);
        OutputPath = linkPath;
        TileIcoImagePath = string.Empty; // <= It can be done, but this app can't handle all the things a Tile Icon has  
    }

    private int _outputCount;
    private enum WindowsBuildOptions { Shell, TileIco }

    public override async Task<LinkWriterResult> CreateOsShortcut()
    {
        // TODO: Have a more explicit way to tell it should use 'tileico'
        var buildOption = ((SettingsOps.GetCachedSettings().TileIcoPath is not null) && (_outputCount < 1)) 
            ? WindowsBuildOptions.TileIco : WindowsBuildOptions.Shell;
        
        _outputCount++;
        return buildOption switch {
            WindowsBuildOptions.Shell => await BuildWinShortcut_Ssl(),
            WindowsBuildOptions.TileIco => await BuildWinShortcut_TileIco(),
            _ => throw new System.ArgumentOutOfRangeException(nameof(buildOption), buildOption, null)
        };
    }

    private async Task<LinkWriterResult> BuildWinShortcut_Ssl() {
        if (string.IsNullOrEmpty(IconPath)) IconPath = RaExecutable;
        return await CreateShortcut(ShortcutManager.CreateShortcut);
    }

    private async Task<LinkWriterResult> BuildWinShortcut_TileIco() 
        => await CreateShortcut(ShortcutManager.CreateTileIcoShortcut); 
}