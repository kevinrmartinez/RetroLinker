/*
    RetroLinker: A .NET GUI application to help create desktop links of games running on RetroArch.
    Copyright (C) 2023-2026 Kevin Rafael Martinez Johnston

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System.Text.Json;
using System.Text.Json.Serialization;

namespace RetroLinker.Models.IO;

internal static class JsonHelper
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
    private static readonly SettingsSerializerContext SettingsContext = new(DefaultOptions);
    
    private static string Serialize(Settings.Settings settingsToSerialize) => JsonSerializer.Serialize(settingsToSerialize, SettingsContext.Settings);
    
    private static Settings.Settings? DeserializeSettings(string json) => JsonSerializer.Deserialize(json, SettingsContext.Settings);
    

    // Proxies to always handle De/Serialization exceptions, and other quirks
    public static string? SerializeProxy(ILocalSerializable objectToSerialize)
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
            Logger.LogErro($"Serialization Error:\n\tType:{objectToSerialize.GetType().Name} \n\t{ex}");
            return null;
        }
    }

    public static T? DeserializeProxy<T>(string json)  where T : class, ILocalSerializable
    {
        try
        {
            var targetType = typeof(T);
            if (targetType == typeof(Settings.Settings)) return DeserializeSettings(json) as T;

            // Type Not Supported
            throw new System.NotSupportedException("Deserializer not implemented");
        }
        catch (System.Exception ex) when  (ex is System.NotSupportedException or JsonException) {
            Logger.LogErro($"Deserialization Error:\n\tType:{typeof(T).Name} \n\t{ex}");
            return null;
        }
    }
}