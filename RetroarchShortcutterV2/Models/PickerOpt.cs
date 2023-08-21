using Avalonia.Platform.Storage;
using System.Collections.Generic;

namespace RetroarchShortcutterV2.Models
{
    public class PickerOpt
    {
        static FilePickerFileType ejecutable = new FilePickerFileType("Ejecutables") { Patterns = new string[] { "*.exe" } };
        static FilePickerFileType appimage = new FilePickerFileType("AppImage") { Patterns = new string[] { "*.AppImage" } };
        static FilePickerFileType config_file = new FilePickerFileType("Config Files") { Patterns = new string[] { "*.cfg" } };
        static FilePickerFileType icon_files = new FilePickerFileType("Windows Icons") { Patterns = new string[] { "*.ico" } };
        static FilePickerFileType comp_icon = new FilePickerFileType("Convertible Images") { Patterns = new string[] { "*.png", "*.jpg" } };
        static FilePickerFileType png_only = new FilePickerFileType("Convertible Images") { Patterns = new string[] { "*.png" } };
        static FilePickerFileType win_lnk = new FilePickerFileType("Windows Shortcut") { Patterns = new string[] { "*.lnk"} };
        static FilePickerFileType lin_lnk = new FilePickerFileType("Desktop link") { Patterns = new string[] { "*.desktop" } };

        static List<FilePickerFileType> RADirFileTypes { get; } = new List<FilePickerFileType> { ejecutable, FilePickerFileTypes.All };
        static List<FilePickerFileType> RADirFileTypes2 { get; } = new List<FilePickerFileType> { appimage, FilePickerFileTypes.All };
        static List<FilePickerFileType> CONFIGDirFileTypes { get; } = new List<FilePickerFileType> { config_file, FilePickerFileTypes.TextPlain };
        static List<FilePickerFileType> ICONfileTypes { get; } = new List<FilePickerFileType> { icon_files, ejecutable, comp_icon };
        static List<FilePickerFileType> ICONfileTypes2 { get; } = new List<FilePickerFileType> { png_only, icon_files, FilePickerFileTypes.ImageAll };
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
