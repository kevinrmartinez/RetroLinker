using System.Text.Json;

namespace RetroLinker.Models.Generic;

public static class JsonHelper
{
    // It is recommended to use these inside a try-catch
    
    // IL2026: Serializer and Deserializer could break due to trimming
    // But the Json libs were excluded from it, so I disabled the warning. 
#pragma warning disable IL2026
    public static string Serialize<T>(T objectToSerialize) => JsonSerializer.Serialize(objectToSerialize, typeof(T));

    public static T? Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json);
#pragma warning restore IL2026
}