using System.Text.Json.Serialization;

namespace RetroLinker.Models.IO;

internal interface ILocalSerializable;

[JsonSerializable(typeof(Settings.Settings))]
internal partial class SettingsSerializerContext : JsonSerializerContext {
    // I believe this can be left empty only because Settings uses primitives as properties
}