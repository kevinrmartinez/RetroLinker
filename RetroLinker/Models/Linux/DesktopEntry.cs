using System.Collections.Generic;
using TG.INI;
using TG.INI.Serialization;

namespace RetroLinker.Models.Linux;

public static partial class ShortcutManager
{
    private const string MainEntry = "Desktop Entry";
    
    private class DesktopEntry : IDesktopEntry
    {
        [IniIgnoreProperty] public IReadOnlyList<string> CategoriesRo => Categories;
        [IniSection(MainEntry)] public DesktopEntryStringList Categories { get; set; }
        [IniSection(MainEntry)] public string? Comment { get; set; }
        [IniSection(MainEntry)] public string Exec { get; set; }
        [IniSection(MainEntry)] public string? Icon { get; set; }
        [IniSection(MainEntry)] public string Name { get; set; }
        [IniSection(MainEntry)] public bool StartupNotify { get; set; }
        [IniSection(MainEntry)] public bool Terminal { get; set; }
        [IniSection(MainEntry)] public DesktopEntryType Type { get; set; } = DesktopEntryType.Application;
    }
    
    public enum DesktopEntryType { Application, Link, Directory }
    
    // FIXME: Constructor doesn't seem to work during Ini deserialization
    private class DesktopEntryStringList : List<string> {
        const char SEPARATOR = ';';
        
        public DesktopEntryStringList() : base() { }

        public DesktopEntryStringList(string? itemsSingleString) {
            // TODO: Test
            if (string.IsNullOrEmpty(itemsSingleString)) return;
            string[] items = itemsSingleString.Split(SEPARATOR);
            AddRange(items);
        }
        
        public override string ToString() => string.Join(SEPARATOR, this);
    }
}