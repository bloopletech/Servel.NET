namespace Servel.NET;

public readonly struct ServelConfiguration
{
    public Site[] Sites { get; }

    public ServelConfiguration(Site[] sites)
    {
        Sites = sites;
    }
}
