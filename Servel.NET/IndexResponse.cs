namespace Servel.NET;

public readonly record struct IndexResponse(DirectoryEntry Directory, ListingQuery? DefaultQuery);

public readonly record struct ListingQuery(string? Method, string? Direction, string? Text);