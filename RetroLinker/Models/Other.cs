using System.Collections.Generic;

namespace RetroLinker.Models;

public class LocalSerializable
{
    // This can be used to serialize Settings and other Types, I'm just not sure how yet
    public string StringExample { get; } = string.Empty;
    public bool BoolExample { get; } = false;
    public int IntExample { get; } = 0;
    public double DoubleExample { get; } = 0.0;
    public byte ByteExample { get; } = 0;
    public List<string> StringListExample { get; } = new();
    
    public LocalSerializable() { }
}