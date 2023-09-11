using System.Text.Json.Serialization;

namespace Servel.NET
{
    public class Entry
    {
        [JsonPropertyName("icon")]
        public required string Icon { get; init; }
        [JsonPropertyName("href")]
        public required string Href { get; init; }
        [JsonPropertyName("class")]
        public required string Class { get; init; }
        [JsonPropertyName("mediaType")]
        public string? MediaType { get; init; }
        [JsonPropertyName("name")]
        public required string Name { get; init; }
        [JsonPropertyName("type")]
        public required string Type { get; init; }
        [JsonPropertyName("size")]
        public long Size { get; init; }
        [JsonPropertyName("sizeText")]
        public required string SizeText { get; init; }
        [JsonPropertyName("mtime")]
        public long Mtime { get; init; }
        [JsonPropertyName("mtimeText")]
        public required string MtimeText { get; init; }
        [JsonPropertyName("media")]
        public bool Media { get; init; }
    }
}
