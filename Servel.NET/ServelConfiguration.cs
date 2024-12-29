namespace Servel.NET;

public readonly struct ServelConfiguration
{
    public string BasePath { get; }
    public Site[] Sites { get; }
    public string? DatabasePath { get; }
    public string? CacheDatabasePath { get; }

    public ServelConfiguration(Site[] sites, ServelOptions options, string basePath)
    {
        BasePath = basePath;
        Sites = sites;
        var enableDatabase = options.EnableDatabase ?? true;
        if(enableDatabase)
        {
            DatabasePath = Path.GetFullPath(options.DatabasePath ?? "ServelDatabase.sqlite3", basePath);
        }
        var enableCacheDatabase = options.EnableCacheDatabase ?? true;
        if(enableCacheDatabase)
        {
            CacheDatabasePath = Path.GetFullPath(options.CacheDatabasePath ?? "ServelCacheDatabase.sqlite3", basePath);
        }
    }
}
