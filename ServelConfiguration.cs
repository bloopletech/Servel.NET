using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace Servel.NET
{
    public class ServelConfiguration
    {
        public IPAddress? Host { get; set; }
        public int Port { get; set; }
        public X509Certificate2? Certificate { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public IEnumerable<Listing> Listings { get; set; }
        public string ServerUrl => $"{(Certificate != null ? "https" : "http")}://{Host}:{Port}";

        public ServelConfiguration(ConfigurationYaml yaml, string yamlBasePath)
        {
            if(yaml.Host != null && yaml.Host != "*") Host = IPAddress.Parse(yaml.Host);
            Port = int.Parse(yaml.Port!);

            if(yaml.Cert != null && yaml.Key != null)
            {
                var certPath = Path.GetFullPath(yaml.Cert, yamlBasePath);
                var keyPath = Path.GetFullPath(yaml.Key, yamlBasePath);
                var certificate = X509Certificate2.CreateFromPemFile(certPath, keyPath);
                Certificate = new X509Certificate2(certificate.Export(X509ContentType.Pkcs12));
            }

            Username = yaml.Username;
            Password = yaml.Password;

            Listings = yaml.Listings.SelectMany(l => l).Select(l => new Listing(l.Key, l.Value));
        }
    }
}
