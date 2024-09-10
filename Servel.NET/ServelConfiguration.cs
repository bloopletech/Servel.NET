using System.Web;
using Tommy;

namespace Servel.NET;

public readonly record struct ServelConfiguration(IEnumerable<SiteConfiguration> Sites)
{
    public static ServelConfiguration Configure() => Configure(AppContext.BaseDirectory);

    public static ServelConfiguration Configure(string basePath)
    {
        var configPath = Path.Combine(basePath, "Configuration.toml");
        EnsureConfigurationFile(configPath);

        using var inputStream = File.OpenText(configPath);
        var options = TOML.Parse(inputStream)!;

        return Configure(options, basePath);
    }

    private static ServelConfiguration Configure(TomlTable options, string basePath)
    {
        var sitesOptions = options.GetArray("Site")!;
        var siteConfigurations = sitesOptions.Children.Select((siteOptions, id) => new SiteConfiguration(
            id,
            ParseSiteOptions(siteOptions.AsTable!),
            basePath));
        return new ServelConfiguration(siteConfigurations.ToList());
    }

    private static SiteOptions ParseSiteOptions(TomlTable siteOptions)
    {
        return new SiteOptions(
            siteOptions.GetString("Host"),
            siteOptions.GetInteger("Port"),
            siteOptions.GetString("Cert"),
            siteOptions.GetString("Key"),
            siteOptions.GetString("Username"),
            siteOptions.GetString("Password"),
            siteOptions.GetBoolean("Host") ?? false,
            ParseListings(siteOptions.GetArray("Listing")!),
            ParseDirectoryOptions(siteOptions.GetArray("DirectoriesOptions")));
    }

    private static SiteListingOption[] ParseListings(TomlArray listings)
    {
        return listings.Children.Select(listing =>
        {
            var key = listing.Keys.First();
            return new SiteListingOption(key, listing.GetString(key)!);
        }).ToArray();
    }

    private static SiteDirectoryOptions[]? ParseDirectoryOptions(TomlArray? directoriesOptions)
    {
        if (directoriesOptions == null) return null;

        return directoriesOptions.Children.Select(directoryOptions => new SiteDirectoryOptions(
            directoryOptions.GetString("UrlPath")!,
            directoryOptions.GetEnum<SortMethod>("SortMethod"),
            directoryOptions.GetEnum<SortDirection>("SortDirection"),
            directoryOptions.GetString("SearchText"),
            (uint?)directoryOptions.GetInteger("Depth"),
            directoryOptions.GetBoolean("CountChildren"))).ToArray();
    }

    private static void EnsureConfigurationFile(string configPath)
    {
        if (File.Exists(configPath)) return;

        var password = HttpUtility.JavaScriptStringEncode(KeyGenerator.GetUniqueKey(20));
        var publicPath = Path.GetFullPath("..\\", Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments));
        var encodedPublicPath = HttpUtility.JavaScriptStringEncode(publicPath);

        var defaultConfiguration = StaticResources.Get("DefaultConfiguration.toml");
        var renderedConfiguration = string.Format(defaultConfiguration, password, encodedPublicPath);
        File.WriteAllText(configPath, renderedConfiguration);
    }
}
