namespace Servel.NET;

public record struct SiteOptions(
    string? Host,
    int? Port,
    string? Cert,
    string? Key,
    string? Username,
    string? Password,
    Audience? Audience,
    SiteListingOption[] Listings,
    SiteDirectoryOptions[]? DirectoriesOptions)
{
    public readonly bool HasCertificate => Cert != null && Key != null;
    public readonly bool HasCredentials => Username != null && Password != null;
}

public enum Audience
{
    Localhost, LocalNetwork, Public
}

public readonly record struct SiteListingOption(string Dir, string Url, string? Name);

public readonly record struct SiteDirectoryOptions(
    string UrlPath,
    SortMethod? SortMethod,
    SortDirection? SortDirection,
    string? SearchText,
    uint? Depth,
    bool? CountChildren)
{
    public readonly bool HasParams => Depth != null || CountChildren != null;
    public readonly bool HasDefaultQuery => SortMethod != null || SortDirection != null || SearchText != null;
}

public enum SortMethod
{
    Name, Mtime, Size, Type
}

public enum SortDirection
{
    Asc, Desc
}
