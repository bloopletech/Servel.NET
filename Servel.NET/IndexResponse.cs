namespace Servel.NET;

public readonly record struct IndexResponse(
    DirectoryEntry Directory,
    ListingQuery? DefaultQuery,
    IList<HistoryEntry>? RecentEntries,
    IList<HistoryEntry>? PopularEntries);

public readonly record struct ListingQuery(string? Method, string? Direction, string? Text);