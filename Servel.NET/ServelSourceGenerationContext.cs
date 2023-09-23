using System.Text.Json.Serialization;

namespace Servel.NET;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
    GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(DirectoryEntry))]
internal partial class ServelSourceGenerationContext : JsonSerializerContext
{
}
