using System.Text.Json.Serialization;

namespace Servel.NET;

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Serialization,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault)]
[JsonSerializable(typeof(IndexResponse))]
[JsonSerializable(typeof(IEnumerable<RootEntry>))]
internal partial class SerializationSourceGenerationContext : JsonSerializerContext
{
}
