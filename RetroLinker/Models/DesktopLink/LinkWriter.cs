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
using RetroLinker.Models.IO;
using RetroLinker.Models.Settings;
using RetroLinker.Models.Linux;
using RetroLinker.Models.Windows;

namespace RetroLinker.Models.DesktopLink;

public class LinkWriter : IShortcutBase
{
    // Some setters may not be necessary
    public string RaExecutable { get; set; } 
    public string RaArguments { get; set; }
    public string? Description { get; set; }
    public string IconPath { get; set; }
    public string FriendlyName { get; private set; }
    public string OutputFileName { get; private set; }
    public string OutputPath { get; private set; }
    public IReadOnlyList<EnvironmentVariable> EnvironmentVariables { get; }
    public string? RaWorkDir { get; }
    public bool TileIcoNameVisible { get; set; }
    public bool TileIcoNameDark { get; set; }
    public bool TileIcoAllUsers { get; set; }
    public bool TileIcoOverwrite { get; set; }
    public string? TileIcoImagePath { get; set; }
    public bool RaVerbose { get; set; }
    
    public List<string> OutputPaths { get; } = new();

    // private LinkWriter() { }

    public LinkWriter(UserLinkBuilder builder)
    {
        RaExecutable = builder.RaExecutable;
        RaArguments = Manager.RetroArchCommandBuilder(builder);
        Description = builder.Description;
        IconPath = builder.IconPath;
        FriendlyName = builder.FriendlyName;
        OutputFileName = builder.OutputFileName;
        OutputPath = builder.OutputPath;
        EnvironmentVariables = builder.EnvironmentVariables;
        RaWorkDir = builder.RaWorkDir;
        TileIcoNameVisible = builder.TileIcoNameVisible;
        TileIcoNameDark = builder.TileIcoNameDark;
        TileIcoAllUsers = builder.TileIcoAllUsers;
        TileIcoOverwrite = builder.TileIcoOverwrite;
        TileIcoImagePath = builder.TileIcoImagePath;
        RaVerbose = builder.RaVerbose;
        
        foreach (var copyDirectory in SettingsOps.GetCachedSettings().SavedCopyPaths)
            OutputPaths.Add(GetNewOutputPath(copyDirectory));
    }
    
    public string GetNewOutputPath(string outputDirectory) 
        => FileOps.CombineMultipleInputs(outputDirectory, OutputFileName);
    
    public void SetOutputPath(string fullPath) {
        OutputPath = fullPath;
        OutputFileName = FileOps.GetFileNameFromPath(OutputPath);
        FriendlyName = FileOps.GetFileNameNoExtFromPath(OutputFileName);
    }
    
    public void SetOutputPathAndFriendlyName(string fullPath, string friendlyName) {
        SetOutputPath(fullPath);
        FriendlyName = friendlyName;
    }
    
    
    /// <summary>
    /// Finalizes the object and build all shortcut files for the current OS.
    /// </summary>
    /// <param name="os">The OS identifier</param>
    /// <returns>A list of <see cref="LinkWriterResult"/>.</returns>
    public async Task<List<LinkWriterResult>> BuildShortcut(bool os) 
    {
        var results = new List<LinkWriterResult>();
        var outputPath = OutputPath;
        try
        {
            // RaArguments was set on constructor
            System.ArgumentException.ThrowIfNullOrEmpty(RaArguments);   // ...but let's check just in case
            var settings = SettingsOps.GetCachedSettings();
            // TODO: Add a '[SupportedOSPlatformGuard("xxx")]' element/guard (0.10)
            LinkParameters parameters = (os) ? new WindowsLinkParameters(this) : new LinuxLinkParameters(this);
            results.Add(await parameters.CreateOsShortcut());
            if (settings.MakeLinkCopy) {
                foreach (var path in OutputPaths) {
                    parameters.SetToCopyOutputPath(path);
                    outputPath = parameters.OutputPath;             // <= Trying to keep track in case of an exception
                    results.Add(await parameters.CreateOsShortcut());
                }
            }
        }
        catch (System.Exception ex) {
            var error = new LinkWriterResult(outputPath);
            error.ResultFailure(ex.Message);
            results.Add(error);
        }
        return results;
    }
}

/// <summary>
/// A simple structure to inform if the Link was written correctly or not.
/// </summary>
/// <param name="OutputPath">The full path of link being handled.</param>
public record struct LinkWriterResult(string OutputPath)
{
    public string OutputPath { get; } = OutputPath;
    public string? Message { get; private set; }
    public bool Error { get; private set; }
    public string? ExMessage { get; private set; }
        
    private static readonly string Success1 = Translations.resMainView.popLinkSucces;
    private static readonly string Failure1 = Translations.resMainView.popLinkFailure;

    public void ResultSuccess() {
        Message = Success1;
        Error = false;
        ExMessage = null;
    }

    public void ResultFailure(string? exMessage = null) {
        Message = Failure1;
        Error = true;
        ExMessage = exMessage;
    }
}