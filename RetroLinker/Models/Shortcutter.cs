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
using System.Threading.Tasks;

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
        public bool TileIcoOverwrite { get; set; }      // 20
        public string TileIcoImage { get; set; }        // 21
        
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
            TileIcoOverwrite = true;
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
        public async Task<List<LinkWriterResult>> BuildShortcut(bool os)
        {
            var cachedSettings = SettingsOps.GetCachedSettings();
            // Windows Setup
            var winBuilderOpt = WindowsBuildOptions.Vbs;
            if (cachedSettings.TileIcoPath is not null) winBuilderOpt = WindowsBuildOptions.TileIco;
            
            // Building the arguments
            Command = CommandManager.CommandBuilder(this);
            // TODO: Add a '[SupportedOSPlatformGuard("xxx")]' element/guard (0.10)
            return (os) ? await BuildWinShortcut(winBuilderOpt) : await BuildLinShorcut();
        }

        private async Task<LinkWriterResult> CreateShortcut(LinkParameters outputParameters,
            System.Func<LinkParameters, Task> osCreateShortcut)
        {
            var result = new LinkWriterResult(outputParameters.OutputPath);
            Logger.LogInfo($"Creating \"{result.OutputPath}\"...");
            try {
                await osCreateShortcut(outputParameters);
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

        private Dictionary<WindowsBuildOptions, System.Func<Task<List<LinkWriterResult>>>> _windowsBuilder = new();
        
        [SupportedOSPlatform(App.PlatformWin)]
        private void SetupWindowsBuilder() {
            _windowsBuilder = new() {
                { WindowsBuildOptions.Vbs, BuildWinShortcut_Ssl },
                { WindowsBuildOptions.TileIco, BuildWinShortcut_TileIco }
            };
        }
        
        [SupportedOSPlatform(App.PlatformWin)]
        private async Task<List<LinkWriterResult>> BuildWinShortcut(WindowsBuildOptions builderOpt) {
            SetupWindowsBuilder();
            var builder = _windowsBuilder[builderOpt];
            return await builder();
        }

        [SupportedOSPlatform(App.PlatformWin)]
        private async Task<List<LinkWriterResult>> BuildWinShortcut_Ssl()
        {
            var resultList = new List<LinkWriterResult>();
            if (string.IsNullOrEmpty(ICONfile)) ICONfile = RAdir;
            foreach (var output in OutputPaths) {
                var param = new WindowsLinkParameters(this, output);
                var linkResult = await CreateShortcut(param, Windows.ShortcutManager.CreateShortcut);
                resultList.Add(linkResult);
            }
            return resultList;
        }

        [SupportedOSPlatform(App.PlatformWin)]
        private async Task<List<LinkWriterResult>> BuildWinShortcut_TileIco()
        {
            var results = new List<LinkWriterResult>();
            var tileIcoOutput = OutputPaths.First();
            var param = new WindowsLinkParameters(this, tileIcoOutput);
            var linkResult = await CreateShortcut(param, Windows.ShortcutManager.CreateTileIcoShortcut);
            results.Add(linkResult);
            OutputPaths.Remove(tileIcoOutput);
            if (OutputPaths.Count > 0) results.AddRange(await BuildWinShortcut_Ssl());
            return results;
        }
        

        // == Linux ==
        [SupportedOSPlatform(App.PlatformLin)]
        private async Task<List<LinkWriterResult>> BuildLinShorcut()
        {
            var resultList = new List<LinkWriterResult>();
            if (string.IsNullOrEmpty(ICONfile)) ICONfile = FileOps.DotDesktopRAIcon;
            foreach (var output in OutputPaths) {
                var param = new LinuxLinkParameters(this, output);
                var linkResult = await CreateShortcut(param, Linux.ShortcutManager.CreateShortcut);
                resultList.Add(linkResult);
            }
            return resultList;
        }
        #endregion
    }


    public class ShortcutterOutputttt
    {
        // REWRITE: Revise whole class
        public string FullPath { get; }
        public string FriendlyName { get; private set; }
        public string FileName { get; }
        public bool CustomEntryName { get; set; }
        public bool ValidOutput { get; private set; }

        // Constructors
        public ShortcutterOutputttt()
        {
            const string NA = "N/A";
            FullPath = NA;
            FriendlyName = NA;
            FileName = NA;
            CustomEntryName = false;
            ValidOutput = false;
        }
        
        public ShortcutterOutputttt(string fullPath)
        {
            FullPath = fullPath;
            FriendlyName = FileOps.GetFileNameNoExtFromPath(fullPath);
            FileName = FileOps.GetFileNameFromPath(fullPath);
            CustomEntryName = false;
            ValidOutput = true;
        }
        
        public ShortcutterOutputttt(string fullPath, string? romCore)
        {
            var outputNames = FileOps.DesktopEntryArray(fullPath, romCore);
            FullPath = outputNames[0];
            FriendlyName = outputNames[1];
            FileName = outputNames[2];
            CustomEntryName = false;
            ValidOutput = true;
        }

        public ShortcutterOutputttt(string fullPath, string friendlyName, string fileName)
        {
            FullPath = fullPath;
            FriendlyName = friendlyName;
            FileName = fileName;
            ValidOutput = true;
        }

        public ShortcutterOutputttt(ShortcutterOutputttt primeOutput, string copyOutput)
        {
            FriendlyName = primeOutput.FriendlyName;
            FileName = primeOutput.FileName;
            FullPath = FileOps.CombineMultipleInputs(copyOutput, primeOutput.FileName);
            ValidOutput = true;
        }
        
        // Methods
        public static ShortcutterOutputttt RebuildOutputWithFriendly(ShortcutterOutputttt originalOutput, bool desktopOs, string? romCore)
        {
            // REWRITE: reorganize constructors along with this method
            var originalDir = FileOps.GetDirFromPath(originalOutput.FullPath)!;
            var newFileName = originalOutput.FriendlyName + FileOps.GetOutputExt(desktopOs);
            return (string.IsNullOrEmpty(romCore)) ? new ShortcutterOutputttt(FileOps.CombineMultipleInputs(originalDir, newFileName))
                    : new ShortcutterOutputttt(FileOps.CombineMultipleInputs(originalDir, newFileName), romCore);
        }
    }
}
