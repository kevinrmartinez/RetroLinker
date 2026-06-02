/*
    A .NET GUI application to help create desktop links of games running on RetroArch.
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
using RetroLinker.Models.Generic;
using RetroLinker.Translations;
using ShortcutCreator = RetroLinker.Models.Windows.ShortcutCreator;

namespace RetroLinker.Models
{
    public class Shortcutter
    {
        // TODO: Revise the names and setter of the properties (>= 0.8)
        #region Object
        
        public string RAdir
        {
            get => ra_dir;
            set { SetRAdir(value); }
        }       // 0

        public string RApath
        {
            get => ra_path; 
            private set { SetRApath(value); }   // TODO: Set later in the workflow 
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
        public List<ShortcutterOutput> OutputPaths { get; set; }      // 9
        public bool VerboseB { get; set; }      // 10
        public bool FullscreenB { get; set; }   // 11
        public bool AccessibilityB { get; set; }// 12
        public bool MenuOnErrorB { get; set; }  // 13
        public string PatchArg { get; set; }    // 14
        public string CONFappend { get; set; }  // 15
        public string SubsysArg { get; set; }      // 16

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
        }

        public Shortcutter(Shortcutter objToClone)
        {
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
        public static List<ShortcutterResult> BuildShortcut(Shortcutter link, bool os)  // TODO: Change from static to dynamic (=0.8)
        {
            // Building the arguments
            link = Commander.CommandBuilder(link);
            return (os) ? BuildWinShortcut(link) : BuildLinShorcut(link);
        }
        
        // Windows
        private static List<ShortcutterResult> BuildWinShortcut(Shortcutter link)
        {
            var ResultList = new List<ShortcutterResult>();
            //WinFuncImport.WinFuncMethods CreateShortcut = WinFuncImport.FuncLoader.GetShortcutMethod();
            
            // Add 2 double quotes for vbs compatibility
            link.Command = Utils.TwoDoubleQuotes(link.Command);
            
            // Try to create every shortcut listed on OutputPaths
            foreach (ShortcutterOutput output in link.OutputPaths)
            {
                var outputFile = output.FullPath;
                var LinkResult = new ShortcutterResult(outputFile);
                ResultList.Add(LinkResult);
                App.Logger?.LogInfo($"Creating \"{outputFile}\"...");
                try 
                { 
                    ShortcutCreator.CreateShortcut(link, outputFile);
                    LinkResult.Messeage = LinkResult.Success1; 
                }
                catch (System.Exception e)
                {
                    App.Logger?.LogErro($"\"{outputFile}\" could not be created!");
                    LinkResult.Messeage = LinkResult.Failure1;
                    LinkResult.Error = true;
                    LinkResult.eMesseage = e.Message;
                }
            }
            
            return ResultList;
        }

        // Linux
        private static List<ShortcutterResult> BuildLinShorcut(Shortcutter link)
        {
            var ResultList = new List<ShortcutterResult>();
            
            if (string.IsNullOrEmpty(link.ICONfile)) /*{ link.ICONfile = FileOps.GetRAIcons(); }*/
            { link.ICONfile = FileOps.DotDesktopRAIcon; }
            
            foreach (var output in link.OutputPaths)
            {
                var outputFile = output.FullPath;
                var LinkResult = new ShortcutterResult(outputFile);
                ResultList.Add(LinkResult);
                App.Logger?.LogInfo($"Creating \"{outputFile}\"...");
                
                try { 
                    Linux.ShortcutCreator.CreateShortcut(link, output);
                    LinkResult.Messeage = LinkResult.Success1;
                }
                catch (System.Exception e)
                {
                    App.Logger?.LogErro($"\"{outputFile}\" could not be created!");
                    App.Logger?.LogErro(e);
                    LinkResult.Messeage = LinkResult.Failure1;
                    LinkResult.Error = true;
                    LinkResult.eMesseage = e.Message;
                }
            }
            return ResultList;
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
            var originalDir = FileOps.GetDirFromPath(originalOutput.FullPath)!;
            var newFileName = originalOutput.FriendlyName + FileOps.GetOutputExt(DesktopOS);
            return (string.IsNullOrEmpty(romCore)) ? new ShortcutterOutput(FileOps.CombineMultipleInputs(originalDir, newFileName))
                    : new ShortcutterOutput(FileOps.CombineMultipleInputs(originalDir, newFileName), romCore);
        }

        public static ShortcutterOutput BuildForOS(bool DesktopOS, string fullPath, string romCore, ShortcutterOutput? baseOutput)
        {
            var newOutput = (DesktopOS) ? new ShortcutterOutput(fullPath) : new ShortcutterOutput(fullPath, romCore);
            
            if (baseOutput is null) return newOutput;
            return (!DesktopOS && baseOutput.CustomEntryName) ? baseOutput : newOutput;
        }
    }
    
    
    public class ShortcutterResult(string outputPath)
    {
        public string OutputPath { get; set; } = outputPath;
        public string? Messeage { get; set; }
        public bool Error { get; set; }
        public string? eMesseage { get; set; }

        
        public readonly string Success1 = resMainView.popLinkSucces;
        public readonly string Failure1 = resMainView.popLinkFailure;
    }
}
