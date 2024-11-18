using System.Text.Json.Serialization;

namespace Servel.NET;

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Serialization,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault)]
[JsonSerializable(typeof(IndexResponse))]
[JsonSerializable(typeof(IEnumerable<ListingEntry>))]
internal partial class SerializationSourceGenerationContext : JsonSerializerContext
{
}
