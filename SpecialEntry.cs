namespace Servel.NET
{
    public class SpecialEntry
    {
        public required string Name { get; init; }
        public required string Href { get; init; }
        public bool HomeEntry { get; init; }
        public bool TopEntry { get; init; }
        public bool ParentEntry { get; init; }
    }
}
