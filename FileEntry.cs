using System.Web;

namespace Servel.NET
{
    public class FileEntry
    {
        public required string Name { get; init; }
        public string Href => HttpUtility.UrlPathEncode(Name);
        public long Size { get; init; }
        public long Mtime { get; init; }
        public bool Video { get; init; }
        public bool Image { get; init; }
        public bool Audio { get; init; }
        public bool Text { get; init; }
    }
}
