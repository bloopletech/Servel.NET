using System.Security.Cryptography.X509Certificates;

namespace Servel.NET;

public readonly struct SiteConfiguration
{
    public int Id { get; }
    public string Host { get; }
    public int Port { get; }
    public X509Certificate2? Certificate { get; }
    public ServelCredentials? Credentials { get; }
    public bool AllowPublicAccess { get; }
    public IEnumerable<Listing> Listings { get; }
    public string ServerUrl => $"{(Certificate != null ? "https" : "http")}://{Host}:{Port}";

    public readonly record struct ServelCredentials(string Username, string Password);

    public SiteConfiguration(int id, SiteOptions options, string basePath)
    {
        Id = id;

        Host = options.Host ?? "*";
        var portAssigned = options.Port.HasValue;
        Port = options.Port ?? 80;

        if (options.HasCertificate)
        {
            var certPath = Path.GetFullPath(options.Cert!, basePath);
            var keyPath = Path.GetFullPath(options.Key!, basePath);
            var certificate = X509Certificate2.CreateFromPemFile(certPath, keyPath);
            Certificate = new X509Certificate2(certificate.Export(X509ContentType.Pkcs12));

            if (!portAssigned) Port = 443;
        }

        if (options.HasCredentials) Credentials = new ServelCredentials(options.Username!, options.Password!);

        AllowPublicAccess = options.AllowPublicAccess;

        Listings = options.Listings.SelectMany(l => l)
            .Select(l => new Listing(Path.GetFullPath(l.Key, basePath), l.Value));
    }
}
