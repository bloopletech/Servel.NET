using Servel.NET.Extensions;
using Tommy;

namespace Servel.NET;

public readonly struct ServelConfigurator(string BasePath)
{
    public ServelConfiguration Configure(string configPath)
    {
        using var inputStream = File.OpenText(configPath);
        var options = TOML.Parse(inputStream)!;

        return ConfigureOptions(options);
    }

    private ServelConfiguration ConfigureOptions(TomlTable options)
    {
        var siteConfigurations = ConfigureSites(options.GetRequiredArray("Site"));
        return new ServelConfiguration(siteConfigurations);
    }

    private Site[] ConfigureSites(TomlArray sites)
    {
        return sites.OfType<TomlTable>().Select(ConfigureSite).ToArray();
    }

    private Site ConfigureSite(TomlTable siteOptions, int id)
    {
        return new Site(id, ConfigureSiteOptions(siteOptions), BasePath);
    }

    private SiteOptions ConfigureSiteOptions(TomlTable siteOptions)
    {
        return new SiteOptions(
            siteOptions.GetString("Host"),
            siteOptions.GetInteger("Port"),
            siteOptions.GetString("Cert"),
            siteOptions.GetString("Key"),
            siteOptions.GetString("Username"),
            siteOptions.GetString("Password"),
            siteOptions.GetBoolean("AllowNetworkAccess"),
            siteOptions.GetBoolean("AllowPublicAccess"),
            ConfigureListings(siteOptions.GetRequiredArray("Listing")),
            ConfigureDirectoriesOptions(siteOptions.GetArray("DirectoriesOptions")));
    }

    private SiteListingOption[] ConfigureListings(TomlArray listings)
    {
        return listings.OfType<TomlTable>().Select(ConfigureListing).ToArray();
    }

    private SiteListingOption ConfigureListing(TomlTable listing)
    {
        var key = listing.Keys.First();
        return new SiteListingOption(key, listing.GetRequiredString(key), listing.GetString("Name"));
    }

    private SiteDirectoryOptions[]? ConfigureDirectoriesOptions(TomlArray? directoriesOptions)
    {
        if (directoriesOptions == null) return null;
        return directoriesOptions.OfType<TomlTable>().Select(ConfigureDirectoryOptions).ToArray();
    }

    private SiteDirectoryOptions ConfigureDirectoryOptions(TomlTable directoryOptions)
    {
        return new SiteDirectoryOptions(
            directoryOptions.GetRequiredString("UrlPath"),
            directoryOptions.GetEnum<SortMethod>("SortMethod"),
            directoryOptions.GetEnum<SortDirection>("SortDirection"),
            directoryOptions.GetString("SearchText"),
            (uint?)directoryOptions.GetInteger("Depth"),
            directoryOptions.GetBoolean("CountChildren"));
    }
}
