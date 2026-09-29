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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace RetroLinker.Models;

public record EnvironmentVariable(string Key, string Value) {
    public override string ToString() =>  $"{Key}={Value}"; // Compatible with Windows and POSIX
    public override int GetHashCode() => Key.GetHashCode();
}

// == Interfaces ==
public interface ILinkParametersBase
{
    /// <summary>
    /// RetroArch's executable, should be a path to a file, or a command
    /// </summary>
    public string RaExecutable { get; }
    public string RaArguments { get; }
    public string? Description { get; }
    public string IconPath { get; }
    public string FriendlyName { get; }
    public string OutputFileName { get; }
    public string OutputPath { get; }
    public string OutputDir => GetOutputDir(this);
    public IReadOnlyList<EnvironmentVariable> EnvironmentVariables { get; }

    protected static string GetOutputDir(ILinkParametersBase l) => FileOps.GetDirFromPath(l.OutputPath) ?? string.Empty;
}

public interface IWindowsLinkParameters : ILinkParametersBase
{
    public string? RaWorkDir { get; }
    public bool TileIcoNameVisible { get; }
    public bool TileIcoNameDark { get; }
    public bool TileIcoAllUsers { get; }
    public bool TileIcoOverwrite { get; }
    public string? TileIcoImagePath { get; }
}

public interface ILinuxLinkParameters : ILinkParametersBase
{
    public bool RaVerbose { get; }
}

public interface IUserLinkParameters
{
    // TODO: Optional stuff may be null
    public string RaRom { get; set; }
    public string RaRomName => GetRomName(this);
    public string RaRomPatch { get; set; }
    public string RaCore { get; set; }
    public string RaCoreSubsystem { get; set; }
    public string? RaConfig { get; set; }
    public string RaAppendedConfig { get; set; }
    public bool RaFullscreen { get; set; }
    public bool RaMenuOnError { get; set; }
    public bool RaAccessibility { get; set; }
    public bool RaVerbose { get; set; }
    public string? RaLogFile { get; set; }
    public bool? CustomEntryName { get; set; }

    protected static string GetRomName(IUserLinkParameters u) => FileOps.GetFileNameNoExtFromPath(u.RaRom);
}

public interface IShortcutBase : IWindowsLinkParameters, ILinuxLinkParameters;

// == Classes ==
// General Builder
public class UserLinkBuilder : IShortcutBase, IUserLinkParameters, INotifyPropertyChanged
{
    public string RaExecutable { get; set => SetAndNotify(value, ref field); }
    public string RaRom { get; set => SetAndNotify(value, ref field); }
    public string RaRomPatch { get; set => SetAndNotify(value, ref field); }
    public string RaCore { get; set => SetAndNotify(value, ref field); }
    public string RaCoreSubsystem { get; set => SetAndNotify(value, ref field); }
    public string? RaConfig { get; set => SetAndNotify(value, ref field); }
    public string RaAppendedConfig { get; set => SetAndNotify(value, ref field); }
    public bool RaFullscreen { get; set => SetAndNotify(value, ref field); }
    public bool RaMenuOnError { get; set => SetAndNotify(value, ref field); }
    public bool RaAccessibility { get; set => SetAndNotify(value, ref field); }
    public bool RaVerbose { get; set => SetAndNotify(value, ref field); }
    public string? RaLogFile { get; set => SetAndNotify(value, ref field); }
    public string? Description { get; set => SetAndNotify(value, ref field); }
    public string IconPath { get; set => SetAndNotify(value, ref field); }
    public bool TileIcoNameVisible { get; set => SetAndNotify(value, ref field); }
    public bool TileIcoNameDark { get; set => SetAndNotify(value, ref field); }
    public bool TileIcoAllUsers { get; set => SetAndNotify(value, ref field); }
    public bool TileIcoOverwrite { get; set => SetAndNotify(value, ref field); }
    public string? TileIcoImagePath { get; set => SetAndNotify(value, ref field); }
    
    public string FriendlyName { get; private set => SetAndNotify(value, ref field); }
    public string OutputFileName { get; private set => SetAndNotify(value, ref field); }
    public bool? CustomEntryName { get; set => SetAndNotify(value, ref field); }
    public string OutputPath { get; private set => SetAndNotify(value, ref field); }
    public string OutputDir => ILinkParametersBase.GetOutputDir(this);
    public IReadOnlyList<EnvironmentVariable> EnvironmentVariables => ObservableEnvironmentVariables;
    
    public string RaArguments { get; }
    public string? RaWorkDir { get; }
    
    private ObservableCollection<EnvironmentVariable> ObservableEnvironmentVariables { get; set => SetAndNotify(value, ref field); }
    
    // private ShortcutBuilder() {}

    public UserLinkBuilder()
    {
        RaExecutable = string.Empty;
        
        RaArguments = string.Empty;
        RaRom = string.Empty;
        RaRomPatch = string.Empty;
        RaCore = string.Empty;
        RaCoreSubsystem = string.Empty;
        RaConfig = string.Empty;
        RaAppendedConfig = string.Empty;
        RaFullscreen = true; 
        RaMenuOnError = true;
        RaAccessibility = true;
        RaVerbose = true;
        
        Description = string.Empty;
        IconPath = string.Empty;
        FriendlyName = string.Empty;
        OutputFileName = string.Empty;
        OutputPath = string.Empty;
        ObservableEnvironmentVariables = new();
        // RaWorkDir = string.Empty;
        // TileIcoImagePath = string.Empty;
    }
    
