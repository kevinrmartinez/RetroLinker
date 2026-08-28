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
using RetroLinker.Models.Generic;
using RetroLinker.Translations;

namespace RetroLinker.Models
{
    public class Shortcutter
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
            private set { SetRApath(value); }   // TODO: Set later in the workflow (0.9)
        }      // 1

        public string ROMdir
        {
            get => rom_dir; 
            set { SetROMdir(value); }
        }     // 2

        public string ROMname
        {
            get => rom_name;
            private set { SetROMname(value); }
        }    // 3
        public string ROMcore { get; set; }     // 4
        public string? CONFfile { get; set; }   // 5
        public string? ICONfile { get; set; }   // 6
        public string Command { get; set; }     // 7
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
            FinishSetup();
        }

        public Shortcutter(Shortcutter objToClone)
        {
            // TODO: Inherit the class from IClonable, and replace this
            RAdir = objToClone.RAdir;
            RApath = objToClone.RApath;
            ROMdir = objToClone.ROMdir;
            ROMname = objToClone.ROMname;
            ROMcore = objToClone.ROMcore;
            CONFfile = objToClone.CONFfile;
            ICONfile = objToClone.ICONfile;
            Command = objToClone.Command;
            Desc = objToClone.Desc;
            OutputPaths = objToClone.OutputPaths;
            VerboseB = objToClone.VerboseB;
            FullscreenB = objToClone.FullscreenB;
            AccessibilityB = objToClone.AccessibilityB;
            MenuOnErrorB = objToClone.MenuOnErrorB;
            PatchArg = objToClone.PatchArg;
            CONFappend = objToClone.CONFappend;
            SubsysArg = objToClone.SubsysArg;
            TileIcoNameVisible = objToClone.TileIcoNameVisible;
            TileIcoNameDark = objToClone.TileIcoNameDark;
            TileIcoAllUsers  = objToClone.TileIcoAllUsers;
            TileIcoImage  = objToClone.TileIcoImage;
            FinishSetup();
        }

        private void FinishSetup()
        {
            _windowsBuilder = new() {
                { WindowsBuildOptions.Vbs, BuildWinShortcut_Vbs },
                { WindowsBuildOptions.TileIco, BuildWinShortcut_TileIco }
            };
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
        public List<ShortcutterResult> BuildShortcut(bool os)
        {
            var cachedSettings = SettingsOps.GetCachedSettings();
            // Windows Setup
            var winBuilderOpt = WindowsBuildOptions.Vbs;
            if (cachedSettings.TileIcoPath is not null) winBuilderOpt = WindowsBuildOptions.TileIco;
            
            // Building the arguments
            Command = CommandManager.CommandBuilder(this);
            return (os) ? BuildWinShortcut(winBuilderOpt) : BuildLinShorcut(this);
        }

        private ShortcutterResult CreateShortcut(ref readonly ShortcutterOutput output,
            System.Action<Shortcutter, ShortcutterOutput> osCreateShortcut)
        {
            var result = new ShortcutterResult(output.FullPath);
            Logger.LogInfo($"Creating \"{result.OutputPath}\"...");
            try {
                osCreateShortcut(this, output);
                result.Message = result.Success1;
            }
            catch (System.Exception ex)
            {
                Logger.LogWarn($"\"{result.OutputPath}\" could not be created!");
                Logger.LogErro(ex);
                result.Message = result.Failure1;
                result.Error = true;
                result.ExMessage = ex.Message;
            }
            return result;
        }
        
        // Windows
        private enum WindowsBuildOptions { Vbs, TileIco }

        private Dictionary<WindowsBuildOptions, System.Func<List<ShortcutterResult>>> _windowsBuilder = new();
        
        private List<ShortcutterResult> BuildWinShortcut(WindowsBuildOptions builderOpt) {
            var builder = _windowsBuilder[builderOpt];
            return builder();
        }

        private List<ShortcutterResult> BuildWinShortcut_Vbs()
        {
            var resultList = new List<ShortcutterResult>();
            Command = Utils.TwoDoubleQuotes(Command); // Add 2 double quotes for vbs compatibility
            foreach (var output in OutputPaths) {
                var linkResult = CreateShortcut(in output, Windows.ShortcutCreator.CreateShortcut);
                resultList.Add(linkResult);
            }
            return resultList;
        }

        private List<ShortcutterResult> BuildWinShortcut_TileIco()
        {
            var results = new List<ShortcutterResult>();
            var tileIcoOutput = OutputPaths.First();
            var linkResult = CreateShortcut(in tileIcoOutput, Windows.ShortcutCreator.CreateTileIcoShortcut);
            results.Add(linkResult);
            OutputPaths.Remove(tileIcoOutput);
            if (OutputPaths.Count > 0) results.AddRange(BuildWinShortcut_Vbs());
            return results;
        }
        

        // Linux
        private List<ShortcutterResult> BuildLinShorcut(Shortcutter link)
        {
            var resultList = new List<ShortcutterResult>();
            if (string.IsNullOrEmpty(link.ICONfile)) link.ICONfile = FileOps.DotDesktopRAIcon;
            foreach (var output in link.OutputPaths) {
                var linkResult = CreateShortcut(in output, Linux.ShortcutCreator.CreateShortcut);
                resultList.Add(linkResult);
            }
            return resultList;
        }
        #endregion
    }


    public class ShortcutterOutput
    {
        public string FullPath { get; set; }
        public string FriendlyName { get; set; }
        public string FileName { get; set; }
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
        void RebuildOutput(string newFullPath)
        {
            if (FullPath == newFullPath) return;
            FullPath = newFullPath;
            FileName = FileOps.GetFileNameFromPath(newFullPath);
        }

        public static ShortcutterOutput RebuildOutputWithFriendly(ShortcutterOutput originalOutput, bool DesktopOS, string? romCore)
        {
            // REWRITE: reorganize constructors along with this method
            var originalDir = FileOps.GetDirFromPath(originalOutput.FullPath)!;
            var newFileName = originalOutput.FriendlyName + FileOps.GetOutputExt(DesktopOS);
            return (string.IsNullOrEmpty(romCore)) ? new ShortcutterOutput(FileOps.CombineMultipleInputs(originalDir, newFileName))
                    : new ShortcutterOutput(FileOps.CombineMultipleInputs(originalDir, newFileName), romCore);
        }

        // REWRITE: Obsolete?
        public static ShortcutterOutput BuildForOS(bool DesktopOS, string fullPath, string romCore, ShortcutterOutput? baseOutput)
        {
            var newOutput = (DesktopOS) ? new ShortcutterOutput(fullPath) : new ShortcutterOutput(fullPath, romCore);
            
            if (baseOutput is null) return newOutput;
            return (!DesktopOS && baseOutput.CustomEntryName) ? baseOutput : newOutput;
        }
    }
    
    
    public struct ShortcutterResult(string outputPath)
    {
        public string OutputPath { get; } = outputPath;
        public string? Message { get; set; }
        public bool Error { get; set; }
        public string? ExMessage { get; set; }

        
        public readonly string Success1 = resMainView.popLinkSucces;
        public readonly string Failure1 = resMainView.popLinkFailure;
    }
}
