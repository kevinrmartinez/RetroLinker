using Avalonia.Platform.Storage;
using System.Collections.Generic;

namespace RetroarchShortcutterV2.Models
{
    public class PickerOpt
    {
        static readonly FilePickerFileType ejecutable = new("Ejecutables") { Patterns = new string[] { "*.exe" } };
        static readonly FilePickerFileType appimage = new("AppImage") { Patterns = new string[] { "*.AppImage" } };
        static readonly FilePickerFileType config_file = new("Config Files") { Patterns = new string[] { "*.cfg" } };
        static readonly FilePickerFileType win_icon_files = new("Windows Icons") { Patterns = new string[] { "*.ico" } };
        static readonly FilePickerFileType comp_icon = new("Convertible Images") { Patterns = new string[] { "*.png", "*.jpg" } };
        static readonly FilePickerFileType lin_icon_files = new("Icon Files") { Patterns = new string[] { "*.ico", "*.png", "*.xpm", "*.svg", "*.svgz" } };
        static readonly FilePickerFileType win_lnk = new("Windows Shortcut") { Patterns = new string[] { "*.lnk"} };
        static readonly FilePickerFileType lin_lnk = new("Desktop link") { Patterns = new string[] { "*.desktop" } };

        static List<FilePickerFileType> RADirFileTypes { get; } = new List<FilePickerFileType> { ejecutable, FilePickerFileTypes.All };
        static List<FilePickerFileType> RADirFileTypes2 { get; } = new List<FilePickerFileType> { appimage, FilePickerFileTypes.All };
        static List<FilePickerFileType> CONFIGDirFileTypes { get; } = new List<FilePickerFileType> { config_file, FilePickerFileTypes.TextPlain };
        static List<FilePickerFileType> ICONfileTypes { get; } = new List<FilePickerFileType> { win_icon_files, ejecutable, comp_icon };
        static List<FilePickerFileType> ICONfileTypes2 { get; } = new List<FilePickerFileType> { lin_icon_files };
        static List<FilePickerFileType> WinShorTypes { get; } = new List<FilePickerFileType> { win_lnk };
        static List<FilePickerFileType> LinShorTypes { get; } = new List<FilePickerFileType> { lin_lnk };


        public static FilePickerOpenOptions OpenPickerOpt(int template)
        {
            var options = new FilePickerOpenOptions();

            switch (template)
            {
                // Windows exe (retroarch.exe)
                case 0:
                    options.AllowMultiple = false;
                    options.Title = "Eliga el ejecutable de RetroArch";
                    options.FileTypeFilter = RADirFileTypes;
                    break;

                // All files for ROMs
                case 1:
                    options.AllowMultiple = false;
                    options.Title = "Eliga la ROM que desea linkear";
                    //options.FileTypeFilter = new List<FilePickerFileType> { FilePickerFileTypes.All };
                    break;

                // Retroarch config
                case 2:
                    options.AllowMultiple = false;
                    options.Title = "Eliga el Config que desea linkear";
                    options.FileTypeFilter = CONFIGDirFileTypes;
                    break;

                // Windows icon
                case 3:
                    options.AllowMultiple = false;
                    options.Title = "Eliga un archivo para como icono";
                    options.FileTypeFilter = ICONfileTypes;
                    break;

                // Linux AppImage
                case 4:
                    options.AllowMultiple = false;
                    options.Title = "Eliga el ejecutable de RetroArch";
                    options.FileTypeFilter = RADirFileTypes2;
                    break;

                // Linux images for icons
                case 5:
                    options.AllowMultiple = false;
                    options.Title = "Eliga un archivo para como icono";
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

        public static FilePickerSaveOptions SavePickerOpt(int template)
        {
            var options = new FilePickerSaveOptions();
            options.ShowOverwritePrompt = true;
            options.Title = "Eliga donde crear el shortcut/link";
            switch (template) 
            {
                // Windows .lnk
                case 0:                 
                    options.DefaultExtension = "lnk";
                    options.FileTypeChoices = WinShorTypes;
                    break;

                // Linux popular .desktop
                case 1:
                    options.DefaultExtension = "desktop";
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
