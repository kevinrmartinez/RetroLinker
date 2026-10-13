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

namespace RetroLinker.Models.Linux;

[SupportedOSPlatform(App.PlatformLin)]
public class LinuxLinkParameters : LinkParameters, ILinuxLinkParameters
{
    public bool RaVerbose { get; init; }

    public LinuxLinkParameters(LinkWriter linkWriter)
    {
        RaExecutable = linkWriter.RaExecutable;
        RaArguments = linkWriter.RaArguments;
        Description = linkWriter.Description;
        IconPath = linkWriter.IconPath;
        RaVerbose = linkWriter.RaVerbose;
        FriendlyName = linkWriter.FriendlyName;
        OutputPath = linkWriter.OutputPath;
        EnvironmentVariables = linkWriter.EnvironmentVariables;
        UpdateHashedVariables();
    }
    
    // PENDING: Constructor for reading a file

    public override async Task<LinkWriterResult> CreateOsShortcut() {
        if (string.IsNullOrEmpty(IconPath)) IconPath = FileOps.DotDesktopRAIcon;
        return await CreateShortcut(ShortcutManager.CreateShortcut);
    }
}