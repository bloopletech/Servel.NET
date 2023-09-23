namespace Servel.NET
{
    public record ConfigurationYaml
    {
        public string? Host { get; set; } = "*";
        public string? Port { get; set; } = "80";
        public string? Cert { get; set; }
        public string? Key { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public required Dictionary<string, string>[] Listings { get; set; }
    }
}
