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

using RetroLinker.Models.IO;
using RetroLinker.Models.Settings;

namespace RetroLinker.Models.DesktopLink;

public class UserLinkBuilder : NotifyPropertyBase, IShortcutBase, IUserLinkBuilder
{
    public string RaExecutable { get; set => SetAndNotify(value, ref field); }
    public string RaRom { get; set => SetAndNotify(value, ref field); }
    public RomPatchArgs? RaRomPatch { get; set => SetAndNotify(value, ref field); }
    public string RaCore { get; set => SetAndNotify(value, ref field); }
    public SubsystemArgs? RaCoreSubsystem { get; set => SetAndNotify(value, ref field); }
    public string? RaConfig { get; set => SetAndNotify(value, ref field); }
    public IReadOnlyAppendedConfigs? RaAppendedConfig { get => ObservableRaAppendedConfig; 
        set => SetRaAppendedConfigStub(value); }
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
    
    public bool? CustomEntryName { get; set => SetAndNotify(value, ref field); }
    public string FriendlyName { get; private set => SetAndNotify(value, ref field); }
    public string OutputFileName { get; private set => SetAndNotify(value, ref field); }
    public string OutputPath { get; private set => SetAndNotify(value, ref field); }
    public string OutputDir => ILinkParametersBase.GetOutputDir(this);
    public IReadOnlyList<EnvironmentVariable> EnvironmentVariables => ObservableEnvironmentVariables;
    
    private ObservableAppendedConfigs? ObservableRaAppendedConfig { get; set => SetAndNotify(value, ref field); }
    private ObservableCollection<EnvironmentVariable> ObservableEnvironmentVariables { get; set => SetAndNotify(value, ref field); }
    
    // These two are required by the contract, but not used here
    public string RaArguments { get; }
    public string? RaWorkDir { get; }
    
    // private ShortcutBuilder() {}

    public UserLinkBuilder()
    {
        RaExecutable = string.Empty;
        
        RaArguments = string.Empty;
        RaRom = string.Empty;
        RaCore = string.Empty;
        
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
        
        RaArguments = linkParam.RaArguments;
        // This can throw CommandLineParserException, caller should catch it
        var parsedArguments = CommandLine.Manager.RetroArchCommandParser(RaArguments);
        RaRom = parsedArguments.RaRom;
        RaRomPatch = parsedArguments.RaRomPatch;
        RaCore = parsedArguments.RaCore;
        RaCoreSubsystem = parsedArguments.RaCoreSubsystem;
        RaConfig = parsedArguments.RaConfig;
        ObservableRaAppendedConfig = (parsedArguments.RaAppendedConfig is {} appendedConfigs) 
            ? new(appendedConfigs) : new();
        RaFullscreen = parsedArguments.RaFullscreen; 
        RaMenuOnError = parsedArguments.RaMenuOnError;
        RaAccessibility = parsedArguments.RaAccessibility;
        RaVerbose = parsedArguments.RaVerbose;
        
        Description = linkParam.Description;
        IconPath = linkParam.IconPath;
        FriendlyName = linkParam.FriendlyName;
        OutputFileName = linkParam.OutputFileName;
        OutputPath = linkParam.OutputPath;      // <= main output (the one in main view)
        ObservableEnvironmentVariables = new(linkParam.EnvironmentVariables);

        CustomEntryName = false;   // <= Only set on Linux, null on other OSs; TODO: Should reverse the Desktop Entry naming convention
        
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

    private void SetRaAppendedConfigStub(IReadOnlyList<string>? list) {
        if (list is not ObservableAppendedConfigs observableCollection) return;
        ObservableRaAppendedConfig = observableCollection;
    }
    
    /* == Inherited Methods == */
    public bool AddAppendedConfig(string appendedConfigPath) {
        if (ObservableRaAppendedConfig is  null) return false;
        ObservableRaAppendedConfig.Add(appendedConfigPath);
        return true;
    }

    public bool RemoveAppendedConfig(string appendedConfigPath) {
        if (ObservableRaAppendedConfig is  null) return false;
        ObservableRaAppendedConfig.Remove(appendedConfigPath);
        return true;
    }

    public bool ClearAppendedConfig() {
        if (ObservableRaAppendedConfig is  null) return false;
        ObservableRaAppendedConfig.Clear();
        return true;
    }

    public bool AddEnvironmentVariable(EnvironmentVariable envVar) {
        ObservableEnvironmentVariables.Add(envVar);
        return true;
    }

    public bool RemoveEnvironmentVariable(EnvironmentVariable envVar) {
        ObservableEnvironmentVariables.Remove(envVar);
        return true;
    }

    public bool ClearEnvironmentVariables() {
        ObservableEnvironmentVariables.Clear();
        return true;
    }
    
    /* == Class Methods == */
    public void SetOutputPath(string fullPath) {
        OutputPath = fullPath;
        OutputFileName = FileOps.GetFileNameFromPath(OutputPath);
        FriendlyName = FileOps.GetFileNameNoExtFromPath(OutputFileName);
    }
    
    public void SetOutputPathAndFriendlyName(string fullPath, string friendlyName) {
        SetOutputPath(fullPath);
        FriendlyName = friendlyName;
    }

    public void RestoreRemembered(RememberedUserChoices userChoices)
    {
        RaFullscreen = userChoices.StoredRaFullscreen.GetValueOrDefault(); 
        RaMenuOnError = userChoices.StoredRaMenuOnError.GetValueOrDefault();
        RaAccessibility = userChoices.StoredRaAccessibility.GetValueOrDefault();
        RaVerbose = userChoices.StoredVerbose.GetValueOrDefault();
        RaLogFile = userChoices.StoredRaLogFile;
        foreach (var variable in userChoices.StoredEnvironmentVariables)
            AddEnvironmentVariable(variable);
    }
}