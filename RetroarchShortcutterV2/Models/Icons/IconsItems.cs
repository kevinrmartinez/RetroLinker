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

        public IconsItems() { }
        
        public IconsItems(string filePath, bool convertionRequiered) 
        {
            FilePath = filePath;
            ConvertionRequiered = convertionRequiered;
        }

        public IconsItems(string fileName, string filePath, bool convertionRequiered) : this(filePath, convertionRequiered)
        {
            FileName = fileName;
            FilePath = filePath;
        }

        public IconsItems(string? fileName, string filePath, int? comboIconInd, bool convertionRequiered) : this(fileName, filePath, convertionRequiered)
        { comboIconIndex = comboIconInd; }

        public IconsItems(string fileName, string filePath, MemoryStream? iconStream, int? comboIconInd, bool convertionRequiered) : this(fileName, filePath, comboIconInd, convertionRequiered)
        { IconStream = iconStream; }

        public override bool Equals(object? obj) => Equals(obj as IconsItems);

        public bool Equals(IconsItems? other)
        {
            return other is not null &&
                   FilePath == other.FilePath;
        }
    }
}
