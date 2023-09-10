using System;
using System.IO;

namespace RetroarchShortcutterV2.Models.Icons
{
    public class IconsItems : IEquatable<IconsItems?>
    {
        public string? FileName { get; set; }
        public string FilePath { get; set; }
        public MemoryStream? IconStream { get; set; }
        public int? comboIconIndex { get; set; }
        public bool ConvertionRequiered { get; set; }

        public IconsItems(string fileName, string filePath, bool convertionRequiered = false) 
        {
            FileName = fileName;
            FilePath = filePath;
            ConvertionRequiered = convertionRequiered;
        }

        public IconsItems(string? fileName, string filePath, int? comboIconInd, bool convertionRequiered = false) : this(fileName, filePath, convertionRequiered)
        { comboIconIndex = comboIconInd; }

        public IconsItems(string fileName, string filePath, MemoryStream? iconStream, int? comboIconInd, bool convertionRequiered = false) : this(fileName, filePath, comboIconInd, convertionRequiered)
        { IconStream = iconStream; }


        public override bool Equals(object? obj) => Equals(obj as IconsItems);

        public bool Equals(IconsItems? other) => (other != null) && (FilePath == other.FilePath);
    }
}
