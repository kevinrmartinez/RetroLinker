using System.Text.Json;
using System.Text.Json.Serialization;

namespace RetroLinker.Models.Generic;

public static class JsonHelper
{
    private static readonly JsonSerializerOptions Options = new() {
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented =  true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };
    
    // It is recommended to use these inside a try-catch
    // IL2026: Serializer and Deserializer could break due to trimming
    // But the Json libs were excluded from it, so I disabled the warning. 
#pragma warning disable IL2026
    public static string Serialize<T>(T objectToSerialize) => JsonSerializer.Serialize(objectToSerialize, typeof(T), Options);

    public static T? Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json);
#pragma warning restore IL2026
}