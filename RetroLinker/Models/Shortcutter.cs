/*
    RetroLinker: A .NET GUI application to help create desktop links of games running on RetroArch.
    Copyright (C) 2023  Kevin Rafael Martinez Johnston

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
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using RetroLinker.Models.Generic;
using RetroLinker.Translations;

namespace RetroLinker.Models
{
    public class Shortcutter : System.ICloneable
    {
        // TODO: Revise the names and setter of the properties (0.9)
        #region Object
        
        public string RAdir
        {
            get => ra_dir;
            set { SetRAdir(value); }
        }       // 0

        public string RApath
        {
            get => ra_path; 
            private set { SetRApath(value); }   // TODO: Retire; move to LinkParameters (0.9)
        }      // 1

        public string ROMdir
        {
            get => rom_dir; 
            set { SetROMdir(value); }
        }     // 2

        public string ROMname
        {
            get => rom_name;
            private set { SetROMname(value); }  // TODO: Retire, it's not used in anything (0.9)
        }    // 3
        public string ROMcore { get; set; }     // 4
        public string? CONFfile { get; set; }   // 5
        public string ICONfile { get; set; }   // 6
        public string Command { get; private set; }     // 7
        public string? Desc { get; set; }       // 8
        public List<ShortcutterOutput> OutputPaths { get; set; }    // 9
        public bool VerboseB { get; set; }      // 10
        public bool FullscreenB { get; set; }   // 11
        public bool AccessibilityB { get; set; }// 12
        public bool MenuOnErrorB { get; set; }  // 13
        public string PatchArg { get; set; }    // 14
        public string CONFappend { get; set; }  // 15
        public string SubsysArg { get; set; }   // 16
        public bool TileIcoNameVisible { get; set; }    // 17
        public bool TileIcoNameDark { get; set; }       // 18
        public bool TileIcoAllUsers { get; set; }       // 19
        public string TileIcoImage { get; set; }        // 20
        
        private string ra_dir   = string.Empty;
        private string ra_path  = string.Empty;
        private string rom_dir  = string.Empty;
        private string rom_name = string.Empty;
        
        
        
        public Shortcutter()
        {
            RAdir = string.Empty;
            RApath = string.Empty;
            ROMdir = string.Empty;
            ROMcore = string.Empty;
            ICONfile = string.Empty;
            Command = string.Empty;
            OutputPaths = new List<ShortcutterOutput>();
            VerboseB = false;
            FullscreenB = false;
            AccessibilityB = false;
            MenuOnErrorB = false;
            PatchArg = string.Empty;
            CONFappend = string.Empty;
            SubsysArg = string.Empty;
            TileIcoImage = string.Empty;
        }

        public object Clone()
        {
            var newShortcut = (Shortcutter)this.MemberwiseClone();
            newShortcut.OutputPaths = new List<ShortcutterOutput>(OutputPaths);
            return newShortcut;
        }

        private void SetRAdir(string value) {
            ra_dir = value;
            RApath = value;
        }

        private void SetRApath(string value)
        {
            var result = FileOps.GetDirFromPath(value);
            ra_path = (string.IsNullOrEmpty(result)) ? string.Empty : result;
        }

        private void SetROMdir(string value) {
            rom_dir = value;
            ROMname = value;
        }

        private void SetROMname(string value) => rom_name = FileOps.GetFileNameFromPath(value);
        
        #endregion

        
        #region Link Output
        // Link Creation - OS selection
        public async Task<List<ShortcutterResult>> BuildShortcut(bool os)
        {
            var cachedSettings = SettingsOps.GetCachedSettings();
            // Windows Setup
            var winBuilderOpt = WindowsBuildOptions.Vbs;
            if (cachedSettings.TileIcoPath is not null) winBuilderOpt = WindowsBuildOptions.TileIco;
            
            // Building the arguments
            Command = CommandManager.CommandBuilder(this);
            // TODO: Add a '[SupportedOSPlatformGuard("xxx")]' element/guard
            return (os) ? await BuildWinShortcut(winBuilderOpt) : await BuildLinShorcut();
        }

        private async Task<ShortcutterResult> CreateShortcut(ShortcutterOutput output,
            System.Func<LinkParameters, Task> osCreateShortcut)
        {
            var linkParams = new LinkParameters(this, output);
            var result = new ShortcutterResult(linkParams.OutputPath);
            Logger.LogInfo($"Creating \"{result.OutputPath}\"...");
            try {
                await osCreateShortcut(linkParams);
                result.ResultSuccess();
            }
            catch (System.Exception ex) {
                Logger.LogWarn($"\"{result.OutputPath}\" could not be created!");
                Logger.LogErro(ex);
                result.ResultFailure(ex.Message);
            }
            return result;
        }
        
        // == Windows ==
        private enum WindowsBuildOptions { Vbs, TileIco }

        private Dictionary<WindowsBuildOptions, System.Func<Task<List<ShortcutterResult>>>> _windowsBuilder = new();
        
        [SupportedOSPlatform(App.PlatformWin)]
        private void SetupWindowsBuilder() {
            _windowsBuilder = new() {
                { WindowsBuildOptions.Vbs, BuildWinShortcut_Ssl },
                { WindowsBuildOptions.TileIco, BuildWinShortcut_TileIco }
            };
        }
        
        [SupportedOSPlatform(App.PlatformWin)]
        private async Task<List<ShortcutterResult>> BuildWinShortcut(WindowsBuildOptions builderOpt) {
            SetupWindowsBuilder();
            var builder = _windowsBuilder[builderOpt];
            return await builder();
        }

        [SupportedOSPlatform(App.PlatformWin)]
        private async Task<List<ShortcutterResult>> BuildWinShortcut_Ssl()
        {
            var resultList = new List<ShortcutterResult>();
            if (string.IsNullOrEmpty(ICONfile)) ICONfile = RAdir;
            foreach (var output in OutputPaths) {
                var linkResult = await CreateShortcut(output, Windows.ShortcutManager.CreateShortcut);
                resultList.Add(linkResult);
            }
            return resultList;
        }

        [SupportedOSPlatform(App.PlatformWin)]
        private async Task<List<ShortcutterResult>> BuildWinShortcut_TileIco()
        {
            var results = new List<ShortcutterResult>();
            var tileIcoOutput = OutputPaths.First();
            var linkResult = await CreateShortcut(tileIcoOutput, Windows.ShortcutManager.CreateTileIcoShortcut);
            results.Add(linkResult);
            OutputPaths.Remove(tileIcoOutput);
            if (OutputPaths.Count > 0) results.AddRange(await BuildWinShortcut_Ssl());
            return results;
        }
        

        // == Linux ==
        [SupportedOSPlatform(App.PlatformLin)]
        private async Task<List<ShortcutterResult>> BuildLinShorcut()
        {
            var resultList = new List<ShortcutterResult>();
            if (string.IsNullOrEmpty(ICONfile)) ICONfile = FileOps.DotDesktopRAIcon;
            foreach (var output in OutputPaths) {
                var linkResult = await CreateShortcut(output, Linux.ShortcutManager.CreateShortcut);
                resultList.Add(linkResult);
            }
            return resultList;
        }
        #endregion
    }


    public class ShortcutterOutput
    {
        // REWRITE: Revise whole class
        public string FullPath { get; }
        public string FriendlyName { get; private set; }
        public string FileName { get; }
        public bool CustomEntryName { get; set; }
        public bool ValidOutput { get; private set; }

        // Constructors
        public ShortcutterOutput()
        {
            const string NA = "N/A";
            FullPath = NA;
            FriendlyName = NA;
            FileName = NA;
            CustomEntryName = false;
            ValidOutput = false;
        }
        
        public ShortcutterOutput(string fullPath)
        {
            FullPath = fullPath;
            FriendlyName = FileOps.GetFileNameNoExtFromPath(fullPath);
            FileName = FileOps.GetFileNameFromPath(fullPath);
            CustomEntryName = false;
            ValidOutput = true;
        }
        
        public ShortcutterOutput(string fullPath, string? romCore)
        {
            var outputNames = FileOps.DesktopEntryArray(fullPath, romCore);
            FullPath = outputNames[0];
            FriendlyName = outputNames[1];
            FileName = outputNames[2];
            CustomEntryName = false;
            ValidOutput = true;
        }

        public ShortcutterOutput(string fullPath, string friendlyName, string fileName)
        {
            FullPath = fullPath;
            FriendlyName = friendlyName;
            FileName = fileName;
            ValidOutput = true;
        }

        public ShortcutterOutput(ShortcutterOutput primeOutput, string copyOutput)
        {
            FriendlyName = primeOutput.FriendlyName;
            FileName = primeOutput.FileName;
            FullPath = FileOps.CombineMultipleInputs(copyOutput, primeOutput.FileName);
            ValidOutput = true;
        }
        
        // Methods
        public static ShortcutterOutput RebuildOutputWithFriendly(ShortcutterOutput originalOutput, bool desktopOs, string? romCore)
        {
            // REWRITE: reorganize constructors along with this method
            var originalDir = FileOps.GetDirFromPath(originalOutput.FullPath)!;
            var newFileName = originalOutput.FriendlyName + FileOps.GetOutputExt(desktopOs);
            return (string.IsNullOrEmpty(romCore)) ? new ShortcutterOutput(FileOps.CombineMultipleInputs(originalDir, newFileName))
                    : new ShortcutterOutput(FileOps.CombineMultipleInputs(originalDir, newFileName), romCore);
        }
    }


    public record struct ShortcutOutputHelper
    {
        public readonly string RaCore;
        public readonly List<ShortcutterOutput> Outputs;
        public readonly bool TileIcoAllUsers;

        public ShortcutOutputHelper(in Shortcutter originalShortcut) {
            RaCore = originalShortcut.ROMcore;
            Outputs = originalShortcut.OutputPaths;
            TileIcoAllUsers = originalShortcut.TileIcoAllUsers;
        }
    } 


    public class LinkParameters
    {
        // TODO: Make this class abstract, and create two new classes: `WindowsLinkParameters` and `LinuxLinkParameters` 
        public string RaExecutable { get; }
        public string? RaWorkDir { get; }
        public string RaArguments { get; }
        public string? Description { get; }
        public bool Verbose { get; }
        public string IconPath { get; }
        public string FriendlyName { get; }
        public string OutputPath { get; }
        public bool TileIcoNameVisible { get; }
        public bool TileIcoNameDark { get; }
        public bool TileIcoAllUsers { get; }
        public string TileIcoImagePath { get; }

        public LinkParameters(Shortcutter shortcut, ShortcutterOutput shortcutOutput)
        {
            RaExecutable = shortcut.RAdir;
            RaWorkDir = FileOps.GetDirFromPath(RaExecutable);
            RaArguments = shortcut.Command;
            Description = shortcut.Desc;
            Verbose = shortcut.VerboseB;
            IconPath = shortcut.ICONfile;
            FriendlyName = shortcutOutput.FriendlyName;
            OutputPath = shortcutOutput.FullPath;
            TileIcoNameVisible = shortcut.TileIcoNameVisible;
            TileIcoNameDark = shortcut.TileIcoNameDark;
            TileIcoAllUsers = shortcut.TileIcoAllUsers;
            TileIcoImagePath = shortcut.TileIcoImage;
        }
    }
    
    
    public record struct ShortcutterResult(string OutputPath)
    {
        public string OutputPath { get; } = OutputPath;
        public string? Message { get; private set; }
        public bool Error { get; private set; }
        public string? ExMessage { get; private set; }

        
        private static readonly string Success1 = resMainView.popLinkSucces;
        private static readonly string Failure1 = resMainView.popLinkFailure;

        public void ResultSuccess() {
            Message = Success1;
        }

        public void ResultFailure(string? exMessage = null) {
            Message = Failure1;
            Error = true;
            ExMessage = exMessage;
        }
    }
}
