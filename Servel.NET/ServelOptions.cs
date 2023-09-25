namespace Servel.NET;

public record struct ServelOptions(
    string Host,
    int? Port,
    string? Cert,
    string? Key,
    string? Username,
    string? Password,
    IReadOnlyDictionary<string, string>[] Listings)
{
    public bool HasCertificate => Cert != null && Key != null;
    public bool HasCredentials => Username != null && Password != null;
}
