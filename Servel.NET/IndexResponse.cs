using System.Text.Json.Serialization;

namespace Servel.NET;

public readonly record struct IndexResponse(
    DirectoryEntry Directory,
    ListingQuery? DefaultQuery,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.Never)] ListingConfiguration Configuration,
    IList<HistoryEntry>? RecentEntries,
    IList<HistoryEntry>? PopularEntries);

public readonly record struct ListingQuery(string? Method, string? Direction, string? Text);

public readonly record struct ListingConfiguration(bool ThumbnailsEnabled);