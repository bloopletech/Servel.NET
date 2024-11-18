namespace Servel.NET;

public readonly record struct IndexResponse(DirectoryEntry DirectoryEntry, ListingQuery? DefaultQuery);

public readonly record struct ListingQuery(string? Method, string? Direction, string? Text);