    public UserLinkBuilder(ILinkParametersBase linkParam)
    {
        RaExecutable = linkParam.RaExecutable;
        
        // TODO
        RaArguments = linkParam.RaArguments; // <= parse arguments
        RaRom = string.Empty;
        RaRomPatch = string.Empty;
        RaCore = string.Empty;
        RaCoreSubsystem = string.Empty;
        RaConfig = string.Empty;
        RaAppendedConfig = string.Empty;
        RaFullscreen = true; 
        RaMenuOnError = true;
        RaAccessibility = true;
        RaVerbose = true;
        
        Description = linkParam.Description;
        IconPath = linkParam.IconPath;
        FriendlyName = linkParam.FriendlyName;
        OutputFileName = linkParam.OutputFileName;
        OutputPath = linkParam.OutputPath;      // <= main output (the one in main view)
        ObservableEnvironmentVariables = new(linkParam.EnvironmentVariables);

        CustomEntryName = (FriendlyName != FileOps.GetFileNameNoExtFromPath(OutputFileName));   // <= Only set on Linux, null on other OSs
        
        var windowsParam =  linkParam as IWindowsLinkParameters;
        var linuxParam =  linkParam as ILinuxLinkParameters;
        RaWorkDir = windowsParam?.RaWorkDir;
        TileIcoNameVisible = windowsParam?.TileIcoNameVisible ?? false;
        TileIcoNameDark = windowsParam?.TileIcoNameDark ?? false;
        TileIcoAllUsers = windowsParam?.TileIcoAllUsers ?? false;
        TileIcoOverwrite = windowsParam?.TileIcoOverwrite ?? false;
        TileIcoImagePath = windowsParam?.TileIcoImagePath;
        RaVerbose = linuxParam?.RaVerbose ?? false;
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetAndNotify<T>(T value, ref T field, [CallerMemberName] string? propertyName = null) {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    public void SetOutputPath(string fullPath) {
        OutputPath = fullPath;
        OutputFileName = FileOps.GetFileNameFromPath(OutputPath);
        FriendlyName = FileOps.GetFileNameNoExtFromPath(OutputFileName);
    }
    
    public void SetOutputPathAndFriendlyName(string fullPath, string friendlyName) {
        SetOutputPath(fullPath);
        FriendlyName = friendlyName;
    }
}

// Output Builders

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
        RaArguments = CommandManager.CommandBuilder(builder);
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
                    parameters.UpdateOutputPath(path);
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

public abstract class LinkParameters : ILinkParametersBase
{
    public string RaExecutable { get; protected init; } = string.Empty;
    public string RaArguments { get; protected init; } = string.Empty;
    public string? Description { get; protected init; }
    public string IconPath { get; protected set; } = string.Empty;
    public string FriendlyName { get; protected init; } = string.Empty;
    public string OutputFileName { get; protected init; } = string.Empty;
    public string OutputPath { get; protected set; } = string.Empty;
    public IReadOnlyList<EnvironmentVariable> EnvironmentVariables { get; protected init; } = new List<EnvironmentVariable>();

    /// <summary>
    /// An OS-agnostic command to write the shortcut to file
    /// </summary>
    /// <returns>A <see cref="LinkWriterResult"/> indicating if the file was whether written successfully or not.</returns>
    public abstract Task<LinkWriterResult> CreateOsShortcut();

    public virtual void UpdateOutputPath(string outputPath) => OutputPath = outputPath;
    
    protected virtual async Task<LinkWriterResult> CreateShortcut(System.Func<LinkParameters, Task> osCreateShortcut)
    {
        var result = new LinkWriterResult(OutputPath);
        Logger.LogInfo($"Creating \"{result.OutputPath}\"...");
        try {
            await osCreateShortcut(this);
            result.ResultSuccess();
        }
        catch (System.Exception ex) {
            Logger.LogWarn($"\"{result.OutputPath}\" could not be created!");
            Logger.LogErro(ex);
            result.ResultFailure(ex.Message);
        }
        return result;
    }
}

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
        var buildOption = (SettingsOps.GetCachedSettings().TileIcoPath is not null && _outputCount < 1) 
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
        return await CreateShortcut(Windows.ShortcutManager.CreateShortcut);
    }

    private async Task<LinkWriterResult> BuildWinShortcut_TileIco() 
        => await CreateShortcut(Windows.ShortcutManager.CreateTileIcoShortcut); 
}

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
    }
    
    // PENDING: Constructor for reading a file

    public override async Task<LinkWriterResult> CreateOsShortcut() {
        if (string.IsNullOrEmpty(IconPath)) IconPath = FileOps.DotDesktopRAIcon;
        return await CreateShortcut(Linux.ShortcutManager.CreateShortcut);
    }
}

/// <summary>
/// A simpe structure to inform if the Link was written correctly or not.
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