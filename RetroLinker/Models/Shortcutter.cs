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
using RetroLinker.Translations;

namespace RetroLinker.Models
{
    public class Shortcutter
    {
        #region Object

        // TODO: Concider using DirectoryInfo and FileInfo instead of strings
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
        public string OutputPath { get; set; }      // 9
        public string[] LNKcpy { get; set; }    // 10
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
            OutputPath = string.Empty;
            LNKcpy = System.Array.Empty<string>();
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

            
            var LinkResult = new ShortcutterResult(link.OutputPath);
            ResultList.Add(LinkResult);
            System.Diagnostics.Trace.WriteLine($"Creating '{link.OutputPath}'...", App.InfoTrace);
            try { CreateShortcut.MInfo.Invoke(CreateShortcut.ObjInstance, CreateShortcutArgs); 
                LinkResult.Messeage = LinkResult.Success1; }
            catch (System.Exception e)
            {
                System.Diagnostics.Trace.WriteLine($"'{link.OutputPath}' could not be created!", App.ErroTrace);
                LinkResult.Messeage = LinkResult.Failure1;
                LinkResult.Error = true;
                LinkResult.eMesseage = e.Message;
            }
            
            for (int i = 0; i < link.LNKcpy.Length; i++)
            {
                LinkResult = new ShortcutterResult(link.LNKcpy[i]);
                ResultList.Add(LinkResult);
                CreateShortcutArgs[5] = link.LNKcpy[i];
                System.Diagnostics.Trace.WriteLine($"Creating '{link.LNKcpy[i]}'...", App.InfoTrace);
                try { CreateShortcut.MInfo.Invoke(CreateShortcut.ObjInstance, CreateShortcutArgs);
                    LinkResult.Messeage = LinkResult.Success1; }
                catch (System.Exception e)
                {
                    System.Diagnostics.Trace.WriteLine($"'{link.LNKcpy[i]}' could not be created!", App.ErroTrace);
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
            string[] NameFix = FileOps.DesktopEntryName(link.OutputPath, link.ROMcore);
            link.OutputPath = NameFix[0];
            
            var LinkResult = new ShortcutterResult(link.OutputPath);
            ResultList.Add(LinkResult);
            System.Diagnostics.Trace.WriteLine($"Creating '{link.OutputPath}'...", App.InfoTrace);
            // TODO: Refactor this into less lines. Maybe just a single loop
            
            try { LinFunc.LinShortcutter.CreateShortcut(link, NameFix[1], byte.MaxValue);
                LinkResult.Messeage = LinkResult.Success1; }
            catch (System.Exception e)
            {
                System.Diagnostics.Trace.WriteLine($"'{link.OutputPath}' could not be created!", App.ErroTrace);
                LinkResult.Messeage = LinkResult.Failure1;
                LinkResult.Error = true;
                LinkResult.eMesseage = e.Message;
            }

            for (int i = 0; i < link.LNKcpy.Length; i++)
            {
                NameFix = FileOps.DesktopEntryName(link.LNKcpy[i], link.ROMcore);
                link.LNKcpy[i] = NameFix[0];
                LinkResult = new ShortcutterResult(link.LNKcpy[i]);
                ResultList.Add(LinkResult);
                System.Diagnostics.Trace.WriteLine($"Creating '{link.LNKcpy[i]}'...", App.InfoTrace);
                
                try { LinFunc.LinShortcutter.CreateShortcut(link, NameFix[1], makeCopyIndex:(byte)i);
                    LinkResult.Messeage = LinkResult.Success1; }
                catch (System.Exception e)
                {
                    System.Diagnostics.Trace.WriteLine($"'{link.LNKcpy[i]}' could not be created!", App.ErroTrace);
                    LinkResult.Messeage = LinkResult.Failure1;
                    LinkResult.Error = true;
                    LinkResult.eMesseage = e.Message;
                }
            }
            return ResultList;
        }

        #endregion
    }

    
    public class ShortcutterResult(string outputDir)
    {
        public string OutputDir { get; set; } = outputDir;
        public string? Messeage { get; set; }
        public bool Error { get; set; }
        public string? eMesseage { get; set; }

        
        public readonly string Success1 = resMainView.popLinkSucces;
        public readonly string Failure1 = resMainView.popLinkFailure;
    }
}
