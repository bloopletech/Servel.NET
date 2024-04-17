using System.Text.Json.Serialization;

namespace Servel.NET;

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Serialization,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault)]
[JsonSerializable(typeof(DirectoryEntry))]
[JsonSerializable(typeof(IEnumerable<string>))]
internal partial class SerializationSourceGenerationContext : JsonSerializerContext
{
}

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Metadata,
    ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip,
    AllowTrailingCommas = true)]
[JsonSerializable(typeof(SiteOptions[]))]
internal partial class DeserializationSourceGenerationContext : JsonSerializerContext
{
}
