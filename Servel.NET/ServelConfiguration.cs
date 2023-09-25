using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace Servel.NET;

public class ServelConfiguration
{
    public IPAddress? Host { get; }
    public int Port { get; }
    public X509Certificate2? Certificate { get; }
    public ServelCredentials? Credentials { get; }
    public IEnumerable<Listing> Listings { get; }
    public string ServerUrl => $"{(Certificate != null ? "https" : "http")}://{Host}:{Port}";

    public readonly record struct ServelCredentials(string Username, string Password);

    public ServelConfiguration(ServelOptions options, string basePath)
    {
        if(options.Host != null) Host = IPAddress.Parse(options.Host);
        var portAssigned = options.Port.HasValue;
        Port = options.Port ?? 80;

        if(options.HasCertificate)
        {
            var certPath = Path.GetFullPath(options.Cert!, basePath);
            var keyPath = Path.GetFullPath(options.Key!, basePath);
            var certificate = X509Certificate2.CreateFromPemFile(certPath, keyPath);
            Certificate = new X509Certificate2(certificate.Export(X509ContentType.Pkcs12));

            if (!portAssigned) Port = 443;
        }

        if(options.HasCredentials) Credentials = new ServelCredentials(options.Username!, options.Password!);

        Listings = options.Listings.SelectMany(l => l)
            .Select(l => new Listing(Path.GetFullPath(l.Key, basePath), l.Value));
    }

    public static ServelConfiguration Configure()
    {
        //var yamlBasePath = Path.Combine(
        //    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        //    "Servel.NET");
        return Configure(AppContext.BaseDirectory);
    }

    public static ServelConfiguration Configure(string basePath)
    {
        var configPath = Path.Combine(basePath, "Configuration.json");
        EnsureConfigurationFile(configPath);

        using var inputStream = File.OpenRead(configPath);
        var options = JsonSerializer.Deserialize(inputStream, ServelOptionsSourceGenerationContext.Default.ServelOptions)!;

        return new ServelConfiguration(options, basePath);
    }

    private static void EnsureConfigurationFile(string configPath)
    {
        if (File.Exists(configPath)) return;

        File.WriteAllText(configPath, """
            {
              "Listings": [
                {
                    "C:\\": "/"
                }
              ]
            }
            """);
    }
}
