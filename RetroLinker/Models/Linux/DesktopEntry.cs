using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using TG.INI;
using TG.INI.Serialization;

namespace RetroLinker.Models.Linux;

public static partial class ShortcutManager
{
    private const string MainEntry = "Desktop Entry";
    
    private class DesktopEntry : IDesktopEntry
    {
        [IniIgnoreProperty] public IReadOnlyList<string> CategoriesRo => Categories;
        [IniSection(MainEntry)][TypeConverter(typeof(DesktopEntryStringListConverter))] 
        public DesktopEntryStringList Categories { get; set; }
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
        public DesktopEntryStringList(IEnumerable<string> collection) : base(collection) { }
        public DesktopEntryStringList(int capacity) : base(capacity) { }
        
        public static implicit operator DesktopEntryStringList(string? itemsSingleString) {
            if (string.IsNullOrEmpty(itemsSingleString)) return new DesktopEntryStringList();
            var items = itemsSingleString.Split(SEPARATOR);
            return new DesktopEntryStringList(items);
        }
        
        public override string ToString() => string.Join(SEPARATOR, this);
    }
    
    public class DesktopEntryStringListConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            if (sourceType == typeof(string)) return true;
            return false;
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string stringValue) {
                DesktopEntryStringList list = stringValue;
                return list;
            }
            return null;
        }
    }
}