namespace Servel.NET;

public readonly struct ServelConfiguration
{
    public Site[] Sites { get; }
    public string? DatabasePath { get; }

    public ServelConfiguration(Site[] sites, ServelOptions options, string basePath)
    {
        Sites = sites;
        var enableDatabase = options.EnableDatabase ?? true;
        if(enableDatabase) DatabasePath = Path.GetFullPath(options.DatabasePath ?? "Database.sqlite3", basePath);
    }
}
