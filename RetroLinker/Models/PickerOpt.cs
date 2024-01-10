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

namespace RetroLinker.Models
{
    public static class PickerOpt
    {
        static readonly FilePickerFileType ejecutable = new("Ejecutables") { Patterns = new string[] { "*.exe" } };
        static readonly FilePickerFileType appimage = new("AppImage") { Patterns = new string[] { "*.AppImage" } };
        static readonly FilePickerFileType shellscripts = new("Script files") { Patterns = new string[] { "*.sh" } };
        static readonly FilePickerFileType config_file = new("Config Files") { Patterns = new string[] { "*.cfg" } };
        static readonly FilePickerFileType win_icon_files = new("Windows Icons") { Patterns = new string[] { "*.ico" } };
        static readonly FilePickerFileType conv_icon = new("Convertible Images") { Patterns = FileOps.WinConvertibleIconsExt };
        static readonly FilePickerFileType lin_icon_files = new("Icon Files") { Patterns = FileOps.LinIconsExt };
        static readonly FilePickerFileType win_lnk = new("Windows Shortcut") { Patterns = new string[] { "*.lnk" } };
        static readonly FilePickerFileType lin_lnk = new("Desktop Entry") { Patterns = new string[] { "*.desktop" } };

        public enum OpenOpts { RAexe, RAroms, RAcfg, WINico, RAout, LINico } 
        public enum SaveOpts { WINlnk, LINdesktop }

        static List<FilePickerFileType> RADirFileTypes_win = new() { ejecutable, FilePickerFileTypes.All };
        static List<FilePickerFileType> RADirFileTypes_lin = new() { appimage, shellscripts, FilePickerFileTypes.All };
        static List<FilePickerFileType> CONFIGDirFileTypes = new() { config_file, FilePickerFileTypes.TextPlain };
        static List<FilePickerFileType> ICONfileTypes = new() { win_icon_files, ejecutable, conv_icon };
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
                    options.Title = "Eliga el ejecutable de RetroArch";
                    options.FileTypeFilter = RADirFileTypes_win;
                    break;

                // All files for ROMs
                case OpenOpts.RAroms:
                    options.AllowMultiple = false;
                    options.Title = "Eliga la ROM que desea linkear";
                    //options.FileTypeFilter = new List<FilePickerFileType> { FilePickerFileTypes.All }; 
                    if (AvaloniaOps.ROMPadreDir != null)
                    { options.SuggestedStartLocation = AvaloniaOps.ROMPadreDir; }
                    // FIXME: No funciona con los FilePicker de las distros
                    break;

                // Retroarch config
                case OpenOpts.RAcfg:
                    options.AllowMultiple = false;
                    options.Title = "Eliga el archivo Config que desea especificar";
                    options.FileTypeFilter = CONFIGDirFileTypes;
                    break;

                // Windows icon
                case OpenOpts.WINico:
                    options.AllowMultiple = false;
                    options.Title = "Eliga un archivo de icono";
                    options.FileTypeFilter = ICONfileTypes;
                    break;

                // Linux executable
                case OpenOpts.RAout:
                    options.AllowMultiple = false;
                    options.Title = "Eliga el ejecutable de RetroArch";
                    options.FileTypeFilter = RADirFileTypes_lin;
                    break;

                // Linux images for icons
                case OpenOpts.LINico:
                    options.AllowMultiple = false;
                    options.Title = "Eliga un archivo de icono";
                    options.FileTypeFilter = ICONfileTypes2;
                    break;

                // Esta opcion no deberia darse
                default:
                    options.AllowMultiple = false;
                    options.Title = "Eliga un archivo";
                    break;
            }
            return options;
        }

        public static FilePickerSaveOptions SavePickerOpt(SaveOpts template)
        {
            var options = new FilePickerSaveOptions();
            options.ShowOverwritePrompt = true;
            options.Title = "Eliga donde crear el shortcut/link";
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
                
                // Esta opcion de deberia pasar
                default:
                    options.Title = "Eliga donde guardar el archivo";
                    break;
            }
            return options;
        }
    }
}
