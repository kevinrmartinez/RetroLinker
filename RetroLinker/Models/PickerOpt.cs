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
using Avalonia.Platform.Storage;
using RetroLinker.Translations;

namespace RetroLinker.Models
{
    public static class PickerOpt
    {
        static readonly FilePickerFileType win_exe = new(resAvaloniaOps.pckFileTypeExe) { Patterns = ["*.exe"] };
        static readonly FilePickerFileType appimage = new(resAvaloniaOps.pckFileTypeAppI) { Patterns = ["*.AppImage"] };
        static readonly FilePickerFileType shellscripts = new(resAvaloniaOps.pckFileTypeSh) { Patterns = ["*.sh"] };
        static readonly FilePickerFileType config_file = new(resAvaloniaOps.pckFileTypeConf) { Patterns = ["*.cfg"] };
        static readonly FilePickerFileType win_icon_files = new(resAvaloniaOps.pckFileTypeIco) { Patterns = ["*.ico"]};
        static readonly FilePickerFileType conv_icon = new(resAvaloniaOps.pckFileTypeConvI) { Patterns = FileOps.WinExtraIconsExt };
        static readonly FilePickerFileType lin_icon_files = new(resAvaloniaOps.pckFileTypeIcon) { Patterns = FileOps.LinIconsExt };
        static readonly FilePickerFileType win_lnk = new(resAvaloniaOps.pckFileTypeWinLnk) { Patterns = ["*.lnk"] };
        static readonly FilePickerFileType lin_lnk = new(resAvaloniaOps.pckFileTypeLinLnk) { Patterns = ["*.desktop"] };
        
        static readonly FilePickerFileType ups_patch = new(resAvaloniaOps.pckFileTypeUPS) { Patterns = ["*.ups"] };
        static readonly FilePickerFileType bps_patch = new(resAvaloniaOps.pckFileTypeBPS) { Patterns = ["*.bps" ] };
        static readonly FilePickerFileType ips_patch = new(resAvaloniaOps.pckFileTypeIPS) { Patterns = ["*.ips"] };
        static readonly FilePickerFileType xd_patch = new(resAvaloniaOps.pckFileTypeXD) { Patterns = ["*.xdelta"] };

        public enum OpenOpts { RAexe, RAroms, RAcfg, WINico, RAbin, LINico } 
        public enum SaveOpts { WINlnk, LINdesktop }
        public enum PatchOpts { UPS, BPS, IPS, XD }

        static readonly List<FilePickerFileType> RADirFileTypes_win = [win_exe, FilePickerFileTypes.All];
        static readonly List<FilePickerFileType> RADirFileTypes_lin = [appimage, shellscripts, FilePickerFileTypes.All];
        static readonly List<FilePickerFileType> CONFIGDirFileTypes = [config_file, FilePickerFileTypes.TextPlain];
        static readonly List<FilePickerFileType> ICONfileTypes = [win_icon_files, win_exe, conv_icon];
        static readonly List<FilePickerFileType> ICONfileTypes2 = [lin_icon_files];
        static readonly List<FilePickerFileType> WinShorTypes = [win_lnk];
        static readonly List<FilePickerFileType> LinShorTypes = [lin_lnk];
        
        
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

                // ROM files (all files)
                case OpenOpts.RAroms:
                    options.AllowMultiple = false;
                    options.Title = resAvaloniaOps.dlgFileRAroms;
                    options.FileTypeFilter = new List<FilePickerFileType> { FilePickerFileTypes.All };
                    if (AvaloniaOps.ROMTopDir is not null)
                    { options.SuggestedStartLocation = AvaloniaOps.ROMTopDir; }
                    /*
                     * From the XDG Portal Docs:
                     * 
                     * "Suggested folder from which the files should be opened.
                     * The portal implementation is free to ignore this option."
                     *
                     * The DBus FilePicker added a way to force the start location and AvaloniaUI 11.0.6 implemented it,
                     * but is very new, and it doesn't solve GTK pickers...
                     */
                    break;

                // RetroArch config
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
            options.SuggestedStartLocation = AvaloniaOps.DesktopFolder;
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
        
        
        public static FilePickerOpenOptions PatchOpenOptions(PatchOpts patchType)
        {
            FilePickerFileType fileType = patchType switch
            {
                PatchOpts.UPS => ups_patch,
                PatchOpts.BPS => bps_patch,
                PatchOpts.IPS => ips_patch,
                PatchOpts.XD => xd_patch,
                _ => ups_patch
            };
            
            return new FilePickerOpenOptions()
            {
                AllowMultiple = false,
                Title = resAvaloniaOps.dlgFilePatch,
                FileTypeFilter = new []{fileType, FilePickerFileTypes.All}
            };
        }
    }
}
