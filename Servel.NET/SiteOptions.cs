namespace Servel.NET;

public record struct SiteOptions(
    string Host,
    int? Port,
    string? Cert,
    string? Key,
    string? Username,
    string? Password,
    bool AllowPublicAccess,
    IReadOnlyDictionary<string, string>[] Listings)
{
    public readonly bool HasCertificate => Cert != null && Key != null;
    public readonly bool HasCredentials => Username != null && Password != null;
}
