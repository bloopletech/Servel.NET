using System.Web;

namespace Servel.NET
{
    public class DirectoryEntry
    {
        public required string Name { get; init; }
        public string Href => HttpUtility.UrlPathEncode(Name + "/");
        public long Mtime { get; init; }
        public bool HomeEntry { get; init; }
        public bool TopEntry { get; init; }
        public bool ParentEntry { get; init; }
        public IEnumerable<DirectoryEntry>? Directories { get; init; }
        public IEnumerable<FileEntry>? Files { get; init; }
        public IEnumerable<SpecialEntry>? SpecialEntries { get; init; }
    }
}
