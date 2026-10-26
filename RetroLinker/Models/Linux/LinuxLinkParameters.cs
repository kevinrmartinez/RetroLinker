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
using RetroLinker.Models.DesktopLink;
using RetroLinker.Models.IO;

namespace RetroLinker.Models.Linux;

public interface IDesktopEntry
{
    public IReadOnlyList<string> CategoriesRo { get; }
    public string? Comment { get; }
    public string Exec { get; }
    public string? Icon { get; }
    public string Name { get; }
    public bool StartupNotify { get; }
    public bool Terminal { get; }
    public ShortcutManager.DesktopEntryType Type { get; }
}

[SupportedOSPlatform(App.PlatformLin)]
public class LinuxLinkParameters : LinkParameters, ILinuxLinkParameters
{
    public bool RaVerbose { get; init; }
    
    public LinuxLinkParameters(IShortcutBase linkWriter)
    {
        EnvironmentVariables = linkWriter.EnvironmentVariables;
        UpdateHashedVariables();
        
        RaExecutable = CreateExecutableLine(linkWriter);
        RaArguments = linkWriter.RaArguments;
        Description = linkWriter.Description;
        IconPath = linkWriter.IconPath;
        RaVerbose = linkWriter.RaVerbose;
        FriendlyName = linkWriter.FriendlyName;
        OutputPath = linkWriter.OutputPath;
        _ogOutputPath = linkWriter.OutputPath;
    }

    public LinuxLinkParameters(IDesktopEntry desktopEntry, string entryFilePath)
    {
        var bashWrapper = Manager.ResolveExecLine(desktopEntry.Exec);
        RaExecutable = bashWrapper.RealTargetExecutable;
        RaArguments = bashWrapper.RealTargetArguments;
        HashedEnvironmentVariables = bashWrapper.EnvironmentVariables;
        EnvironmentVariables = new List<EnvironmentVariable>(HashedEnvironmentVariables);
        
        Description = desktopEntry.Comment;
        IconPath = desktopEntry.Icon ?? string.Empty;
        RaVerbose = desktopEntry.Terminal;
        FriendlyName = desktopEntry.Name;
        OutputPath = entryFilePath;
        OutputFileName = FileOps.GetFileNameFromPath(OutputPath);
        _ogOutputPath = entryFilePath;
    }

    public override async Task<LinkWriterResult> CreateOsShortcut() {
        if (string.IsNullOrEmpty(IconPath)) IconPath = FileOps.DotDesktopRAIcon;
        return await CreateShortcut(ShortcutManager.CreateShortcut);
    }

    private string CreateExecutableLine(IShortcutBase linkWriter)
    {
        // Quotes are needed if the path to the executable has problematic shell characters
        // Double quotes will be used to more easily handle the reading of the file
        var correctedBin = Utils.ConditionallyQuoteString(linkWriter.RaExecutable, false);
        if (HashedEnvironmentVariables.Count < 1) return correctedBin;
        else
        {
            var binLine = new List<string>();
            binLine.Add(Manager.BashEnvCommand);
            foreach (var environmentVariable in HashedEnvironmentVariables)
                binLine.Add(environmentVariable.ToStringBash());
            binLine.Add(correctedBin);
            return string.Join(Manager.SepWhiteSpace, binLine);
        }
    }
}