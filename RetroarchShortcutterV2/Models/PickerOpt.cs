using Avalonia.Platform.Storage;
using System.Collections.Generic;

namespace RetroarchShortcutterV2.Models
{
    public class PickerOpt
    {
        static FilePickerFileType ejecutable = new FilePickerFileType("Ejecutables") { Patterns = new string[] { "*.exe" } };
        static FilePickerFileType config_file = new FilePickerFileType("Config Files") { Patterns = new string[] { "*.cfg" } };
        static FilePickerFileType icon_files = new FilePickerFileType("Windows Icons") { Patterns = new string[] { "*.ico" } };
        static FilePickerFileType comp_icon = new FilePickerFileType("Convertible Images") { Patterns = new string[] { "*.png", "*.jpg" } };
        static FilePickerFileType win_lnk = new FilePickerFileType("Windows Shortcutr") { Patterns = new string[] { "*.lnk"} };

        static List<FilePickerFileType> RADirFileTypes { get; } = new List<FilePickerFileType> { ejecutable, FilePickerFileTypes.All };
        static List<FilePickerFileType> CONFIGDirFileTypes { get; } = new List<FilePickerFileType> { config_file, FilePickerFileTypes.TextPlain };
        static List<FilePickerFileType> ICONfileTypes { get; } = new List<FilePickerFileType> { icon_files, ejecutable, comp_icon };
        static List<FilePickerFileType> WinShorTypes { get; } = new List<FilePickerFileType> { win_lnk };


        public static FilePickerOpenOptions OpenPickerOpt(int template)
        {
            var options = new FilePickerOpenOptions();

            switch (template)
            {
                case 0:
                    options.AllowMultiple = false;
                    options.Title = "Eliga el ejecutable de RetroArch";
                    options.FileTypeFilter = RADirFileTypes;
                    break;

                case 1:
                    options.AllowMultiple = false;
                    options.Title = "Eliga la ROM que desea linkear";
                    //options.FileTypeFilter = new List<FilePickerFileType> { FilePickerFileTypes.All };
                    break;

                case 2:
                    options.AllowMultiple = false;
                    options.Title = "Eliga el Config que desea linkear";
                    options.FileTypeFilter = CONFIGDirFileTypes;
                    break;

                case 3:
                    options.AllowMultiple = false;
                    options.Title = "Eliga un archivo para como icono";
                    options.FileTypeFilter = ICONfileTypes;
                    break;

                default:
                    // Esta opcion no deberia darse
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
            switch (template) 
            {
                case 0:
                    options.Title = "Eliga donde crear el link";
                    options.DefaultExtension = "lnk";
                    options.FileTypeChoices = WinShorTypes;
                    break;
                default:
                    options.Title = "Eliga donde guardar el archivo";
                    break;
            }
            return options;
        }
    }
}
