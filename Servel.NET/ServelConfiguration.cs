using System.Net;
using System.Security.Cryptography.X509Certificates;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

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

        public static ServelConfiguration Parse()
        {
            var yamlBasePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Servel.NET");
            var yamlPath = Path.Combine(yamlBasePath, "servel.yml");

            var yamlDeserializer = new DeserializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();
            var configurationYaml = yamlDeserializer.Deserialize<ConfigurationYaml>(File.ReadAllText(yamlPath));

            return new ServelConfiguration(configurationYaml, yamlBasePath);
        }
    }
}
