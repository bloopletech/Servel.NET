using Servel.NET.Extensions;
using Tommy;

namespace Servel.NET;

public readonly struct ServelConfigurator(string BasePath)
{
    public ServelConfiguration Configure(string configPath)
    {
        using var inputStream = File.OpenText(configPath);
        var options = TOML.Parse(inputStream)!;

        return ConfigureServel(options);
    }

    private ServelConfiguration ConfigureServel(TomlTable options)
    {
        var siteConfigurations = ConfigureSites(options.GetRequiredArray("Site"));
        return new ServelConfiguration(siteConfigurations, ConfigureServelOptions(options), BasePath);
    }

    private static ServelOptions ConfigureServelOptions(TomlTable options)
    {
        return new ServelOptions(
            options.GetBoolean("EnableDatabase"),
            options.GetString("DatabasePath"),
            options.GetBoolean("EnableCacheDatabase"),
            options.GetString("CacheDatabasePath"));
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
            siteOptions.GetString("JwtSigningKey"),
            siteOptions.GetEnum<Audience>("Audience"),
            ConfigureRoots(siteOptions.GetRequiredArray("Directory")),
            ConfigureDirectoriesOptions(siteOptions.GetArray("DirectoriesOptions")));
    }

    private SiteRootOption[] ConfigureRoots(TomlArray directories)
    {
        return directories.OfType<TomlTable>().Select(ConfigureRoot).ToArray();
    }

    private SiteRootOption ConfigureRoot(TomlTable directory)
    {
        var key = directory.Keys.First();
        return new SiteRootOption(key, directory.GetRequiredString(key), directory.GetString("Name"));
    }

    private SiteDirectoryOptions[]? ConfigureDirectoriesOptions(TomlArray? directoriesOptions)
    {
        if(directoriesOptions == null) return null;
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
