using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroarchShortcutterV2.Models.Icons
{
    public class IconsItems : IEquatable<IconsItems?>
    {
        public string? FileName { get; set; }
        public string FilePath { get; set; }
        public MemoryStream? IconStream { get; set; }
        public int? comboIconIndex { get; set; }

        public IconsItems() { }
        
        public IconsItems(string filePath) { FilePath = filePath; }

        public IconsItems(string fileName, string filePath)
        {
            FileName = fileName;
            FilePath = filePath;
        }

        public IconsItems(string? fileName, string filePath, int? comboIconInd) : this(fileName, filePath)
        {
            comboIconIndex = comboIconInd;
        }

        public IconsItems(string fileName, string filePath, MemoryStream? iconStream, int? comboIconInd) : this(fileName, filePath, comboIconInd)
        {
            IconStream = iconStream;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as IconsItems);
        }

        public bool Equals(IconsItems? other)
        {
            return other is not null &&
                   FilePath == other.FilePath;
        }
    }
}
