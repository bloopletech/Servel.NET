namespace Servel.NET;

public readonly record struct ServelOptions(
    bool? EnableDatabase,
    string? DatabasePath,
    bool? EnableCacheDatabase,
    string? CacheDatabasePath);
