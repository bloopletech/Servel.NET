using System.Web;

namespace Servel.NET
{
    public enum Ftype
    {
        Directory,
        File
    }

    public enum MediaType
    {
        Image,
        Video,
        Audio,
        Text
    }

    public class Entry
    {
        public required Ftype Ftype { get; init; }
        public required string Type { get; init; }
        public MediaType? MediaType { get; init; }
        public required string ListingClasses { get; init; }
        public required string Icon { get; init; }
        public required string Href { get; init; }
        public required string Name { get; init; }
        public long? Size { get; init; }
        public DateTimeOffset? Mtime { get; init; }

        public bool IsDirectory() => Ftype == Ftype.Directory;
        public bool IsFile() => Ftype == Ftype.File;

        public bool IsMedia() => MediaType != null;

        public object AsJson()
        {
            return new
            {
                icon = Icon,
                href = HttpUtility.UrlPathEncode(Href),
                classes = ListingClasses,
                mediaType = MediaType?.ToString(),
                name = Name,
                type = Type,
                size = Size ?? 0,
                sizeText = Size == null ? "-" : Size.ToString(),
                mtime = Mtime?.ToUnixTimeMilliseconds() ?? 0,
                mtimeText = Mtime == null ? "-" : Mtime.Value.ToString("d MMM yyyy h:mm tt"),
                media = IsMedia()
            };
        }
    }
}
