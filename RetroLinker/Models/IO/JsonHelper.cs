using System.Text.Json;
using System.Text.Json.Serialization;

namespace RetroLinker.Models.IO;

public abstract class LocalSerializable { }

public static class JsonHelper
{
    /*
     * IL2026: Serializer and Deserializer could break due to trimming
     * And it is breaking, a 'JsonTypeInfo' has to be made for every Type wish to serialize
     * Alternatives (creating and setting up a 'JsonSerializerContext') require more work
     */
    
    private static readonly JsonSerializerOptions DefaultOptions = new() {
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented =  true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        AllowOutOfOrderMetadataProperties = true,
        PropertyNameCaseInsensitive = false,
    };
    
    //public static string Serialize<T>(T objectToSerialize) => JsonSerializer.Serialize(objectToSerialize, typeof(T), DefaultOptions);
    //public static T? Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json);

    // == Settings ==
    private static readonly Settings.SettingsSerializerContext SettingsContext = new(DefaultOptions);
    
    private static string Serialize(Settings.Settings settingsToSerialize) => JsonSerializer.Serialize(settingsToSerialize, SettingsContext.Settings);
    
    private static Settings.Settings? Deserialize(string json) => JsonSerializer.Deserialize(json, SettingsContext.Settings);
    

    // Proxies to always handle De/Serialization exceptions, and other quirks
    public static string? SerializeProxy(LocalSerializable objectToSerialize)
    {
        try {
            switch (objectToSerialize)
            {
                case Settings.Settings settings:
                    return Serialize(settings);
                default:
                    throw new System.NotSupportedException("Serializer not implemented");
            }
        }
        catch (System.NotSupportedException ex) {
            Logger.LogErro($"Serialization Error:\n\tType:{objectToSerialize.GetType().Name} \n\t{ex.Message}");
            return null;
        }
    }

    public static T? DeserializeProxy<T>(string json)  where T : LocalSerializable
    {
        try
        {
            return typeof(T).Name switch
            {
                nameof(Settings.Settings) => Deserialize(json) as T,
                _ => throw new System.NotSupportedException("Deserializer not implemented")
            };
        }
        catch (System.Exception ex) when  (ex is System.NotSupportedException or JsonException) {
            Logger.LogErro($"Deserialization Error:\n\tType:{typeof(T).Name} \n\t{ex.Message}");
            return null;
        }
    }
}