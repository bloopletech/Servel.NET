using Microsoft.Extensions.FileProviders;
using Servel.NET.FileProviders;
using System.Security.Cryptography.X509Certificates;

namespace Servel.NET;

public readonly struct Site
{
    public int Id { get; }
    public string Host { get; }
    public int Port { get; }
    public X509Certificate2? Certificate { get; }
    public Credentials? Credentials { get; }
    public string? JwtSigningKey { get; }
    public Audience Audience { get; }
    public IEnumerable<Root> Roots { get; }
    public string ServerUrl => $"{(Certificate != null ? "https" : "http")}://{Host}:{Port}";
    public IEnumerable<DirectoryOptions> DirectoriesOptions { get; }

    public Site(int id, SiteOptions options, string basePath)
    {
        Id = id;

        Host = options.Host ?? "*";
        var portAssigned = options.Port.HasValue;
        Port = options.Port ?? 80;

        if(options.HasCertificate)
        {
            var certPath = Path.GetFullPath(options.Cert!, basePath);
            var keyPath = Path.GetFullPath(options.Key!, basePath);
            var certificate = X509Certificate2.CreateFromPemFile(certPath, keyPath);
            Certificate = X509CertificateLoader.LoadPkcs12(certificate.Export(X509ContentType.Pkcs12), null);

            if(!portAssigned) Port = 443;
        }

        if(options.HasCredentials) Credentials = new(options.Username!, options.Password!);
        JwtSigningKey = options.JwtSigningKey;

        Audience = options.Audience ?? Audience.LocalNetwork;

        Roots = options.Roots.Select(l => new Root(Path.GetFullPath(l.Dir, basePath), l.Url, l.Name));

        DirectoriesOptions = options.DirectoriesOptions?.Select(ConvertDirectoryOptions) ?? [];
    }

    private static DirectoryOptions ConvertDirectoryOptions(SiteDirectoryOptions options)
    {
        var indexParams = new IndexParameters(options.Depth ?? 0, options.CountChildren ?? false);

        ListingQuery? defaultQuery = null;
        if(options.HasDefaultQuery)
        {
            defaultQuery = new ListingQuery(
                options.SortMethod?.ToString().ToLowerInvariant(),
                options.SortDirection?.ToString().ToLowerInvariant(),
                options.SearchText);
        }

        return new DirectoryOptions(options.UrlPath, indexParams, defaultQuery);
    }
}

public readonly record struct Credentials(string Username, string Password);

public enum Audience
{
    Localhost, LocalNetwork, Public
}

public readonly record struct Root(string FsPath, string UrlPath, string? Name)
{
    public readonly RootFileProvider FileProvider = new(new PhysicalFileProvider(FsPath));
    public bool IsMountAtRoot => UrlPath == "/";
}

public readonly record struct DirectoryOptions(
    PathString UrlPath,
    IndexParameters DefaultParameters,
    ListingQuery? DefaultQuery);

public enum SortMethod
{
    Name, Mtime, Size, Type
}

public enum SortDirection
{
    Asc, Desc
}