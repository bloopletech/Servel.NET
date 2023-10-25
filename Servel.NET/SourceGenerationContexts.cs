using System.Text.Json.Serialization;

namespace Servel.NET;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
    GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(DirectoryEntry))]
[JsonSerializable(typeof(IEnumerable<string>))]
internal partial class SerializationSourceGenerationContext : JsonSerializerContext
{
}

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(ServelOptions))]
internal partial class ServelOptionsSourceGenerationContext : JsonSerializerContext
{
}
