using System;
using System.IO;
using System.Reflection;

namespace RetroLinker.Desktop;

public static class ResourceLoader
{
    // Manual resource navigation thanks to Denis Beurive and Arekadiusz on stackoverflow
    //https://stackoverflow.com/questions/70707947/how-to-set-a-file-as-an-embedded-resource-in-rider-intellij
    public static string[] test1(Assembly assembly) => assembly.GetManifestResourceNames();

    public static string[] GetTextFromResource(Assembly assembly, string resourceName)
    {
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) throw new Exception($"Resource not found: {resourceName}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd().Split('\n');
    }
}