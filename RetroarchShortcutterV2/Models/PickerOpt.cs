using Avalonia.Platform.Storage;
using System.Collections.Generic;

namespace RetroarchShortcutterV2.Models
{
    public class PickerOpt
    {

        //public static FilePickerFileType Executable { get; } = new("Executables")
        //{
        //    Patterns = new[] { "*.exe" }
        //};

        
        public static FilePickerOpenOptions OpenPickerOpt(int template)
        {
            var options = new FilePickerOpenOptions();

            switch (template)
            {
                case 0:
                    options.AllowMultiple = false;
                    options.Title = "Eliga el ejecutable de RetroArch";
                    options.FileTypeFilter = RADirFileTypes2;
                    break;

                case 1:
                    options.AllowMultiple = false;
                    options.Title = "Eliga la ROM que desea linkear";
                    //options.FileTypeFilter = new List<FilePickerFileType> { FilePickerFileTypes.All };
                    break;

                case 2:
                    options.AllowMultiple = false;
                    options.Title = "Eliga el Config que desea vincular";
                    //options.FileTypeFilter = CONFIGDirFileTypes;
                    break;

                case 3:
                    options.AllowMultiple = false;
                    options.Title = "Eliga un archivo para como icono";
                    options.FileTypeFilter = ICONfileTypes2;
                    break;

                default:
                    // Esta opcion no deberia darse
                    options.AllowMultiple = false;
                    options.Title = "Eliga un archivo";
                    break;
            }

            return options;
        }


        static FilePickerFileType ejecutable = new FilePickerFileType("Ejecutables") { Patterns = new string[] { "*.exe" } };
        static FilePickerFileType config_file = new FilePickerFileType("Config Files") { Patterns = new string[] { "*.cfg" } };
        static FilePickerFileType icon_files = new FilePickerFileType("Windows Icons") { Patterns = new string[] { "*.ico" } };
        static FilePickerFileType comp_icon = new FilePickerFileType("Convertible Images") { Patterns = new string[] { "*.png", "*.jpg" } };

        public static List<FilePickerFileType> RADirFileTypes2 { get; } = new List<FilePickerFileType> { ejecutable, FilePickerFileTypes.All };

        //public static List<FilePickerFileType> RADirFileTypes
        //{
        //    set
        //    {
        //        RADirFileTypes.Add(ejecutable);
        //        RADirFileTypes.Add(FilePickerFileTypes.All);
        //    }
        //    get { return RADirFileTypes; }
        //}

        //public static List<FilePickerFileType> CONFIGDirFileTypes
        //{
        //    set
        //    {
        //        CONFIGDirFileTypes.Add(config_file);
        //        CONFIGDirFileTypes.Add(FilePickerFileTypes.All);
        //    }
        //    get { return CONFIGDirFileTypes; }
        //}

        //public static List<FilePickerFileType> ICONfileTypes
        //{
        //    set
        //    {
        //        ICONfileTypes.Add(icon_files);
        //        ICONfileTypes.Add(ejecutable);
        //        ICONfileTypes.Add(comp_icon);
        //    }
        //    get { return ICONfileTypes; }
        //}

        public static List<FilePickerFileType> ICONfileTypes2 { get; } = new List<FilePickerFileType> { icon_files, ejecutable, comp_icon };
    }
}
