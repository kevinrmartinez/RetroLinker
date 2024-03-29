﻿/*
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
using Avalonia.Platform.Storage;
using RetroLinker.Translations;

namespace RetroLinker.Models
{
    public static class PickerOpt
    {
        static readonly FilePickerFileType win_exe = new(resAvaloniaOps.pckFileTypeExe) { Patterns = new string[] { "*.exe" } };
        static readonly FilePickerFileType appimage = new(resAvaloniaOps.pckFileTypeAppI) { Patterns = new string[] { "*.AppImage" } };
        static readonly FilePickerFileType shellscripts = new(resAvaloniaOps.pckFileTypeSh) { Patterns = new string[] { "*.sh" } };
        static readonly FilePickerFileType config_file = new(resAvaloniaOps.pckFileTypeConf) { Patterns = new string[] { "*.cfg" } };
        static readonly FilePickerFileType win_icon_files = new(resAvaloniaOps.pckFileTypeIco) { Patterns = new string[] { "*.ico" } };
        static readonly FilePickerFileType conv_icon = new(resAvaloniaOps.pckFileTypeConvI) { Patterns = FileOps.WinConvertibleIconsExt };
        static readonly FilePickerFileType lin_icon_files = new(resAvaloniaOps.pckFileTypeIcon) { Patterns = FileOps.LinIconsExt };
        static readonly FilePickerFileType win_lnk = new(resAvaloniaOps.pckFileTypeWinLnk) { Patterns = new string[] { "*.lnk" } };
        static readonly FilePickerFileType lin_lnk = new(resAvaloniaOps.pckFileTypeLinLnk) { Patterns = new string[] { "*.desktop" } };

        public enum OpenOpts { RAexe, RAroms, RAcfg, WINico, RAbin, LINico } 
        public enum SaveOpts { WINlnk, LINdesktop }

        static List<FilePickerFileType> RADirFileTypes_win = new() { win_exe, FilePickerFileTypes.All };
        static List<FilePickerFileType> RADirFileTypes_lin = new() { appimage, shellscripts, FilePickerFileTypes.All };
        static List<FilePickerFileType> CONFIGDirFileTypes = new() { config_file, FilePickerFileTypes.TextPlain };
        static List<FilePickerFileType> ICONfileTypes = new() { win_icon_files, win_exe, conv_icon };
        static List<FilePickerFileType> ICONfileTypes2 = new() { lin_icon_files };
        static List<FilePickerFileType> WinShorTypes = new() { win_lnk };
        static List<FilePickerFileType> LinShorTypes = new() { lin_lnk };
        
        
        public static FilePickerOpenOptions OpenPickerOpt(OpenOpts template)
        {
            var options = new FilePickerOpenOptions();

            switch (template)
            {
                // Windows exe
                case OpenOpts.RAexe:
                    options.AllowMultiple = false;
                    options.Title = resAvaloniaOps.dlgFileRAexe;
                    options.FileTypeFilter = RADirFileTypes_win;
                    break;

                // All files for ROMs
                case OpenOpts.RAroms:
                    options.AllowMultiple = false;
                    options.Title = resAvaloniaOps.dlgFileRAexe;
                    //options.FileTypeFilter = new List<FilePickerFileType> { FilePickerFileTypes.All }; 
                    if (AvaloniaOps.ROMPadreDir != null)
                    { options.SuggestedStartLocation = AvaloniaOps.ROMPadreDir; }
                    // TODO: No funciona con los FilePicker de las distros de linux
                    break;

                // Retroarch config
                case OpenOpts.RAcfg:
                    options.AllowMultiple = false;
                    options.Title = resAvaloniaOps.dlgFileRAcfg;
                    options.FileTypeFilter = CONFIGDirFileTypes;
                    break;

                // Windows icon
                case OpenOpts.WINico:
                    options.AllowMultiple = false;
                    options.Title = resAvaloniaOps.dlgFileIcon;
                    options.FileTypeFilter = ICONfileTypes;
                    break;

                // Linux executable
                case OpenOpts.RAbin:
                    options.AllowMultiple = false;
                    options.Title = resAvaloniaOps.dlgFileRAexe;
                    options.FileTypeFilter = RADirFileTypes_lin;
                    break;

                // Linux images for icons
                case OpenOpts.LINico:
                    options.AllowMultiple = false;
                    options.Title = resAvaloniaOps.dlgFileIcon;
                    options.FileTypeFilter = ICONfileTypes2;
                    break;

                // This part should never happen...
                default:
                    System.Diagnostics.Debug.WriteLine("This part should never happen...", App.DebgTrace);
                    options.AllowMultiple = false;
                    options.Title = resAvaloniaOps.dlgFileInFallback;
                    break;
            }
            return options;
        }

        public static FilePickerSaveOptions SavePickerOpt(SaveOpts template)
        {
            var options = new FilePickerSaveOptions();
            options.ShowOverwritePrompt = true;
            options.Title = resAvaloniaOps.dlgFileLINKDir;
            switch (template) 
            {
                // Windows .lnk
                case SaveOpts.WINlnk:                 
                    options.DefaultExtension = FileOps.WinLinkExt;
                    options.FileTypeChoices = WinShorTypes;
                    break;

                // Linux popular .desktop
                case SaveOpts.LINdesktop:
                    options.DefaultExtension = FileOps.LinLinkExt;
                    options.FileTypeChoices = LinShorTypes;
                    break;
                
                // This part should never happen...
                default:
                    System.Diagnostics.Debug.WriteLine("This part should never happen...", App.DebgTrace);
                    options.Title = resAvaloniaOps.dlgFileOutFallback;
                    break;
            }
            return options;
        }
    }
}
