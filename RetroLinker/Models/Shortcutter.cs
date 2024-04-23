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
using System.Runtime.CompilerServices;
using RetroLinker.Translations;

namespace RetroLinker.Models
{
    public class Shortcutter
    {
        #region Object
        
        public string RAdir
        {
            get => ra_dir;
            set { SetRAdir(value); }
        }       // 0

        public string RApath
        {
            get => ra_path; 
            private set { SetRApath(value); }
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
        public List<ShortcutterOutput> OutputPath { get; set; }      // 9
        // public string[] OutputName { get; set; }    // 10
        public bool VerboseB { get; set; }      // 11
        public bool FullscreenB { get; set; }   // 12
        public bool AccessibilityB { get; set; }// 13

        private string ra_dir;
        private string ra_path;
        private string rom_dir;
        private string rom_name;
        
        
        
        public Shortcutter()
        {
            RAdir = string.Empty;
            RApath = string.Empty;
            ROMdir = string.Empty;
            ROMcore = string.Empty;
            Command = string.Empty;
            OutputPath = new List<ShortcutterOutput>();
            // LNKcpy = System.Array.Empty<string>();
            VerboseB = false;
            FullscreenB = false;
            AccessibilityB = false;
        }

        private void SetRAdir(string value)
        {
            ra_dir = value;
            RApath = value;
        }

        private void SetRApath(string value) => ra_path = FileOps.GetDirFromPath(value);

        private void SetROMdir(string value)
        {
            rom_dir = value;
            ROMname = value;
        }

        private void SetROMname(string value) => rom_name = FileOps.GetFileNameFromPath(value);
        
        #endregion

        #region Link Output

        // Link Creatinon - OS selection
        public static List<ShortcutterResult> BuildShortcut(Shortcutter link, bool os)
        {
            return (os) ? BuildWinShortcut(link) : BuildLinShorcut(link);
        }
        
        // Windows
        private static List<ShortcutterResult> BuildWinShortcut(Shortcutter link)
        {
            var ResultList = new List<ShortcutterResult>();
            WinFuncImport.WinFuncMethods CreateShortcut = WinFuncImport.FuncLoader.GetShortcutMethod();

            link.RApath = FileOps.GetDirFromPath(link.RAdir);

            // Double quotes for directories that are parameters ->
            // -> for RetroArch's WorkingDirectory
            link.RApath = Utils.FixUnusualDirectories(link.RApath);

            // -> for RetroArch's executable
            link.RAdir = Utils.FixUnusualDirectories(link.RAdir);

            // Building the arguments
            link = Commander.CommandBuilder(link);

            // Grouping the .lnk parameters
            var CreateShortcutArgs = new object[]
            {
                link.RAdir, link.RApath, link.Command,
                link.ICONfile, link.Desc, link.OutputPath
            };

            
            ShortcutterResult LinkResult;
            // ResultList.Add(LinkResult);
            // System.Diagnostics.Trace.WriteLine($"Creating '{link.OutputPath}'...", App.InfoTrace);
            // try { CreateShortcut.MInfo.Invoke(CreateShortcut.ObjInstance, CreateShortcutArgs); 
            //     LinkResult.Messeage = LinkResult.Success1; }
            // catch (System.Exception e)
            // {
            //     System.Diagnostics.Trace.WriteLine($"'{link.OutputPath}' could not be created!", App.ErroTrace);
            //     LinkResult.Messeage = LinkResult.Failure1;
            //     LinkResult.Error = true;
            //     LinkResult.eMesseage = e.Message;
            // }

            foreach (var output in link.OutputPath)
            {
                var outputFile = output.FullPath;
                LinkResult = new ShortcutterResult(outputFile);
                ResultList.Add(LinkResult);
                CreateShortcutArgs[5] = outputFile;
                System.Diagnostics.Trace.WriteLine($"Creating '{outputFile}'...", App.InfoTrace);
                try { CreateShortcut.MInfo.Invoke(CreateShortcut.ObjInstance, CreateShortcutArgs);
                    LinkResult.Messeage = LinkResult.Success1; }
                catch (System.Exception e)
                {
                    System.Diagnostics.Trace.WriteLine($"'{outputFile}' could not be created!", App.ErroTrace);
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
            
            // Building the arguments
            link = Commander.CommandBuilder(link);
            //link.Command = link.Command.Replace("\"", "\'");
            if (string.IsNullOrEmpty(link.ICONfile)) /*{ link.ICONfile = FileOps.GetRAIcons(); }*/
            { link.ICONfile = FileOps.DotDesktopRAIcon; }
            
            // Applying 'Free Desktop' to link name
            // string[] NameFix = FileOps.DesktopEntryName(link.OutputPath, link.ROMcore);
            // link.OutputPath = NameFix[0];
            
            ShortcutterResult LinkResult;
            // ResultList.Add(LinkResult);
            // System.Diagnostics.Trace.WriteLine($"Creating '{link.OutputPath}'...", App.InfoTrace);
            //
            // try { LinFunc.LinShortcutter.CreateShortcut(link, NameFix[1], byte.MaxValue);
            //     LinkResult.Messeage = LinkResult.Success1; }
            // catch (System.Exception e)
            // {
            //     System.Diagnostics.Trace.WriteLine($"'{link.OutputPath}' could not be created!", App.ErroTrace);
            //     LinkResult.Messeage = LinkResult.Failure1;
            //     LinkResult.Error = true;
            //     LinkResult.eMesseage = e.Message;
            // }

            foreach (var output in link.OutputPath)
            {
                var outputFile = output.FullPath;
                LinkResult = new ShortcutterResult(outputFile);
                ResultList.Add(LinkResult);
                System.Diagnostics.Trace.WriteLine($"Creating '{outputFile}'...", App.InfoTrace);
                
                try { LinFunc.LinShortcutter.CreateShortcut(link, output);
                    LinkResult.Messeage = LinkResult.Success1; }
                catch (System.Exception e)
                {
                    System.Diagnostics.Trace.WriteLine($"'{outputFile}' could not be created!", App.ErroTrace);
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
        public bool ValidOutput { get; private set; }

        public ShortcutterOutput()
        {
            const string NA = "N/A";
            FullPath = NA;
            FriendlyName = NA;
            FileName = NA;
            ValidOutput = false;
        }
        
        public ShortcutterOutput(string fullPath)
        {
            FullPath = fullPath;
            FriendlyName = FileOps.GetFileNameNoExtFromPath(fullPath);
            FileName = FileOps.GetFileNameFromPath(fullPath);
            ValidOutput = true;
        }
        
        public ShortcutterOutput(string fullPath, string romCore)
        {
            var outputNames = FileOps.DesktopEntryName(fullPath, romCore);
            FullPath = outputNames[0];
            FriendlyName = outputNames[1];
            FileName = outputNames[2];
            ValidOutput = true;
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
