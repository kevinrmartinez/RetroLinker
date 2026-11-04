namespace RetroLinker.Models.IO;

public class IconItem (string filePath, bool conversionRequired = false) : IIconItem
{
    public string FilePath { get; } = filePath;
    public bool ConversionRequired { get; } = conversionRequired;
}